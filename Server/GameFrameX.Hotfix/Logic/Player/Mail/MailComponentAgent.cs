// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using System.Collections.Generic;
using GameFrameX.Apps.Common.Session;
using GameFrameX.Apps.Player.Mail;
using GameFrameX.Apps.Player.Mail.Component;
using GameFrameX.Apps.Player.Mail.Entity;
using GameFrameX.Apps.Player.Reward;
using GameFrameX.Apps.Player.Reward.Entity;
using GameFrameX.Core.Abstractions.Attribute;
using GameFrameX.Foundation.Utility;
using GameFrameX.Hotfix.Logic.Player.Login;
using GameFrameX.Hotfix.Logic.Player.Reward;

namespace GameFrameX.Hotfix.Logic.Player.Mail;

/// <summary>
/// 玩家邮件组件代理。承载邮件懒创建、列表、读信、删除、过期清理、撤回作废与变更通知。
/// </summary>
/// <remarks>
/// 不可逆边界（U1 §4.4）：
/// <list type="bullet">
/// <item>B2 删除：仅当附件全部已领或无附件或不可领时允许删除；否则拒绝 <c>UnclaimedAttachment</c>。</item>
/// <item>B3 撤回：已 <see cref="ClaimStatus.Claimed"/> 的附件与资产不变，仅未领附件置 <see cref="ClaimStatus.Discarded"/>。</item>
/// <item>B4 过期：按 Campaign <c>ExpireAttachmentPolicy</c> 处理，默认 <see cref="ExpireAttachmentPolicy.DiscardUnclaimed"/>。</item>
/// <item>B5 懒创建幂等：同一 <c>CampaignId@Version</c> 仅实例化一次；撤回 Campaign 记入 <c>CreatedCampaignVersions</c> 防重实例化。</item>
/// </list>
/// <see cref="ReadStatus"/> / <see cref="AttachmentStatus"/> 为派生展示字段，单一事实来源为 <see cref="MailState.MailStatus"/> + 各附件 <see cref="MailAttachmentInstance.ClaimStatus"/> + <see cref="MailState.ReadTime"/>，由本代理集中维护，禁止外部独立写入（U1 §4.3）。
/// </remarks>
public class MailComponentAgent : StateComponentAgent<MailComponent, MailBoxState>
{
    /// <summary>
    /// 邮件懒同步（U1 §4.5 + §4.6）。触发点：登录完成后、列表拉取前、显式同步。
    /// </summary>
    /// <remarks>
    /// 先按 <c>LastSyncCampaignTime</c> 增量扫描新发布 Campaign 做懒创建（B5），再扫描已撤回 Campaign 对已实例化邮件执行 B3 作废。变更后持久化并推送 <c>NotifyMailChanged</c>。
    /// </remarks>
    [Service]
    public virtual Task SyncAsync()
    {
        return SyncAsyncImpl();
    }

