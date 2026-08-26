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
/// 玩家邮件内的单个附件实例。实例化时从 <see cref="MailAttachmentState"/> 复制元信息，并承载领取状态（U1 §3.5）。
/// </summary>
/// <remarks>
/// 领取状态 <see cref="ClaimStatus"/> 单向流转：<see cref="ClaimStatus.Claimable"/> → <see cref="ClaimStatus.Claimed"/>（已领，终态，撤回不回滚，B3）；
/// 或 → <see cref="ClaimStatus.Discarded"/>（撤回 / 过期作废，B3 / B4）。领取幂等（B6）由统一发放接口保证。
/// </remarks>
public sealed class MailAttachmentInstance
{
    /// <summary>附件槽位 ID。邮件内唯一，实例化时从 <see cref="MailAttachmentState.SlotId"/> 复制。</summary>
    public int SlotId { get; set; }

    /// <summary>奖励类型。实例化时复制。</summary>
    public int RewardType { get; set; }

    /// <summary>物品 ID。实例化时复制。</summary>
    public int ItemId { get; set; }

    /// <summary>数量。实例化时复制。</summary>
    public long Amount { get; set; }

    /// <summary>展示用图标 ID。实例化时复制。</summary>
    public int IconId { get; set; }

    /// <summary>领取状态。单向流转，终态不可回退（B3 / B6）。</summary>
    public ClaimStatus ClaimStatus { get; set; } = ClaimStatus.Claimable;

    /// <summary>首次领取时间（unix 秒）。空表示未领取。一旦写入不可清空（B3 不回滚）。</summary>
    public long? ClaimTime { get; set; }
}
