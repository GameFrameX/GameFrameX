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

namespace GameFrameX.Apps.Player.Mail.Entity;

/// <summary>
/// 玩家个人邮件实例。实例化即冻结文案与 <see cref="ExpireTime"/>，后续不重算（U1 §3.4 / §4.5）。
/// </summary>
/// <remarks>
/// 状态机主线为 <see cref="MailStatus"/>（见 <see cref="GameFrameX.Apps.Player.Mail.MailStatus"/> 注释）；
/// <see cref="ReadStatus"/> / <see cref="AttachmentStatus"/> 为派生展示字段，单一事实来源是 <see cref="MailStatus"/> + 各 <see cref="Attachments"/>.<see cref="MailAttachmentInstance.ClaimStatus"/> + <see cref="ReadTime"/>，禁止独立写入造成漂移（U1 §4.3）。
/// </remarks>
public sealed class MailState
{
    /// <summary>玩家邮件实例 ID。玩家内唯一，实例化时分配。</summary>
    public long MailId { get; set; }

    /// <summary>来源发布记录 ID。</summary>
    public long CampaignId { get; set; }

    /// <summary>来源发布版本号。与 <see cref="MailBoxState.CreatedCampaignVersions"/> 的 <c>CampaignId@Version</c> 对应。</summary>
    public int CampaignVersion { get; set; }

    /// <summary>邮件业务类型。继承自 Campaign。</summary>
    public MailType MailType { get; set; }

    /// <summary>渲染后标题（来自 Campaign <c>LocalizedContentSnapshot</c> 按玩家语言选取）。实例化即冻结。</summary>
    public string Title { get; set; }

    /// <summary>渲染后正文。实例化即冻结。</summary>
    public string Content { get; set; }

    /// <summary>模板 ID。继承自 Campaign，便于审计与重渲染。</summary>
    public int TemplateId { get; set; }

    /// <summary>模板版本。继承自 Campaign。</summary>
    public long TemplateVersion { get; set; }

    /// <summary>模板参数。继承自 Campaign。</summary>
    public Dictionary<string, string> TemplateArgs { get; set; } = new Dictionary<string, string>();

    /// <summary>实例化时间（unix 秒）。懒创建发生时刻。</summary>
    public long CreateTime { get; set; }

    /// <summary>实例过期时间（unix 秒）。按 Campaign <c>ExpireAt</c> 或 <see cref="CreateTime"/> + <c>ExpireDays</c> 计算，实例化时一次性写定，后续不重算（U1 §4.5）。</summary>
    public long ExpireTime { get; set; }

    /// <summary>首次读信时间（unix 秒）。空表示未读。</summary>
    public long? ReadTime { get; set; }

    /// <summary>删除时间（unix 秒）。空表示未删除。</summary>
    public long? DeleteTime { get; set; }

    /// <summary>读信维度派生状态。</summary>
    public ReadStatus ReadStatus { get; set; } = ReadStatus.Unread;

    /// <summary>附件整体维度派生状态。</summary>
    public AttachmentStatus AttachmentStatus { get; set; } = AttachmentStatus.NoAttachment;

    /// <summary>邮件生命周期主线状态。终态 <see cref="MailStatus.Deleted"/> / <see cref="MailStatus.Revoked"/> / <see cref="MailStatus.Expired"/> 不可回退。</summary>
    public MailStatus MailStatus { get; set; } = MailStatus.Unread;

    /// <summary>附件实例列表。实例化时从 Campaign 复制元信息，承载领取状态。</summary>
    public List<MailAttachmentInstance> Attachments { get; set; } = new List<MailAttachmentInstance>();
}