    private async Task SyncAsyncImpl()
    {
        var (playerLevel, playerCreatedTime) = await GetPlayerFilterContext();
        var serverId = GameServerConst.Game.Id;
        var state = OwnerComponent.State;
        var changedMailIds = new List<long>();
        var changed = false;

        // 1. 懒创建（U1 §4.5）：增量扫描新发布 Campaign。
        var campaigns = MailCampaignRegistry.QueryPublishedSince(state.LastSyncCampaignTime);
        long maxPublishedAt = state.LastSyncCampaignTime;
        foreach (var campaign in campaigns)
        {
            if (campaign.PublishedAt > maxPublishedAt)
            {
                maxPublishedAt = campaign.PublishedAt;
            }

            var key = MailCampaignRegistry.BuildKey(campaign.CampaignId, campaign.PublishVersion);
            if (state.CreatedCampaignVersions.Contains(key))
            {
                continue; // 幂等（B5）
            }

            if (campaign.Status == MailCampaignStatus.Revoked)
            {
                // 撤回 Campaign 防重实例化（B5）：记 key 永久跳过。
                state.CreatedCampaignVersions.Add(key);
                changed = true;
                continue;
            }

            if (campaign.ChannelIds != null && campaign.ChannelIds.Count > 0)
            {
                // 渠道条件未补齐：不实例化也不记 key，待后续补齐后再判断（U1 §4.5）。
                continue;
            }

            if (!MailCampaignFilter.Match(campaign, serverId, playerLevel, playerCreatedTime))
            {
                continue; // 命中失败：不记 key（本 since 游标推进后不再重评估）
            }

            var mail = Instantiate(campaign);
            state.List.Add(mail);
            state.CreatedCampaignVersions.Add(key);
            if (mail.ReadStatus == ReadStatus.Unread)
            {
                state.UnreadCount++;
            }

            changedMailIds.Add(mail.MailId);
            changed = true;
        }

        state.LastSyncCampaignTime = maxPublishedAt;

        // 2. 撤回作废（U1 §4.6 / B3）：扫描已撤回 Campaign，对已实例化邮件作废未领附件。
        foreach (var revoked in MailCampaignRegistry.GetRevokedCampaigns())
        {
            if (ApplyRevokeToMailbox(state, revoked.CampaignId, revoked.PublishVersion, changedMailIds))
            {
                changed = true;
            }
        }

        if (changed)
        {
            await OwnerComponent.WriteStateAsync();
            await NotifyChangedAsync(changedMailIds);
        }
    }

    /// <summary>
    /// 处理 <see cref="ReqMailList"/>：先懒同步，再分页返回邮件摘要（排除已删除）。
    /// </summary>
    [Service]
    public virtual Task OnReqMailListAsync(INetWorkChannel netWorkChannel, ReqMailList request, RespMailList response)
    {
        return OnReqMailListAsyncImpl(netWorkChannel, request, response);
    }

    private async Task OnReqMailListAsyncImpl(INetWorkChannel netWorkChannel, ReqMailList request, RespMailList response)
    {
        await SyncAsync();

        var state = OwnerComponent.State;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var cursor = request.Cursor < 0 ? 0 : request.Cursor;

        var visible = new List<MailState>();
        for (var i = state.List.Count - 1; i >= 0; i--)
        {
            // 倒序（新邮件在前），排除已删除。
            var mail = state.List[i];
            if (mail.MailStatus == MailStatus.Deleted)
            {
                continue;
            }

            visible.Add(mail);
        }

        response.UnreadCount = state.UnreadCount;
        for (var i = cursor; i < visible.Count && response.Mails.Count < pageSize; i++)
        {
            response.Mails.Add(BuildMailInfo(visible[i]));
        }

        response.HasMore = cursor + response.Mails.Count < visible.Count;
    }

    /// <summary>
    /// 处理 <see cref="ReqMailRead"/>：标记已读（读信幂等），返回完整文案与附件领取状态。
    /// </summary>
    [Service]
    public virtual Task OnReqMailReadAsync(INetWorkChannel netWorkChannel, ReqMailRead request, RespMailRead response)
    {
        return OnReqMailReadAsyncImpl(netWorkChannel, request, response);
    }

    private async Task OnReqMailReadAsyncImpl(INetWorkChannel netWorkChannel, ReqMailRead request, RespMailRead response)
    {
        var state = OwnerComponent.State;
        var mail = TryFindMail(state, request.MailId);
        if (mail == null || mail.MailStatus == MailStatus.Deleted)
        {
            response.MailId = request.MailId;
            response.ErrorCode = (int)MailErrorCode.MailNotFound;
            return;
        }

        if (mail.ReadStatus == ReadStatus.Unread)
        {
            mail.ReadStatus = ReadStatus.Read;
            mail.ReadTime = TimerHelper.UnixTimeSeconds();
            if (state.UnreadCount > 0)
            {
                state.UnreadCount--;
            }

            // 读信驱动主线状态：Unread → Read（其它状态不动）。
            if (mail.MailStatus == MailStatus.Unread)
            {
                mail.MailStatus = MailStatus.Read;
            }

            await OwnerComponent.WriteStateAsync();
        }

        response.MailId = mail.MailId;
        response.Title = mail.Title;
        response.Content = mail.Content;
        response.TemplateId = mail.TemplateId;
        response.TemplateVersion = mail.TemplateVersion;
        response.ReadStatus = (int)mail.ReadStatus;
        response.MailStatus = (int)mail.MailStatus;
        foreach (var att in mail.Attachments)
        {
            response.Attachments.Add(new MailAttachmentInfo
            {
                SlotId = att.SlotId,
                RewardType = att.RewardType,
                ItemId = att.ItemId,
                Count = att.Amount,
                ClaimStatus = (int)att.ClaimStatus,
            });
        }

    }

