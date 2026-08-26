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

using System.Collections.Generic;
using System.Text;
using GameFrameX.Apps.Player.Mail;
using GameFrameX.Apps.Player.Mail.Entity;

namespace GameFrameX.Hotfix.Logic.Player.Mail;

/// <summary>
/// 邮件附件领取的纯函数集合（U1 §4.4 / B6）。集中承载领取前的查找、领取状态写入、附件整体维度派生与幂等 trace 构造，
/// 作为 <see cref="MailComponentAgent"/> 与单元测试的共同事实来源。
/// </summary>
/// <remarks>
/// 单向流转：<see cref="ClaimStatus.Claimable"/> → <see cref="ClaimStatus.Claimed"/>（终态，撤回不回滚，B3）；
/// 或 → <see cref="ClaimStatus.Discarded"/>（撤回 / 过期作废，B3 / B4）。重复领取视为幂等成功，不重复发奖（B6）。
/// </remarks>
public static class MailAttachmentClaim
{
    /// <summary>
    /// 查找并准备一次单附件领取。不写入任何状态，仅返回命中的邮件 / 附件引用（未命中为 null）。
    /// 不在此处判定错误码：调用方根据 <see cref="ClaimPrepareResult.Mail"/> / <see cref="ClaimPrepareResult.Attachment"/>
    /// 是否命中与 <see cref="MailAttachmentInstance.ClaimStatus"/> 直接给响应的 ErrorCode 赋值（成功不赋，框架默认 0）。
    /// </summary>
    /// <param name="state">玩家邮件箱状态。</param>
    /// <param name="mailId">邮件实例 ID。</param>
    /// <param name="slotId">附件槽位 ID。</param>
    public static ClaimPrepareResult Prepare(MailBoxState state, long mailId, int slotId)
    {
        var mail = TryFindMail(state, mailId);
        if (mail == null || mail.MailStatus == MailStatus.Deleted)
        {
            return ClaimPrepareResult.Empty;
        }

        // 终态邮件（撤回 / 过期）整体不可领：未领附件已被作废流程置 Discarded，统一由附件状态判定。
        var attachment = TryFindAttachment(mail, slotId);
        return new ClaimPrepareResult { Mail = mail, Attachment = attachment };
    }

    /// <summary>
    /// 将附件置为已领（终态）。一旦写入不可回退（B3 撤回不回滚已领附件）。
    /// </summary>
    public static void MarkClaimed(MailAttachmentInstance attachment, long now)
    {
        attachment.ClaimStatus = ClaimStatus.Claimed;
        attachment.ClaimTime = now;
    }

    /// <summary>
    /// 将附件置为作废（撤回 / 过期，B3 / B4）。已领附件不应调用本方法（由调用方保证）。
    /// </summary>
    public static void MarkDiscarded(MailAttachmentInstance attachment)
    {
        if (attachment.ClaimStatus == ClaimStatus.Claimed)
        {
            return; // B3：已领不动。
        }

        attachment.ClaimStatus = ClaimStatus.Discarded;
    }

    /// <summary>
    /// 重算邮件附件整体维度（U1 §4.3）。派生字段，单一事实来源为 <see cref="MailState.MailStatus"/> + 各附件 <see cref="MailAttachmentInstance.ClaimStatus"/>。
    /// </summary>
    public static AttachmentStatus ComputeAttachmentStatus(MailState mail)
    {
        if (mail.Attachments == null || mail.Attachments.Count == 0)
        {
            return AttachmentStatus.NoAttachment;
        }

        if (mail.MailStatus == MailStatus.Revoked || mail.MailStatus == MailStatus.Expired)
        {
            return AttachmentStatus.Unclaimable;
        }

        var claimed = 0;
        var claimable = 0;
        foreach (var att in mail.Attachments)
        {
            if (att.ClaimStatus == ClaimStatus.Claimed)
            {
                claimed++;
            }
            else if (att.ClaimStatus == ClaimStatus.Claimable)
            {
                claimable++;
            }
        }

        if (claimable == 0 && claimed == mail.Attachments.Count)
        {
            return AttachmentStatus.AllClaimed;
        }

        // 仍存在可领（未领）附件 → PartialClaimed（含 0 已领情况，用于 B2 删除门禁）。
        return AttachmentStatus.PartialClaimed;
    }

    /// <summary>
    /// 构造领取幂等追踪键（U1 §4.4 / B6）：以 <c>CampaignId:CampaignVersion:MailId:SlotId</c> 四元组唯一标识一次附件发放。
    /// 与 <c>RewardGrantComponentAgent</c> 的 <c>RoleId:SourceType:SourceId:TraceId</c> 共同组成全局幂等键。
    /// </summary>
    public static string BuildTrace(long campaignId, int campaignVersion, long mailId, int slotId)
    {
        var sb = new StringBuilder();
        sb.Append(campaignId).Append(':').Append(campaignVersion).Append(':').Append(mailId).Append(':').Append(slotId);
        return sb.ToString();
    }

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

    private static MailAttachmentInstance TryFindAttachment(MailState mail, int slotId)
    {
        if (mail.Attachments == null)
        {
            return null;
        }

        foreach (var att in mail.Attachments)
        {
            if (att.SlotId == slotId)
            {
                return att;
            }
        }

        return null;
    }
}

/// <summary>
/// <see cref="MailAttachmentClaim.Prepare"/> 的查找结果（仅承载命中的邮件 / 附件引用，不存储错误码）。
/// </summary>
public sealed class ClaimPrepareResult
{
    /// <summary>命中的邮件实例（未命中或已删除时为 null）。</summary>
    public MailState Mail { get; set; }

    /// <summary>命中的附件实例（未命中槽位时为 null）。</summary>
    public MailAttachmentInstance Attachment { get; set; }

    /// <summary>空结果（邮件未命中）。复用避免分配。</summary>
    public static ClaimPrepareResult Empty { get; } = new ClaimPrepareResult();
}
