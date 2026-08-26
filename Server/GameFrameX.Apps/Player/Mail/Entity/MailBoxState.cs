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

namespace GameFrameX.Apps.Player.Mail.Entity;

/// <summary>
/// 玩家邮件箱状态。每玩家一份，挂在玩家 Actor 的 <see cref="Component.MailComponent"/> 上。
/// </summary>
/// <remarks>
/// 懒创建幂等（B5）以 <see cref="CreatedCampaignVersions"/>（<c>CampaignId@Version</c>）为去重键：同一 Campaign 仅实例化一次；
/// 含已撤回 Campaign，防撤回后重实例化；渠道条件未补齐的 Campaign **不记 key**（条件待定，区别于撤回的永久跳过）。
/// </remarks>
public sealed class MailBoxState : CacheState
{
    /// <summary>玩家个人邮件实例集合。</summary>
    public List<MailState> List { get; set; } = new List<MailState>();

    /// <summary>
    /// 已实例化的 <c>CampaignId@Version</c> 集合。懒创建去重键（B5）。
    /// 含已撤回 Campaign（防撤回后重实例化）；渠道未补齐的 Campaign 不记入此集合（待补齐后再判断）。
    /// </summary>
    public HashSet<string> CreatedCampaignVersions { get; set; } = new HashSet<string>();

    /// <summary>未读计数。冗余字段，由读信流程维护，仅作展示。</summary>
    public int UnreadCount { get; set; }

    /// <summary>上次懒同步游标（<c>PublishedAt</c> 增量扫描）。用于避免每次全量扫描发布记录。</summary>
    public long LastSyncCampaignTime { get; set; }

    /// <summary>上次过期清理时间。用于定时清理调度。</summary>
    public long LastCleanupTime { get; set; }

    /// <summary>
    /// 玩家内邮件实例 ID 分配序列。玩家 Actor 单线程驱动，自增即可保证玩家内唯一。
    /// </summary>
    public long MailIdSeq { get; set; }
}