    /// <summary>
    /// 处理 <see cref="ReqMailDelete"/>：U1 §4.8 删除算法（B2）。未领附件邮件拒绝删除。
    /// </summary>
    [Service]
    public virtual Task OnReqMailDeleteAsync(INetWorkChannel netWorkChannel, ReqMailDelete request, RespMailDelete response)
    {
        return OnReqMailDeleteAsyncImpl(netWorkChannel, request, response);
    }

    private async Task OnReqMailDeleteAsyncImpl(INetWorkChannel netWorkChannel, ReqMailDelete request, RespMailDelete response)
    {
        var state = OwnerComponent.State;
        var mail = TryFindMail(state, request.MailId);
        if (mail == null)
        {
            response.MailId = request.MailId;
            response.ErrorCode = (int)MailErrorCode.MailNotFound;
            return;
        }

        if (mail.MailStatus == MailStatus.Deleted)
        {
            response.MailId = request.MailId;
            response.ErrorCode = (int)MailErrorCode.MailAlreadyDeleted;
            return;
        }

        RecomputeAttachmentStatus(mail);
        // B2：仅当无附件 / 全部已领 / 不可领时允许删除。
        if (mail.AttachmentStatus != AttachmentStatus.NoAttachment
            && mail.AttachmentStatus != AttachmentStatus.AllClaimed
            && mail.AttachmentStatus != AttachmentStatus.Unclaimable)
        {
            response.MailId = request.MailId;
            response.ErrorCode = (int)MailErrorCode.UnclaimedAttachment;
            return;
        }

        mail.MailStatus = MailStatus.Deleted;
        mail.DeleteTime = TimerHelper.UnixTimeSeconds();
        await OwnerComponent.WriteStateAsync();

        response.MailId = request.MailId;
        await NotifyChangedAsync(new List<long> { mail.MailId });
    }

    /// <summary>
    /// 处理 <see cref="ReqMailClaimAttachment"/>：领取单个邮件附件。经统一发放接口 <see cref="RewardGrantComponentAgent.GrantAsync"/> 入账（B6 幂等），
    /// 成功后置 <see cref="ClaimStatus.Claimed"/> 终态；失败保留 <see cref="ClaimStatus.Claimable"/> 供重试。
    /// </summary>
    /// <remarks>不触发 <see cref="SyncAsync"/>：领取作用于当前邮件箱快照，懒同步由列表 / 读信路径保证。幂等命中已领槽位时按成功回包，不重复发奖。</remarks>
    [Service]
    public virtual Task OnReqMailClaimAttachmentAsync(INetWorkChannel netWorkChannel, ReqMailClaimAttachment request, RespMailClaimAttachment response)
    {
        return OnReqMailClaimAttachmentAsyncImpl(netWorkChannel, request, response);
    }

