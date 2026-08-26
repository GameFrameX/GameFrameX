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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GameFrameX.Apps.Player.Mail;
using GameFrameX.Apps.Player.Mail.Entity;

namespace GameFrameX.Hotfix.Logic.Player.Mail
{
    /// <summary>
    /// 运营邮件 Campaign 进程级注册表：Admin HTTP API 的写入 / 撤回 / 查询入口，
    /// 也是玩家邮件懒创建逻辑读取 Campaign 的只读源（见 U1 §3.8）。
    /// 一期为单进程内存权威（<see cref="ConcurrentDictionary{TKey, TValue}"/>），跨进程持久化与同步不在本 change 范围。
    /// </summary>
    /// <remarks>
    /// 不可逆边界（B1–B4）：
    /// - B1 发布后主体字段不可修改：已 <see cref="MailCampaignStatus.Published"/> 的 Campaign 再次提交时拒绝（返回 <see cref="MailCampaignErrorCode.HasExist"/>）。
    /// - B3 撤回不回滚：撤回只置 <see cref="MailCampaignState.Status"/> = <see cref="MailCampaignStatus.Revoked"/> 与 <see cref="MailCampaignState.RevokedAt"/>，已发放的奖励不回收。
    /// - B4 过期未领取附件按 <see cref="MailCampaignState.ExpireAttachmentPolicy"/> 处理；落库与领取流程由后续玩家邮件核心 change 实现。
    /// </remarks>
    public static class MailCampaignRegistry
    {
        private static readonly ConcurrentDictionary<long, MailCampaignState> Campaigns = new ConcurrentDictionary<long, MailCampaignState>();

        private static long _nextCampaignId = 1;

        /// <summary>
        /// 校验 Campaign 参数合法性（发布与预览共用）。
        /// 返回 <see cref="MailCampaignErrorCode.Ok"/> 表示通过；否则返回对应错误码。
        /// </summary>
        /// <param name="campaign">待校验的 Campaign 快照（<see cref="MailCampaignState.CampaignId"/> 可为 0，由发布接口分配）。</param>
        public static MailCampaignErrorCode Validate(MailCampaignState campaign)
        {
            if (campaign == null)
            {
                return MailCampaignErrorCode.InvalidCampaignParameter;
            }

            if (campaign.Titles == null || campaign.Titles.Count == 0)
            {
                return MailCampaignErrorCode.InvalidCampaignParameter;
            }

            foreach (var title in campaign.Titles)
            {
                if (title == null || string.IsNullOrWhiteSpace(title.Language) || string.IsNullOrWhiteSpace(title.Text))
                {
                    return MailCampaignErrorCode.InvalidCampaignParameter;
                }
            }

            if (campaign.Contents != null)
            {
                foreach (var content in campaign.Contents)
                {
                    if (content == null || string.IsNullOrWhiteSpace(content.Language))
                    {
                        return MailCampaignErrorCode.InvalidCampaignParameter;
                    }
                }
            }

            if (campaign.Attachments != null && campaign.Attachments.Count > 0)
            {
                var slotSet = new HashSet<int>();
                foreach (var attachment in campaign.Attachments)
                {
                    // 附件数量非正属于 Campaign 参数非法（奖励发放层的 InvalidReward 用于运行时发放失败，与此处校验区分）。
                    if (attachment == null || attachment.Amount <= 0)
                    {
                        return MailCampaignErrorCode.InvalidCampaignParameter;
                    }

                    if (!slotSet.Add(attachment.SlotId))
                    {
                        return MailCampaignErrorCode.InvalidCampaignParameter;
                    }
                }
            }

            if (campaign.MinLevel < 0 || campaign.MaxLevel < 0)
            {
                return MailCampaignErrorCode.InvalidCampaignParameter;
            }

            if (campaign.MaxLevel > 0 && campaign.MinLevel > campaign.MaxLevel)
            {
                return MailCampaignErrorCode.InvalidCampaignParameter;
            }

            if (campaign.ExpireAt < 0 || campaign.PublishedAt < 0 || campaign.CreateTime < 0)
            {
                return MailCampaignErrorCode.InvalidCampaignParameter;
            }

            return MailCampaignErrorCode.Ok;
        }

