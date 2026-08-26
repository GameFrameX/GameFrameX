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
using GameFrameX.Apps.Player.Mail;
using GameFrameX.Hotfix.Logic.Player.Mail;

namespace GameFrameX.Hotfix.Logic.Http.Mail
{
    /// <summary>
    /// Admin 撤回运营邮件 Campaign。
    /// 路由：POST /api/mailCampaign/revoke。
    /// 仅置 <see cref="MailCampaignState.Status"/> = Revoked 与时间戳（B3：撤回不回滚已发放资产）。
    /// </summary>
    [HttpMessageMapping(typeof(RevokeMailCampaignHttpHandler))]
    [HttpMessageRequest(typeof(RevokeMailCampaignRequest))]
    [HttpMessageResponse(typeof(RevokeMailCampaignResponse))]
    [Description("Admin 撤回运营邮件 Campaign")]
    public sealed class RevokeMailCampaignHttpHandler : BaseHttpHandler
    {
        /// <inheritdoc />
        public override async Task<string> Action(string ip, string url, HttpMessageRequestBase requestBase)
        {
            var request = (RevokeMailCampaignRequest)requestBase;
            var response = new RevokeMailCampaignResponse { CampaignId = request.CampaignId };

            var code = MailCampaignRegistry.Revoke(request.CampaignId, request.Operator, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            response.Code = code;
            switch (code)
            {
                case MailCampaignErrorCode.Ok:
                    response.Message = "撤回成功（已发放资产不回滚 B3）";
                    if (MailCampaignRegistry.TryQuery(request.CampaignId, out var campaign))
                    {
                        response.RevokedAt = campaign.RevokedAt;
                    }

                    break;
                case MailCampaignErrorCode.CampaignNotFound:
                    response.Message = "Campaign 不存在";
                    break;
                case MailCampaignErrorCode.CampaignAlreadyRevoked:
                    response.Message = "Campaign 已撤回，不可重复撤回";
                    break;
                default:
                    response.Message = "撤回失败";
                    break;
            }

            return HttpJsonResult.SuccessString(response);
        }
    }

    /// <summary>
    /// 撤回 Campaign 请求体。
    /// </summary>
    public sealed class RevokeMailCampaignRequest : HttpMessageRequestBase
    {
        /// <summary>Campaign ID。</summary>
        [Required]
        [Range(1, long.MaxValue)]
        [Description("Campaign ID")]
        public long CampaignId { get; set; }

        /// <summary>撤回操作人（Admin 账号）。</summary>
        [Description("撤回操作人")]
        public string Operator { get; set; }
    }

    /// <summary>
    /// 撤回 Campaign 响应体。
    /// </summary>
    public sealed class RevokeMailCampaignResponse : HttpMessageResponseBase
    {
        /// <summary>业务码。</summary>
        [Description("业务码")]
        public MailCampaignErrorCode Code { get; set; }

        /// <summary>Campaign ID。</summary>
        [Description("CampaignId")]
        public long CampaignId { get; set; }

        /// <summary>撤回时间（Unix 秒，UTC）。撤回失败时为 0。</summary>
        [Description("撤回时间")]
        public long RevokedAt { get; set; }

        /// <summary>提示信息。</summary>
        [Description("提示信息")]
        public string Message { get; set; }
    }
}