    private async Task OnReqMailClaimAttachmentAsyncImpl(INetWorkChannel netWorkChannel, ReqMailClaimAttachment request, RespMailClaimAttachment response)
    {
        var state = OwnerComponent.State;
        var prepare = MailAttachmentClaim.Prepare(state, request.MailId, request.SlotId);

        response.MailId = request.MailId;
        response.SlotId = request.SlotId;

        // 邮件不存在 / 已删除
        if (prepare.Mail == null)
        {
            response.ErrorCode = (int)MailErrorCode.MailNotFound;
            return;
        }

        // 槽位不存在
        if (prepare.Attachment == null)
        {
            response.ErrorCode = (int)MailErrorCode.AttachmentNotFound;
            return;
        }

        // B6 幂等：已领槽位按成功回包当前状态，不调用发放接口（不赋 ErrorCode，框架默认 0）。
        if (prepare.Attachment.ClaimStatus == ClaimStatus.Claimed)
        {
            FillClaimResponse(response, prepare.Mail, prepare.Attachment);
            return;
        }

        // 作废 / 其它非可领状态：不可领取
        if (prepare.Attachment.ClaimStatus != ClaimStatus.Claimable)
        {
            response.ErrorCode = (int)MailErrorCode.UnclaimableAttachment;
            return;
        }

        var mail = prepare.Mail;
        var att = prepare.Attachment;
        var now = TimerHelper.UnixTimeSeconds();

        var grantResult = await GrantAttachmentAsync(mail, att);

        if (grantResult.AllSuccess)
        {
            MailAttachmentClaim.MarkClaimed(att, now);
        }

        mail.AttachmentStatus = MailAttachmentClaim.ComputeAttachmentStatus(mail);
        await OwnerComponent.WriteStateAsync();

        FillClaimResponse(response, mail, att);
        // 成功不赋 ErrorCode（默认 0）；奖励发放部分失败时透传发放错误码（OperationStatusCode 体系）。
        if (!grantResult.AllSuccess)
        {
            response.ErrorCode = grantResult.ErrorCode;
        }
        await NotifyChangedAsync(new List<long> { mail.MailId });
    }

    /// <summary>
    /// 处理 <see cref="ReqMailClaimAllAttachment"/>：一键领取当前邮件箱所有 <see cref="ClaimStatus.Claimable"/> 槽位。逐项独立判定，部分失败不影响其余项。
    /// </summary>
    [Service]
    public virtual Task OnReqMailClaimAllAttachmentAsync(INetWorkChannel netWorkChannel, ReqMailClaimAllAttachment request, RespMailClaimAllAttachment response)
    {
        return OnReqMailClaimAllAttachmentAsyncImpl(netWorkChannel, request, response);
    }

    private async Task OnReqMailClaimAllAttachmentAsyncImpl(INetWorkChannel netWorkChannel, ReqMailClaimAllAttachment request, RespMailClaimAllAttachment response)
    {
        var state = OwnerComponent.State;
        var changedMailIds = new List<long>();
        var now = TimerHelper.UnixTimeSeconds();

        foreach (var mail in state.List)
        {
            if (mail.MailStatus == MailStatus.Deleted)
            {
                continue;
            }

            foreach (var att in mail.Attachments)
            {
                if (att.ClaimStatus != ClaimStatus.Claimable)
                {
                    continue;
                }

                var grantResult = await GrantAttachmentAsync(mail, att);
                var success = grantResult.AllSuccess;
                if (success)
                {
                    MailAttachmentClaim.MarkClaimed(att, now);
                }

                mail.AttachmentStatus = MailAttachmentClaim.ComputeAttachmentStatus(mail);

                response.Slots.Add(new MailClaimedSlot
                {
                    MailId = mail.MailId,
                    SlotId = att.SlotId,
                    RewardType = att.RewardType,
                    ItemId = att.ItemId,
                    Count = att.Amount,
                    ClaimStatus = (int)att.ClaimStatus,
                    Success = success,
                });

                if (success)
                {
                    response.ClaimedCount++;
                }

                if (!changedMailIds.Contains(mail.MailId))
                {
                    changedMailIds.Add(mail.MailId);
                }
            }
        }

        if (changedMailIds.Count > 0)
        {
            await OwnerComponent.WriteStateAsync();
            await NotifyChangedAsync(changedMailIds);
        }

    }

