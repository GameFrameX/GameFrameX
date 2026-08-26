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
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using System.Collections.Generic;
using GameFrameX.Apps.Player.Mail;
using GameFrameX.Apps.Player.Mail.Entity;
using GameFrameX.Hotfix.Logic.Player.Mail;

namespace GameFrameX.Hotfix.Logic.Http.Mail
{
    /// <summary>
    /// Admin 预览 / 校验 Campaign 参数（不落库）。
    /// 路由：POST /api/mailCampaign/preview。
    /// 用于发布前确认多语言文案、附件、过滤条件是否合法，并返回预估命中规模。
    /// </summary>
    [HttpMessageMapping(typeof(PreviewMailCampaignHttpHandler))]
    [HttpMessageRequest(typeof(PreviewMailCampaignRequest))]
    [HttpMessageResponse(typeof(PreviewMailCampaignResponse))]
    [Description("Admin 预览 / 校验运营邮件 Campaign")]
    public sealed class PreviewMailCampaignHttpHandler : BaseHttpHandler
    {
        /// <inheritdoc />
        public override async Task<string> Action(string ip, string url, HttpMessageRequestBase requestBase)
        {
            var request = (PreviewMailCampaignRequest)requestBase;
            var response = new PreviewMailCampaignResponse();

            var state = new MailCampaignState
            {
                MailType = request.MailType,
                MinLevel = request.MinLevel,
                MaxLevel = request.MaxLevel,
                Titles = request.Titles ?? new List<MailLocalizedContent>(),
                Contents = request.Contents ?? new List<MailLocalizedContent>(),
                Attachments = request.Attachments ?? new List<MailAttachmentState>(),
                ServerIds = request.ServerIds ?? new List<int>(),
                ChannelIds = request.ChannelIds ?? new List<int>(),
                ExpireAttachmentPolicy = request.ExpireAttachmentPolicy,
                ExpireAt = request.ExpireAt,
            };

            var code = MailCampaignRegistry.Validate(state);
            if (code != MailCampaignErrorCode.Ok)
            {
                response.Code = code;
                response.Message = "参数校验失败";
                return HttpJsonResult.SuccessString(response);
            }

            try
            {
                MailCampaignRegistry.Preview(state, out var estimatedHitCount);
                response.Code = MailCampaignErrorCode.Ok;
                response.EstimatedHitCount = estimatedHitCount;
                response.Message = "参数校验通过";
                return HttpJsonResult.SuccessString(response);
            }
            catch (ArgumentException ex)
            {
                response.Code = MailCampaignErrorCode.InvalidCampaignParameter;
                response.Message = ex.Message;
                return HttpJsonResult.SuccessString(response);
            }
        }
    }

    /// <summary>
    /// 预览 / 校验 Campaign 请求体。字段与发布请求一致，但不分配 CampaignId。
    /// </summary>
    public sealed class PreviewMailCampaignRequest : HttpMessageRequestBase
    {
        /// <summary>邮件类型。</summary>
        [Required]
        [Description("邮件类型")]
        public MailType MailType { get; set; }

        /// <summary>多语言标题，至少 1 项。</summary>
        [Required]
        [Description("多语言标题")]
        public List<MailLocalizedContent> Titles { get; set; } = new List<MailLocalizedContent>();

        /// <summary>多语言正文。</summary>
        [Description("多语言正文")]
        public List<MailLocalizedContent> Contents { get; set; } = new List<MailLocalizedContent>();

        /// <summary>附件列表。</summary>
        [Description("附件列表")]
        public List<MailAttachmentState> Attachments { get; set; } = new List<MailAttachmentState>();

        /// <summary>服务器 ID 白名单。</summary>
        [Description("服务器 ID 白名单")]
        public List<int> ServerIds { get; set; } = new List<int>();

        /// <summary>渠道 ID 白名单。</summary>
        [Description("渠道 ID 白名单")]
        public List<int> ChannelIds { get; set; } = new List<int>();

        /// <summary>最低等级。</summary>
        [Description("最低等级")]
        public int MinLevel { get; set; }

        /// <summary>最高等级。</summary>
        [Description("最高等级")]
        public int MaxLevel { get; set; }

        /// <summary>附件过期处置策略。</summary>
        [Description("附件过期处置策略")]
        public ExpireAttachmentPolicy ExpireAttachmentPolicy { get; set; }

        /// <summary>过期时间（Unix 秒，UTC）。</summary>
        [Description("过期时间")]
        public long ExpireAt { get; set; }
    }

    /// <summary>
    /// 预览 / 校验 Campaign 响应体。
    /// </summary>
    public sealed class PreviewMailCampaignResponse : HttpMessageResponseBase
    {
        /// <summary>业务码（Ok / InvalidCampaignParameter）。</summary>
        [Description("业务码")]
        public MailCampaignErrorCode Code { get; set; }

        /// <summary>预估命中玩家数。一期固定 -1（未知），需接入在线 / 角色统计模块后才有真实值。</summary>
        [Description("预估命中玩家数")]
        public long EstimatedHitCount { get; set; }

        /// <summary>提示信息。</summary>
        [Description("提示信息")]
        public string Message { get; set; }
    }
}
