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

namespace GameFrameX.Hotfix.Logic.Player.Mail
{
    /// <summary>
    /// 运营邮件 Campaign HTTP 接口错误码。
    /// 仅用于 Admin HTTP API（发布 / 预览 / 撤回 / 查询），不进入客户端协议，故不走 protobuf，直接定义为 C# 枚举。
    /// 由 <see cref="MailCampaignRegistry"/> 产生，HTTP handler 透传到响应。
    /// </summary>
    public enum MailCampaignErrorCode
    {
        /// <summary>成功</summary>
        Ok = 0,

        /// <summary>Campaign 已发布或已撤回，主体字段不可修改（B1）。</summary>
        HasExist = 1,

        /// <summary>Campaign 不存在（查询 / 撤回时未找到对应 CampaignId）。</summary>
        CampaignNotFound = 2,

        /// <summary>Campaign 已撤回，不可重复撤回或修改。</summary>
        CampaignAlreadyRevoked = 3,

        /// <summary>Campaign 参数非法（必填缺失、过滤条件矛盾、附件数量非正等）。</summary>
        InvalidCampaignParameter = 4,
    }
}