    /// <summary>
    /// 调用统一发放接口发放单个附件。幂等键 <c>RoleId:SourceType=Mail:SourceId:TraceId</c>，trace 为 <c>CampaignId:Version:MailId:SlotId</c> 四元组（B6）。
    /// </summary>
    private async Task<RewardGrantResult> GrantAttachmentAsync(MailState mail, MailAttachmentInstance att)
    {
        var rewardAgent = await ActorManager.GetComponentAgent<RewardGrantComponentAgent>();
        return await rewardAgent.GrantAsync(new RewardGrantRequest
        {
            RoleId = ActorId,
            SourceType = RewardSourceType.Mail,
            SourceId = "mail:" + mail.MailId + ":slot:" + att.SlotId,
            TraceId = MailAttachmentClaim.BuildTrace(mail.CampaignId, mail.CampaignVersion, mail.MailId, att.SlotId),
            Rewards = new List<RewardItem>
            {
                new RewardItem { RewardType = (RewardType)att.RewardType, ItemId = att.ItemId, Count = att.Amount },
            },
        });
    }

    /// <summary>
    /// 填充单领响应的奖励元信息与领取后邮件 / 附件状态。
    /// </summary>
    private static void FillClaimResponse(RespMailClaimAttachment response, MailState mail, MailAttachmentInstance att)
    {
        response.RewardType = att.RewardType;
        response.ItemId = att.ItemId;
        response.Count = att.Amount;
        response.ClaimStatus = (int)att.ClaimStatus;
        response.MailStatus = (int)mail.MailStatus;
        response.AttachmentStatus = (int)mail.AttachmentStatus;
    }

    /// <summary>
    /// 过期清理（U1 §4.7 / B4）。遍历邮件箱，按 Campaign <c>ExpireAttachmentPolicy</c> 处理到期邮件。
    /// </summary>
    [Service]
    public virtual Task ExpireSweepAsync()
    {
        return ExpireSweepAsyncImpl();
    }

    private async Task ExpireSweepAsyncImpl()
    {
        var state = OwnerComponent.State;
        var now = TimerHelper.UnixTimeSeconds();
        var changedMailIds = new List<long>();
        var changed = false;

        foreach (var mail in state.List)
        {
            if (mail.MailStatus == MailStatus.Deleted || mail.MailStatus == MailStatus.Revoked)
            {
                continue;
            }

            if (mail.ExpireTime > now)
            {
                continue;
            }

            var policy = ResolveExpirePolicy(mail);
            switch (policy)
            {
                case ExpireAttachmentPolicy.DiscardUnclaimed:
                    foreach (var att in mail.Attachments)
                    {
                        if (att.ClaimStatus == ClaimStatus.Claimable)
                        {
                            att.ClaimStatus = ClaimStatus.Discarded;
                        }
                    }

                    break;
                case ExpireAttachmentPolicy.KeepUnclaimed:
                    // 保留待领取：附件仍可领，仅邮件视图过期。
                    break;
                case ExpireAttachmentPolicy.AutoClaim:
                    // 预留：自动领取未领附件，需显式配置；本 change 不启用。
                    break;
            }

            mail.MailStatus = MailStatus.Expired;
            RecomputeAttachmentStatus(mail);
            changedMailIds.Add(mail.MailId);
            changed = true;
        }

        state.LastCleanupTime = now;

        if (changed)
        {
            await OwnerComponent.WriteStateAsync();
            await NotifyChangedAsync(changedMailIds);
        }
    }

    /// <summary>
    /// 对当前邮件箱应用 Campaign 撤回（U1 §4.6 / B3）。已 <see cref="ClaimStatus.Claimed"/> 的附件与资产不变；未领附件置 <see cref="ClaimStatus.Discarded"/>。
    /// </summary>
    /// <remarks>幂等：重复调用对已处理邮件无副作用。通常由 <see cref="SyncAsync"/> 扫描已撤回 Campaign 时触发；本方法也可供撤回推送路径直接调用。</remarks>
    [Service]
    public virtual Task ApplyRevokeAsync(long campaignId, int campaignVersion)
    {
        return ApplyRevokeAsyncImpl(campaignId, campaignVersion);
    }

