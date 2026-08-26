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

namespace GameFrameX.Apps.Player.Mail
{
    /// <summary>
    /// 邮件附件过期处置策略（B4：过期未领取附件默认作废，可配置）。
    /// 决定 Campaign 撤回或自然过期后，玩家尚未领取的附件如何处理。
    /// </summary>
    public enum ExpireAttachmentPolicy
    {
        /// <summary>
        /// 默认作废：未领取附件直接丢弃，不进入玩家任何资产账户，日志留痕用于审计。
        /// </summary>
        DiscardUnclaimed = 0,

        /// <summary>
        /// 保留未领取：附件保留在邮件中，玩家仍可领取，仅停止向新玩家下发（用于「停发但已命中可继续领」场景）。
        /// </summary>
        KeepUnclaimed = 1,

        /// <summary>
        /// 自动入袋：未领取附件通过统一奖励发放接口自动发放到玩家账户（需谨慎，可能突破发放窗口限制）。
        /// </summary>
        AutoClaim = 2,
    }
}