        /// <summary>
        /// 发布或更新 Campaign（B1：已发布的主体字段不可修改，重复提交返回 <see cref="MailCampaignErrorCode.HasExist"/>）。
        /// 成功时分配 <see cref="MailCampaignState.CampaignId"/>（若入参为 0）、置 <see cref="MailCampaignState.Status"/> = Published、写入 <see cref="MailCampaignState.PublishedAt"/>。
        /// </summary>
        /// <param name="campaign">待发布的 Campaign 快照。</param>
        /// <param name="operatorName">发布操作人（Admin 账号），写入 <see cref="MailCampaignState.PublishOperator"/> 用于审计。</param>
        /// <param name="nowUnixSeconds">发布时间戳（Unix 秒，UTC）；调用方传入以便测试注入。</param>
        /// <returns>发布成功后的最终 Campaign（含分配的 CampaignId / 版本号 / 时间戳）。</returns>
        public static MailCampaignState PublishOrUpdate(MailCampaignState campaign, string operatorName, long nowUnixSeconds)
        {
            var code = Validate(campaign);
            if (code != MailCampaignErrorCode.Ok)
            {
                throw new ArgumentException("Campaign 参数非法，code=" + code);
            }

            if (campaign.CampaignId <= 0)
            {
                campaign.CampaignId = AllocateCampaignId();
                campaign.PublishVersion = 1;
            }
            else if (Campaigns.TryGetValue(campaign.CampaignId, out var existing))
            {
                if (existing.Status == MailCampaignStatus.Published || existing.Status == MailCampaignStatus.Revoked)
                {
                    throw new InvalidOperationException("Campaign 已发布或已撤回，主体字段不可修改（B1）。CampaignId=" + campaign.CampaignId);
                }

                campaign.PublishVersion = existing.PublishVersion + 1;
            }
            else
            {
                campaign.PublishVersion = 1;
            }

            campaign.Status = MailCampaignStatus.Published;
            campaign.PublishedAt = nowUnixSeconds;
            if (campaign.CreateTime <= 0)
            {
                campaign.CreateTime = nowUnixSeconds;
            }

            campaign.PublishOperator = operatorName;
            campaign.RevokedAt = 0;
            campaign.RevokeOperator = null;
            Campaigns[campaign.CampaignId] = campaign;
            return campaign;
        }

        /// <summary>
        /// 撤回 Campaign（B3：撤回不回滚已发放资产）。
        /// 将 <see cref="MailCampaignState.Status"/> 置为 <see cref="MailCampaignStatus.Revoked"/>，写入 <see cref="MailCampaignState.RevokedAt"/> 与 <see cref="MailCampaignState.RevokeOperator"/>。
        /// </summary>
        /// <param name="campaignId">待撤回的 Campaign ID。</param>
        /// <param name="operatorName">撤回操作人（Admin 账号）。</param>
        /// <param name="nowUnixSeconds">撤回时间戳（Unix 秒，UTC）。</param>
        /// <returns>成功返回 <see cref="MailCampaignErrorCode.Ok"/>；Campaign 不存在返回 <see cref="MailCampaignErrorCode.CampaignNotFound"/>；已撤回返回 <see cref="MailCampaignErrorCode.CampaignAlreadyRevoked"/>。</returns>
        public static MailCampaignErrorCode Revoke(long campaignId, string operatorName, long nowUnixSeconds)
        {
            if (!Campaigns.TryGetValue(campaignId, out var campaign))
            {
                return MailCampaignErrorCode.CampaignNotFound;
            }

            if (campaign.Status == MailCampaignStatus.Revoked)
            {
                return MailCampaignErrorCode.CampaignAlreadyRevoked;
            }

            campaign.Status = MailCampaignStatus.Revoked;
            campaign.RevokedAt = nowUnixSeconds;
            campaign.RevokeOperator = operatorName;
            return MailCampaignErrorCode.Ok;
        }

        /// <summary>
        /// 查询单个 Campaign（Admin 查询发布状态）。
        /// </summary>
        /// <param name="campaignId">Campaign ID。</param>
        /// <param name="campaign">查到时输出 Campaign 快照；否则输出 null。</param>
        /// <returns>存在返回 true；否则 false。</returns>
        public static bool TryQuery(long campaignId, out MailCampaignState campaign)
        {
            return Campaigns.TryGetValue(campaignId, out campaign);
        }

        /// <summary>
        /// 构造玩家邮箱懒创建去重键 <c>CampaignId@PublishVersion</c>（B5 幂等键）。
        /// </summary>
        public static string BuildKey(long campaignId, int publishVersion)
        {
            return campaignId + "@" + publishVersion;
        }