    private async Task ApplyRevokeAsyncImpl(long campaignId, int campaignVersion)
    {
        var state = OwnerComponent.State;
        var changedMailIds = new List<long>();
        var changed = ApplyRevokeToMailbox(state, campaignId, campaignVersion, changedMailIds);

        if (changed)
        {
            await OwnerComponent.WriteStateAsync();
            await NotifyChangedAsync(changedMailIds);
        }
    }

    /// <summary>
    /// 推送 <see cref="NotifyMailChanged"/>。离线玩家不推（下次登录 / 拉列表触发同步）。
    /// </summary>
    private async Task NotifyChangedAsync(List<long> changedMailIds)
    {
        if (changedMailIds == null || changedMailIds.Count == 0)
        {
            return;
        }

        var channel = SessionManager.GetByRoleId(ActorId)?.WorkChannel;
        if (channel == null)
        {
            return;
        }

        var notify = new NotifyMailChanged
        {
            UnreadCount = OwnerComponent.State.UnreadCount,
        };
        notify.ChangedMailIds.AddRange(changedMailIds);
        await channel.WriteAsync(notify);
    }

    /// <summary>
    /// 读取玩家筛选上下文（等级、创建时间）。从同 Actor 的 <see cref="PlayerComponentAgent"/> 状态读取。
    /// </summary>
    private async Task<(long level, long createdTime)> GetPlayerFilterContext()
    {
        var playerAgent = await ActorManager.GetComponentAgent<PlayerComponentAgent>();
        var playerState = playerAgent.OwnerComponent.State;
        return (playerState.Level, playerState.CreatedTime);
    }

    /// <summary>
    /// 实例化玩家邮件（U1 §4.5 <c>Instantiate</c>）。渲染文案、计算 <see cref="MailState.ExpireTime"/>、复制附件元信息（不含领取状态）。
    /// </summary>
    private MailState Instantiate(MailCampaignState campaign)
    {
        var state = OwnerComponent.State;
        var now = TimerHelper.UnixTimeSeconds();
        PickLocalized(campaign, out var title, out var content);

        var mail = new MailState
        {
            MailId = ++state.MailIdSeq,
            CampaignId = campaign.CampaignId,
            CampaignVersion = campaign.PublishVersion,
            MailType = campaign.MailType,
            Title = title,
            Content = content,
            TemplateId = (int)campaign.TemplateId,
            CreateTime = now,
            ExpireTime = ResolveExpireTime(campaign, now),
            MailStatus = MailStatus.Unread,
            ReadStatus = ReadStatus.Unread,
        };

        foreach (var att in campaign.Attachments)
        {
            mail.Attachments.Add(new MailAttachmentInstance
            {
                SlotId = att.SlotId,
                RewardType = att.RewardType,
                ItemId = att.ItemId,
                Amount = att.Amount,
                IconId = att.IconId,
                ClaimStatus = ClaimStatus.Claimable,
            });
        }

        RecomputeAttachmentStatus(mail);
        return mail;
    }

    /// <summary>
    /// 对邮件箱执行撤回作废（B3）。返回是否有变更。
    /// </summary>
    private bool ApplyRevokeToMailbox(MailBoxState state, long campaignId, int campaignVersion, List<long> changedMailIds)
    {
        var changed = false;
        foreach (var mail in state.List)
        {
            if (mail.CampaignId != campaignId || mail.CampaignVersion != campaignVersion)
            {
                continue;
            }

            if (mail.MailStatus == MailStatus.Deleted || mail.MailStatus == MailStatus.Expired)
            {
                continue; // 终态不动
            }

            var hasUnclaimed = false;
            foreach (var att in mail.Attachments)
            {
                if (att.ClaimStatus == ClaimStatus.Claimed)
                {
                    continue; // B3：已领不动
                }

                if (att.ClaimStatus == ClaimStatus.Claimable)
                {
                    att.ClaimStatus = ClaimStatus.Discarded;
                    hasUnclaimed = true;
                }
            }

            // 仍存在非已领附件（被作废）→ 终态 Revoked；全部已领则保持原状态（玩家可自行删除）。
            if (hasUnclaimed || mail.Attachments.Count == 0)
            {
                // 无附件邮件撤回：保持原状态（不强制作废）。有作废附件则进入 Revoked 终态。
                if (hasUnclaimed && mail.MailStatus != MailStatus.Revoked)
                {
                    mail.MailStatus = MailStatus.Revoked;
                }
            }

            RecomputeAttachmentStatus(mail);
            changedMailIds.Add(mail.MailId);
            changed = true;
        }

        return changed;
    }

