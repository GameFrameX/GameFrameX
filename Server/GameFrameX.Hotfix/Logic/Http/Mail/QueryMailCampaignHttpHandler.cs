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

using System.Collections.Generic;
using GameFrameX.Apps.Player.Mail;
using GameFrameX.Apps.Player.Mail.Entity;
using GameFrameX.Hotfix.Logic.Player.Mail;

namespace GameFrameX.Hotfix.Logic.Http.Mail
{
    /// <summary>
    /// Admin 查询运营邮件 Campaign 发布状态。
    /// 路由：GET /api/mailCampaign/query。
    /// <see cref="QueryMailCampaignRequest.CampaignId"/> 大于 0 时按 ID 精确查询单条；否则按过滤条件列表查询。
    /// </summary>
    [HttpMessageMapping(typeof(QueryMailCampaignHttpHandler))]
    [HttpMessageRequest(typeof(QueryMailCampaignRequest))]
    [HttpMessageResponse(typeof(QueryMailCampaignResponse))]
    [Description("Admin 查询运营邮件 Campaign 发布状态")]
    public sealed class QueryMailCampaignHttpHandler : BaseHttpHandler
    {
        /// <inheritdoc />
        public override async Task<string> Action(string ip, string url, HttpMessageRequestBase requestBase)
        {
            var request = (QueryMailCampaignRequest)requestBase;
            var response = new QueryMailCampaignResponse { Code = MailCampaignErrorCode.Ok };

            if (request.CampaignId > 0)
            {
                if (MailCampaignRegistry.TryQuery(request.CampaignId, out var single))
                {
                    response.Campaigns.Add(single);
                    response.Total = 1;
                }
                else
                {
                    response.Code = MailCampaignErrorCode.CampaignNotFound;
                    response.Message = "Campaign 不存在";
                }

                return HttpJsonResult.SuccessString(response);
            }

            var list = MailCampaignRegistry.QueryAll(
                status: request.Status,
                mailType: request.MailType,
                serverId: request.ServerId,
                channelId: request.ChannelId,
                minLevel: request.MinLevel,
                createdFromUnixSeconds: request.CreatedFrom,
                createdToUnixSeconds: request.CreatedTo,
                limit: request.Limit);

            response.Campaigns = list;
            response.Total = list.Count;
            response.Message = "查询成功";
            return HttpJsonResult.SuccessString(response);
        }
    }

    /// <summary>
    /// 查询 Campaign 请求体。所有过滤条件为 AND 关系，空 / 0 表示不限。
    /// </summary>
    public sealed class QueryMailCampaignRequest : HttpMessageRequestBase
    {
        /// <summary>Campaign ID。大于 0 时精确查询单条，忽略其余过滤条件。</summary>
        [Description("Campaign ID")]
        public long CampaignId { get; set; }

        /// <summary>状态过滤；null 表示不限。</summary>
        [Description("状态过滤")]
        public MailCampaignStatus? Status { get; set; }

        /// <summary>类型过滤；null 表示不限。</summary>
        [Description("类型过滤")]
        public MailType? MailType { get; set; }

        /// <summary>服务器 ID 过滤；≤ 0 表示不限。</summary>
        [Description("服务器 ID")]
        public int ServerId { get; set; }

        /// <summary>渠道 ID 过滤；≤ 0 表示不限。</summary>
        [Description("渠道 ID")]
        public int ChannelId { get; set; }

        /// <summary>最低等级过滤；≤ 0 表示不限。</summary>
        [Description("最低等级")]
        public int MinLevel { get; set; }

        /// <summary>创建时间下限（Unix 秒，UTC）；≤ 0 表示不限。</summary>
        [Description("创建时间下限")]
        public long CreatedFrom { get; set; }

        /// <summary>创建时间上限（Unix 秒，UTC）；≤ 0 表示不限。</summary>
        [Description("创建时间上限")]
        public long CreatedTo { get; set; }

        /// <summary>返回条数上限；≤ 0 表示不限。</summary>
        [Description("返回条数上限")]
        public int Limit { get; set; }
    }

    /// <summary>
    /// 查询 Campaign 响应体。
    /// </summary>
    public sealed class QueryMailCampaignResponse : HttpMessageResponseBase
    {
        /// <summary>业务码。</summary>
        [Description("业务码")]
        public MailCampaignErrorCode Code { get; set; }

        /// <summary>命中 Campaign 列表。</summary>
        [Description("Campaign 列表")]
        public List<MailCampaignState> Campaigns { get; set; } = new List<MailCampaignState>();

        /// <summary>命中总数。</summary>
        [Description("命中总数")]
        public int Total { get; set; }

        /// <summary>提示信息。</summary>
        [Description("提示信息")]
        public string Message { get; set; }
    }
}
