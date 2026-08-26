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

namespace GameFrameX.Apps.Player.Reward.Entity;

/// <summary>
/// 统一奖励发放接口的请求。
/// </summary>
/// <remarks>
/// 调用方必须保证 <see cref="RoleId"/> 命中目标玩家、<see cref="SourceType"/>/<see cref="SourceId"/>/<see cref="TraceId"/> 三者共同唯一。
/// 幂等键 = <c>RoleId:SourceType:SourceId:TraceId</c>（B6），重复提交不重复发奖、短路返回上次结果。
/// </remarks>
public sealed class RewardGrantRequest
{
    /// <summary>
    /// 目标玩家 ID（玩家 Actor 的 <c>ActorId</c>，等同玩家 <c>PlayerState.Id</c>）。
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// 奖励来源类型。禁止使用 <see cref="RewardSourceType.None"/> 拼接幂等键。
    /// </summary>
    public RewardSourceType SourceType { get; set; }

    /// <summary>
    /// 来源 ID。建议形如 <c>mail:&lt;MailId&gt;:attachment:&lt;AttachmentId&gt;</c>，与调用方业务一一对应。
    /// </summary>
    public string SourceId { get; set; }

    /// <summary>
    /// 调用方追踪 ID。与 <see cref="SourceType"/>/<see cref="SourceId"/> 组成幂等键的一部分。
    /// </summary>
    public string TraceId { get; set; }

    /// <summary>
    /// 待发放奖励列表。逐项独立判定成功 / 失败。
    /// </summary>
    public List<RewardItem> Rewards { get; set; } = new List<RewardItem>();
}

/// <summary>
/// 统一奖励发放接口的结果。
/// </summary>
public sealed class RewardGrantResult
{
    /// <summary>
    /// 逐项发放结果。
    /// </summary>
    public List<RewardGrantItemResult> Items { get; set; } = new List<RewardGrantItemResult>();

    /// <summary>
    /// 整体错误码（<c>OperationStatusCode</c> 整数值）：
    /// 全部成功 <c>Ok</c>；部分成功 <c>PartialSuccess</c>；全部失败取首个失败项错误码。
    /// </summary>
    public int ErrorCode { get; set; }

    /// <summary>
    /// 是否全部奖励发放成功。
    /// </summary>
    public bool AllSuccess { get; set; }
}
