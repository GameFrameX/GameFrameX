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
using System.Collections.Generic;
using GameFrameX.Apps.Player.Mail;
using GameFrameX.Apps.Player.Mail.Entity;
using GameFrameX.Hotfix.Logic.Player.Mail;

namespace GameFrameX.Hotfix.Logic.Http.Mail
{
    /// <summary>
    /// Admin 发布运营邮件 Campaign。
    /// 路由：POST /api/mailCampaign/publish。
    /// 校验通过后写入不可变 <see cref="MailCampaignState"/> 快照（B1：发布后主体字段不可修改）。
    /// </summary>
    [HttpMessageMapping(typeof(PublishMailCampaignHttpHandler))]
    [HttpMessageRequest(typeof(PublishMailCampaignRequest))]
    [HttpMessageResponse(typeof(PublishMailCampaignResponse))]
    [Description("Admin 发布运营邮件 Campaign")]
    public sealed class PublishMailCampaignHttpHandler : BaseHttpHandler
    {
        /// <inheritdoc />
        public override async Task<string> Action(string ip, string url, HttpMessageRequestBase requestBase)
        {
            var request = (PublishMailCampaignRequest)requestBase;
            var response = new PublishMailCampaignResponse();

            var state = BuildCampaignState(request);
            var code = MailCampaignRegistry.Validate(state);
            if (code != MailCampaignErrorCode.Ok)
            {
                response.Code = code;
                response.Message = "Campaign 参数校验失败";
                return HttpJsonResult.SuccessString(response);
            }

            try
            {
                var published = MailCampaignRegistry.PublishOrUpdate(state, request.Operator, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                response.Code = MailCampaignErrorCode.Ok;
                response.CampaignId = published.CampaignId;
                response.Version = published.PublishVersion;
                response.PublishedAt = published.PublishedAt;
                response.Message = "发布成功";
                return HttpJsonResult.SuccessString(response);
            }
            catch (InvalidOperationException)
            {
                response.Code = MailCampaignErrorCode.HasExist;
                response.Message = "Campaign 已发布或已撤回，主体字段不可修改（B1）";
                return HttpJsonResult.SuccessString(response);
            }
            catch (ArgumentException)
            {
                response.Code = MailCampaignErrorCode.InvalidCampaignParameter;
                response.Message = "Campaign 参数非法";
                return HttpJsonResult.SuccessString(response);
            }
        }

        private static MailCampaignState BuildCampaignState(PublishMailCampaignRequest request)
        {
            var state = new MailCampaignState
            {
                CampaignId = request.CampaignId,
                TemplateId = request.TemplateId,
                MailType = request.MailType,
                MinLevel = request.MinLevel,
                MaxLevel = request.MaxLevel,
                ExpireAttachmentPolicy = request.ExpireAttachmentPolicy,
                ExpireAt = request.ExpireAt,
                Titles = request.Titles ?? new List<MailLocalizedContent>(),
                Contents = request.Contents ?? new List<MailLocalizedContent>(),
                SenderNames = request.SenderNames ?? new List<MailLocalizedContent>(),
                Attachments = request.Attachments ?? new List<MailAttachmentState>(),
                ServerIds = request.ServerIds ?? new List<int>(),
                ChannelIds = request.ChannelIds ?? new List<int>(),
            };
            return state;
        }
    }

    /// <summary>
    /// 发布 Campaign 请求体。<see cref="CampaignId"/> 为 0 表示新建；非 0 表示覆盖未发布的草稿。
    /// </summary>
    public sealed class PublishMailCampaignRequest : HttpMessageRequestBase
    {
        /// <summary>Campaign ID，0 表示新建。</summary>
        [Description("Campaign ID，0 表示新建")]
        public long CampaignId { get; set; }

        /// <summary>模板 ID，0 表示无模板。</summary>
        [Description("模板 ID")]
        public long TemplateId { get; set; }

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

        /// <summary>多语言发件人名称。</summary>
        [Description("多语言发件人名称")]
        public List<MailLocalizedContent> SenderNames { get; set; } = new List<MailLocalizedContent>();

        /// <summary>附件列表。</summary>
        [Description("附件列表")]
        public List<MailAttachmentState> Attachments { get; set; } = new List<MailAttachmentState>();

        /// <summary>命中过滤：服务器 ID 白名单，空表示不限。</summary>
        [Description("服务器 ID 白名单")]
        public List<int> ServerIds { get; set; } = new List<int>();

        /// <summary>命中过滤：渠道 ID 白名单，空表示不限。</summary>
        [Description("渠道 ID 白名单")]
        public List<int> ChannelIds { get; set; } = new List<int>();

        /// <summary>命中过滤：最低等级（含），0 表示不限。</summary>
        [Description("最低等级")]
        public int MinLevel { get; set; }

        /// <summary>命中过滤：最高等级（含），0 表示不限。</summary>
        [Description("最高等级")]
        public int MaxLevel { get; set; }

        /// <summary>附件过期处置策略（B4）。</summary>
        [Description("附件过期处置策略")]
        public ExpireAttachmentPolicy ExpireAttachmentPolicy { get; set; }

        /// <summary>过期时间（Unix 秒，UTC），0 表示永不过期。</summary>
        [Description("过期时间")]
        public long ExpireAt { get; set; }

        /// <summary>发布操作人（Admin 账号）。</summary>
        [Description("发布操作人")]
        public string Operator { get; set; }
    }

    /// <summary>
    /// 发布 Campaign 响应体。<see cref="Code"/> 非 Ok 时其余字段无意义。
    /// </summary>
    public sealed class PublishMailCampaignResponse : HttpMessageResponseBase
    {
        /// <summary>业务码（Ok / InvalidCampaignParameter / HasExist 等）。</summary>
        [Description("业务码")]
        public MailCampaignErrorCode Code { get; set; }

        /// <summary>分配 / 复用的 CampaignId。</summary>
        [Description("CampaignId")]
        public long CampaignId { get; set; }

        /// <summary>版本号。</summary>
        [Description("版本号")]
        public int Version { get; set; }

        /// <summary>发布时间（Unix 秒，UTC）。</summary>
        [Description("发布时间")]
        public long PublishedAt { get; set; }

        /// <summary>提示信息。</summary>
        [Description("提示信息")]
        public string Message { get; set; }
    }
}
