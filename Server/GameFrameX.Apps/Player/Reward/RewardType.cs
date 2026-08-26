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

namespace GameFrameX.Apps.Player.Reward;

/// <summary>
/// 统一奖励发放的奖励类型路由枚举。
/// </summary>
/// <remarks>
/// 编号一旦发布不可变：后续新增类型只追加，禁止复用既有编号。
/// 一期仅 <see cref="NormalItem"/> 可发放，其余为预留路由位，由后续隐藏资产 / 订阅 / 权益模块承接，发放接口一期返回 <c>Unsupported</c>。
/// </remarks>
public enum RewardType
{
    /// <summary>
    /// 普通道具：写入玩家背包 <c>BagState</c>，在线推送背包变更通知。
    /// </summary>
    NormalItem = 0,

    /// <summary>
    /// 隐藏道具：预留路由位，由后续隐藏资产模块承接，不进入普通背包展示。
    /// </summary>
    HiddenItem = 1,

    /// <summary>
    /// 月卡：预留路由位，由后续订阅 / 权益模块承接。
    /// </summary>
    MonthlyCard = 2,

    /// <summary>
    /// 终生卡：预留路由位，由后续订阅 / 权益模块承接。
    /// </summary>
    LifetimeCard = 3,

    /// <summary>
    /// VIP 点：预留路由位，由后续 VIP / 权益模块承接。
    /// </summary>
    VipPoint = 4,

    /// <summary>
    /// 权益类资产：预留路由位，由后续权益模块承接。
    /// </summary>
    Equity = 5,
}