    /// <summary>
    /// 重算 <see cref="MailState.AttachmentStatus"/> 派生字段（U1 §4.3）。委托 <see cref="MailAttachmentClaim.ComputeAttachmentStatus"/>，单一事实来源。
    /// </summary>
    private static void RecomputeAttachmentStatus(MailState mail)
    {
        mail.AttachmentStatus = MailAttachmentClaim.ComputeAttachmentStatus(mail);
    }

    /// <summary>
    /// 计算实例过期时间：Campaign <c>ExpireAt</c> 大于 0 时使用；否则表示永不过期。
    /// </summary>
    private static long ResolveExpireTime(MailCampaignState campaign, long createTime)
    {
        if (campaign.ExpireAt > 0)
        {
            return campaign.ExpireAt;
        }

        return long.MaxValue;
    }

    /// <summary>
    /// 解析邮件过期策略。从 Campaign 读取；邮件实例未单独存储策略，依赖 CampaignId@Version 反查（一期 Campaign 在注册表内）。
    /// </summary>
    private ExpireAttachmentPolicy ResolveExpirePolicy(MailState mail)
    {
        var campaign = MailCampaignRegistry.QueryPublishedSince(0).Find(c => c.CampaignId == mail.CampaignId && c.PublishVersion == mail.CampaignVersion);
        return campaign?.ExpireAttachmentPolicy ?? ExpireAttachmentPolicy.DiscardUnclaimed;
    }

    /// <summary>
    /// 选取本地化文案。一期默认取 <c>zh-CN</c>，缺失则取快照首项；多语言渲染引擎留 follow-up。
    /// </summary>
    private static void PickLocalized(MailCampaignState campaign, out string title, out string content)
    {
        title = string.Empty;
        content = string.Empty;
        title = PickText(campaign.Titles);
        content = PickText(campaign.Contents);
    }

    private static string PickText(List<MailLocalizedContent> content)
    {
        if (content == null || content.Count == 0)
        {
            return string.Empty;
        }

        foreach (var item in content)
        {
            if (item != null && string.Equals(item.Language, "zh-cn", StringComparison.OrdinalIgnoreCase))
            {
                return item.Text ?? string.Empty;
            }
        }

        foreach (var item in content)
        {
            if (item != null)
            {
                return item.Text ?? string.Empty;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 按邮件 ID 查找实例（不含已删除之外的过滤由调用方决定）。
    /// </summary>
    private static MailState TryFindMail(MailBoxState state, long mailId)
    {
        foreach (var mail in state.List)
        {
            if (mail.MailId == mailId)
            {
                return mail;
            }
        }

        return null;
    }

    /// <summary>
    /// 构造邮件摘要视图。
    /// </summary>
    private static MailInfo BuildMailInfo(MailState mail)
    {
        RecomputeAttachmentStatus(mail);
        return new MailInfo
        {
            MailId = mail.MailId,
            CampaignId = mail.CampaignId,
            CampaignVersion = mail.CampaignVersion,
            MailType = (int)mail.MailType,
            Title = mail.Title,
            ReadStatus = (int)mail.ReadStatus,
            AttachmentStatus = (int)mail.AttachmentStatus,
            MailStatus = (int)mail.MailStatus,
            CreateTime = mail.CreateTime,
            ExpireTime = mail.ExpireTime,
            HasAttachment = mail.Attachments != null && mail.Attachments.Count > 0,
        };
    }
}
