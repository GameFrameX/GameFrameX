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
    /// 运营邮件发布记录（Campaign）的生命周期状态。
    /// 发布成功后 Campaign 处于 <see cref="Published"/>；调用 Admin 撤回接口后转为 <see cref="Revoked"/>。
    /// 一旦发布，Campaign 主体字段不可修改（B1：发布后不可修改），仅状态字段可流转。
    /// </summary>
    public enum MailCampaignStatus
    {
        /// <summary>
        /// 已发布：Campaign 已落库，玩家可在过滤命中后懒创建邮件实例。
        /// </summary>
        Published = 0,

        /// <summary>
        /// 已撤回：运营通过 Admin 接口撤回；新玩家不再命中，已发放但未领取的附件按 <see cref="ExpireAttachmentPolicy"/> 处理，已领取资产不回滚（B3）。
        /// </summary>
        Revoked = 1,
    }
}
