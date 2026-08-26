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

namespace GameFrameX.Apps.Player.Mail;

/// <summary>
/// 玩家邮件实例生命周期状态（U1 §4.2）。
/// </summary>
/// <remarks>
/// 主线状态机：<see cref="Unread"/> →（读信）→ <see cref="Read"/> →（附件全领）→ <see cref="Claimed"/> →（删除）→ <see cref="Deleted"/>；
/// 或 <see cref="Unread"/>/活跃态 →（撤回/过期）→ <see cref="Revoked"/> / <see cref="Expired"/>。
/// <see cref="Deleted"/> / <see cref="Revoked"/> / <see cref="Expired"/> 为终态，不可回退。
/// 删除只允许在附件全部已领或无附件时进入 <see cref="Deleted"/>（B2，见删除算法）。
/// </remarks>
public enum MailStatus
{
    /// <summary>未读。实例化初态。</summary>
    Unread = 0,

    /// <summary>已读，无附件或附件未全领。</summary>
    Read = 1,

    /// <summary>部分领取。有附件已领但未全领。</summary>
    PartialClaimed = 2,

    /// <summary>附件全部已领（或无附件且已读后的稳定态）。</summary>
    Claimed = 3,

    /// <summary>已删除。终态，不可恢复（B2 校验通过后才允许进入）。</summary>
    Deleted = 4,

    /// <summary>已撤回。终态（B3）。</summary>
    Revoked = 5,

    /// <summary>已过期。终态（B4）。</summary>
    Expired = 6,
}

/// <summary>
/// 邮件读信维度（U1 §4.3）。与 <see cref="MailStatus"/> 正交，由 <c>ReadTime</c> 派生。
/// </summary>
public enum ReadStatus
{
    /// <summary>未读。</summary>
    Unread = 0,

    /// <summary>已读。读信幂等，重复读信不重复触发。</summary>
    Read = 1,
}

/// <summary>
/// 邮件附件整体维度，便于列表展示（U1 §4.3）。派生字段，单一事实来源为 <see cref="MailStatus"/> + 各附件 <see cref="ClaimStatus"/>。
/// </summary>
public enum AttachmentStatus
{
    /// <summary>无附件。</summary>
    NoAttachment = 0,

    /// <summary>有附件但不可领（撤回 / 过期）。</summary>
    Unclaimable = 1,

    /// <summary>部分附件已领。</summary>
    PartialClaimed = 2,

    /// <summary>附件全部已领。</summary>
    AllClaimed = 3,
}

/// <summary>
/// 单个附件的领取状态（U1 §4.3 / B6）。
/// </summary>
public enum ClaimStatus
{
    /// <summary>可领。</summary>
    Claimable = 0,

    /// <summary>已领。终态，不可回退；撤回不回滚已领附件（B3）。</summary>
    Claimed = 1,

    /// <summary>作废。撤回或过期导致未领附件作废（B3 / B4）。</summary>
    Discarded = 2,
}