        /// <summary>
        /// 按 <see cref="MailCampaignState.PublishedAt"/> 升序返回发布时间大于 <paramref name="since"/> 的 Campaign。
        /// </summary>
        public static List<MailCampaignState> QueryPublishedSince(long since)
        {
            return Campaigns.Values
                .Where(c => c.PublishedAt > since)
                .OrderBy(c => c.PublishedAt)
                .ToList();
        }

        /// <summary>
        /// 返回当前所有已撤回 Campaign，供玩家懒同步时作废已实例化邮件（B3）。
        /// </summary>
        public static List<MailCampaignState> GetRevokedCampaigns()
        {
            return Campaigns.Values
                .Where(c => c.Status == MailCampaignStatus.Revoked)
                .ToList();
        }

        /// <summary>
        /// 按过滤条件查询 Campaign 列表（Admin 查询发布状态）。所有过滤条件为 AND 关系，空 / 0 表示不限。
        /// </summary>
        /// <param name="status">状态过滤；传 null 表示不限。</param>
        /// <param name="mailType">类型过滤；传 null 表示不限。</param>
        /// <param name="serverId">服务器 ID 过滤；≤ 0 表示不限。</param>
        /// <param name="channelId">渠道 ID 过滤；≤ 0 表示不限。</param>
        /// <param name="minLevel">最低等级过滤；≤ 0 表示不限。</param>
        /// <param name="createdFromUnixSeconds">创建时间下限（含）；≤ 0 表示不限。</param>
        /// <param name="createdToUnixSeconds">创建时间上限（含）；≤ 0 表示不限。</param>
        /// <param name="limit">返回条数上限；≤ 0 表示返回全部。</param>
        public static List<MailCampaignState> QueryAll(MailCampaignStatus? status = null, MailType? mailType = null, int serverId = 0, int channelId = 0, int minLevel = 0, long createdFromUnixSeconds = 0, long createdToUnixSeconds = 0, int limit = 0)
        {
            IEnumerable<MailCampaignState> result = Campaigns.Values;

            if (status.HasValue)
            {
                result = result.Where(c => c.Status == status.Value);
            }

            if (mailType.HasValue)
            {
                result = result.Where(c => c.MailType == mailType.Value);
            }

            if (serverId > 0)
            {
                result = result.Where(c => c.ServerIds != null && c.ServerIds.Contains(serverId));
            }

            if (channelId > 0)
            {
                result = result.Where(c => c.ChannelIds != null && c.ChannelIds.Contains(channelId));
            }

            if (minLevel > 0)
            {
                result = result.Where(c => c.MaxLevel == 0 || c.MaxLevel >= minLevel);
            }

            if (createdFromUnixSeconds > 0)
            {
                result = result.Where(c => c.CreateTime >= createdFromUnixSeconds);
            }

            if (createdToUnixSeconds > 0)
            {
                result = result.Where(c => c.CreateTime <= createdToUnixSeconds);
            }

            result = result.OrderBy(c => c.CreateTime);

            if (limit > 0)
            {
                result = result.Take(limit);
            }

            return result.ToList();
        }

        /// <summary>
        /// 估算 Campaign 命中规模（预览用）。一期返回附件槽位数 + 过滤条件摘要，不做真实玩家数估算（数据源不在本 change 范围）。
        /// </summary>
        /// <param name="campaign">待预览的 Campaign。</param>
        /// <param name="estimatedHitCount">估算命中玩家数（一期固定返回 -1，表示「需要接入在线 / 角色统计模块后才有真实值」）。</param>
        public static void Preview(MailCampaignState campaign, out long estimatedHitCount)
        {
            var code = Validate(campaign);
            if (code != MailCampaignErrorCode.Ok)
            {
                throw new ArgumentException("Campaign 参数非法，code=" + code);
            }

            // 一期无角色 / 在线统计模块接入，估算命中返回 -1 表示「未知」。
            estimatedHitCount = -1;
        }

        /// <summary>
        /// 分配下一个 CampaignId（线程安全自增）。一期使用进程内自增，跨进程部署需替换为雪花 / 数据库序列。
        /// </summary>
        private static long AllocateCampaignId()
        {
            return System.Threading.Interlocked.Increment(ref _nextCampaignId);
        }

        /// <summary>
        /// 测试 / SelfCheck 用：清空注册表并重置 ID 分配器。生产路径不得调用。
        /// </summary>
        internal static void ResetForTest()
        {
            Campaigns.Clear();
            _nextCampaignId = 1;
        }
    }
}
