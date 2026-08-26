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
/// 统一奖励发放的一次幂等账本记录，存储在玩家 <see cref="RewardGrantRecordState"/> 中。
/// </summary>
/// <remarks>
/// 该记录是发放幂等（B6）的事实来源：同一 <see cref="TraceKey"/> 再次提交时，发放接口直接读取本记录返回上次结果，
/// 不重复写背包、不重复扣附件。记录一旦写入即不可变（不更新、不删除，一期无 TTL / 容量回收）。
/// </remarks>
public sealed class RewardGrantRecord
{
    /// <summary>
    /// 幂等键，格式 <c>RoleId:SourceType:SourceId:TraceId</c>。
    /// </summary>
    public string TraceKey { get; set; }

    /// <summary>
    /// 来源类型（冗余存储，便于审计）。
    /// </summary>
    public int SourceType { get; set; }

    /// <summary>
    /// 来源 ID（冗余存储，便于审计）。
    /// </summary>
    public string SourceId { get; set; }

    /// <summary>
    /// 调用方追踪 ID（冗余存储，便于审计）。
    /// </summary>
    public string TraceId { get; set; }

    /// <summary>
    /// 发放完成的 unix 秒。
    /// </summary>
    public long GrantedAt { get; set; }

    /// <summary>
    /// 是否全部成功。
    /// </summary>
    public bool AllSuccess { get; set; }

    /// <summary>
    /// 整体错误码（<c>OperationStatusCode</c> 整数值）。
    /// </summary>
    public int ErrorCode { get; set; }

    /// <summary>
    /// 逐项发放结果快照。重复提交时据此重建返回结果。
    /// </summary>
    public List<RewardGrantItemResult> Items { get; set; } = new List<RewardGrantItemResult>();
}
