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
using GameFrameX.DataBase.Mongo;

namespace GameFrameX.Apps.Player.Mail.Entity
{
    /// <summary>
    /// 运营邮件发布记录（Campaign）不可变快照。
    /// 由 Admin 发布接口（<c>PublishMailCampaignHttpHandler</c>）写入，发布后主体字段不可修改（B1：发布后不可修改），
    /// 仅 <see cref="Status"/> / <see cref="RevokedAt"/> / <see cref="RevokeOperator"/> 可通过撤回接口流转。
    /// </summary>
    public sealed class MailCampaignState : CacheState
    {
        /// <summary>
        /// Campaign 唯一 ID。由发布接口生成（雪花 ID 或自增），全局唯一。
        /// </summary>
        public long CampaignId { get; set; }

        /// <summary>
        /// 模板 ID。引用运营邮件模板（多语言标题/正文/附件预设），0 表示无模板（裸文案发布）。
        /// </summary>
        public long TemplateId { get; set; }

        /// <summary>
        /// 发布版本号。每次重新发布同一 Campaign（修正未发布草稿）自增；发布成功后不可再变更主体字段，版本随之冻结。
        /// 与持久化层 <c>EntityBase.Version</c>（乐观并发控制）语义不同，单独命名为 <see cref="PublishVersion"/>。
        /// </summary>
        public int PublishVersion { get; set; }

        /// <summary>
        /// 生命周期状态。发布后为 <c>Published</c>，撤回后为 <c>Revoked</c>。撤回不可逆。
        /// </summary>
        public MailCampaignStatus Status { get; set; }

        /// <summary>
        /// 邮件类型（系统 / 运营 / 补偿）。决定展示样式与过滤命中策略。
        /// </summary>
        public MailType MailType { get; set; }

        /// <summary>
        /// 多语言标题列表。至少含一种语言，缺失语言由客户端回退到首项。
        /// </summary>
        public List<MailLocalizedContent> Titles { get; set; } = new List<MailLocalizedContent>();

        /// <summary>
        /// 多语言正文列表。规则同 <see cref="Titles"/>。
        /// </summary>
        public List<MailLocalizedContent> Contents { get; set; } = new List<MailLocalizedContent>();

        /// <summary>
        /// 多语言发件人名称列表。规则同 <see cref="Titles"/>。
        /// </summary>
        public List<MailLocalizedContent> SenderNames { get; set; } = new List<MailLocalizedContent>();

        /// <summary>
        /// 附件槽位列表。玩家领取时按 <c>SlotId</c> 逐项走统一奖励发放接口。
        /// </summary>
        public List<MailAttachmentState> Attachments { get; set; } = new List<MailAttachmentState>();

        /// <summary>
        /// 命中过滤：服务器 ID 白名单。空列表表示不限服务器。
        /// </summary>
        public List<int> ServerIds { get; set; } = new List<int>();

        /// <summary>
        /// 命中过滤：渠道 ID 白名单。空列表表示不限渠道。
        /// </summary>
        public List<int> ChannelIds { get; set; } = new List<int>();

        /// <summary>
        /// 命中过滤：最低玩家等级（含）。0 表示不设下限。
        /// </summary>
        public int MinLevel { get; set; }

        /// <summary>
        /// 命中过滤：最高玩家等级（含）。0 表示不设上限。
        /// </summary>
        public int MaxLevel { get; set; }

        /// <summary>
        /// 附件过期处置策略（B4）。撤回或自然过期后未领取附件的处理方式。
        /// </summary>
        public ExpireAttachmentPolicy ExpireAttachmentPolicy { get; set; }

        /// <summary>
        /// 邮件过期时间（Unix 秒，UTC）。0 表示永不过期；过期后未领取附件按 <see cref="ExpireAttachmentPolicy"/> 处理。
        /// </summary>
        public long ExpireAt { get; set; }

        /// <summary>
        /// 发布时间（Unix 秒，UTC）。Admin 发布接口写入时戳。
        /// </summary>
        public long PublishedAt { get; set; }

        /// <summary>
        /// 撤回时间（Unix 秒，UTC）。未撤回时为 0；撤回后写入时间戳，不可清零。
        /// </summary>
        public long RevokedAt { get; set; }

        /// <summary>
        /// Campaign 创建时间（Unix 秒，UTC）。Admin 查询发布状态时按此字段过滤。
        /// </summary>
        public long CreateTime { get; set; }

        /// <summary>
        /// 发布操作人（Admin 账号名）。用于审计。
        /// </summary>
        public string PublishOperator { get; set; }

        /// <summary>
        /// 撤回操作人（Admin 账号名）。未撤回时为 null。
        /// </summary>
        public string RevokeOperator { get; set; }
    }
}
