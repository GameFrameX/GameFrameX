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
using ProtoBuf;
using System.Collections.Generic;
using GameFrameX.NetWork.Abstractions;
using GameFrameX.NetWork.Messages;

namespace GameFrameX.Proto.Proto
{
	/// <summary>
	/// 邮件业务错误码（6 位 = 模块ID 500 + 3 位编号）。客户端协议 ErrorCode 以 int 透传：发生错误时赋具体码，成功不赋值（框架默认 0）。
	/// </summary>
	[System.ComponentModel.Description("邮件业务错误码（6 位 = 模块ID 500 + 3 位编号）。客户端协议 ErrorCode 以 int 透传：发生错误时赋具体码，成功不赋值（框架默认 0）。")]
	public enum MailErrorCode
	{
		/// <summary>
		/// 邮件不存在（或已被删除）
		/// </summary>
		[System.ComponentModel.Description("邮件不存在（或已被删除）")]
		MailNotFound = 500001,

		/// <summary>
		/// 邮件已处于删除终态
		/// </summary>
		[System.ComponentModel.Description("邮件已处于删除终态")]
		MailAlreadyDeleted = 500002,

		/// <summary>
		/// 存在未领取附件，禁止删除
		/// </summary>
		[System.ComponentModel.Description("存在未领取附件，禁止删除")]
		UnclaimedAttachment = 500003,

		/// <summary>
		/// 邮件附件槽位不存在
		/// </summary>
		[System.ComponentModel.Description("邮件附件槽位不存在")]
		AttachmentNotFound = 500004,

		/// <summary>
		/// 附件已领取（幂等命中）
		/// </summary>
		[System.ComponentModel.Description("附件已领取（幂等命中）")]
		AttachmentAlreadyClaimed = 500005,

		/// <summary>
		/// 附件不可领取（撤回 / 过期已作废）
		/// </summary>
		[System.ComponentModel.Description("附件不可领取（撤回 / 过期已作废）")]
		UnclaimableAttachment = 500006,
	}

	/// <summary>
	/// 邮件附件视图（含领取状态）。用于邮件列表与读信响应
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("邮件附件视图（含领取状态）。用于邮件列表与读信响应")]
	public sealed class MailAttachmentInfo
	{
		/// <summary>
		/// 附件槽位 ID（邮件内唯一）
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("附件槽位 ID（邮件内唯一）")]
		public int SlotId { get; set; }

		/// <summary>
		/// 奖励类型（0=普通道具，其余为预留路由位）
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("奖励类型（0=普通道具，其余为预留路由位）")]
		public int RewardType { get; set; }

		/// <summary>
		/// 物品 ID
		/// </summary>
		[ProtoMember(3)]
		[System.ComponentModel.Description("物品 ID")]
		public int ItemId { get; set; }

		/// <summary>
		/// 数量
		/// </summary>
		[ProtoMember(4)]
		[System.ComponentModel.Description("数量")]
		public long Count { get; set; }

		/// <summary>
		/// 领取状态（0=可领，1=已领，2=作废）
		/// </summary>
		[ProtoMember(5)]
		[System.ComponentModel.Description("领取状态（0=可领，1=已领，2=作废）")]
		public int ClaimStatus { get; set; }
	}

	/// <summary>
	/// 邮件摘要。用于列表展示
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("邮件摘要。用于列表展示")]
	public sealed class MailInfo
	{
		/// <summary>
		/// 玩家邮件实例 ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("玩家邮件实例 ID")]
		public long MailId { get; set; }

		/// <summary>
		/// 来源运营邮件活动 ID
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("来源运营邮件活动 ID")]
		public long CampaignId { get; set; }

		/// <summary>
		/// 来源发布版本号
		/// </summary>
		[ProtoMember(3)]
		[System.ComponentModel.Description("来源发布版本号")]
		public long CampaignVersion { get; set; }

		/// <summary>
		/// 邮件业务类型（MailType）
		/// </summary>
		[ProtoMember(4)]
		[System.ComponentModel.Description("邮件业务类型（MailType）")]
		public int MailType { get; set; }

		/// <summary>
		/// 标题
		/// </summary>
		[ProtoMember(5)]
		[System.ComponentModel.Description("标题")]
		public string Title { get; set; }

		/// <summary>
		/// 读信状态（0=未读，1=已读）
		/// </summary>
		[ProtoMember(6)]
		[System.ComponentModel.Description("读信状态（0=未读，1=已读）")]
		public int ReadStatus { get; set; }

		/// <summary>
		/// 附件整体状态（0=无附件，1=不可领，2=部分已领，3=全部已领）
		/// </summary>
		[ProtoMember(7)]
		[System.ComponentModel.Description("附件整体状态（0=无附件，1=不可领，2=部分已领，3=全部已领）")]
		public int AttachmentStatus { get; set; }

		/// <summary>
		/// 邮件生命周期状态（MailStatus）
		/// </summary>
		[ProtoMember(8)]
		[System.ComponentModel.Description("邮件生命周期状态（MailStatus）")]
		public int MailStatus { get; set; }

		/// <summary>
		/// 实例化时间（unix 秒）
		/// </summary>
		[ProtoMember(9)]
		[System.ComponentModel.Description("实例化时间（unix 秒）")]
		public long CreateTime { get; set; }

		/// <summary>
		/// 过期时间（unix 秒）
		/// </summary>
		[ProtoMember(10)]
		[System.ComponentModel.Description("过期时间（unix 秒）")]
		public long ExpireTime { get; set; }

		/// <summary>
		/// 是否存在附件
		/// </summary>
		[ProtoMember(11)]
		[System.ComponentModel.Description("是否存在附件")]
		public bool HasAttachment { get; set; }
	}

	/// <summary>
	/// 单个附件槽位的领取结果（单领 / 一键领取共用）
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("单个附件槽位的领取结果（单领 / 一键领取共用）")]
	public sealed class MailClaimedSlot
	{
		/// <summary>
		/// 邮件实例 ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("邮件实例 ID")]
		public long MailId { get; set; }

		/// <summary>
		/// 附件槽位 ID
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("附件槽位 ID")]
		public int SlotId { get; set; }

		/// <summary>
		/// 奖励类型（RewardType）
		/// </summary>
		[ProtoMember(3)]
		[System.ComponentModel.Description("奖励类型（RewardType）")]
		public int RewardType { get; set; }

		/// <summary>
		/// 物品 ID
		/// </summary>
		[ProtoMember(4)]
		[System.ComponentModel.Description("物品 ID")]
		public int ItemId { get; set; }

		/// <summary>
		/// 数量
		/// </summary>
		[ProtoMember(5)]
		[System.ComponentModel.Description("数量")]
		public long Count { get; set; }

		/// <summary>
		/// 领取状态（ClaimStatus）
		/// </summary>
		[ProtoMember(6)]
		[System.ComponentModel.Description("领取状态（ClaimStatus）")]
		public int ClaimStatus { get; set; }

		/// <summary>
		/// 是否发放成功
		/// </summary>
		[ProtoMember(7)]
		[System.ComponentModel.Description("是否发放成功")]
		public bool Success { get; set; }
	}

	/// <summary>
	/// 拉取邮件列表请求（触发懒同步）
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("拉取邮件列表请求（触发懒同步）")]
	[MessageTypeHandler(((500) << 16) + 10)]
	public sealed class ReqMailList : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 分页游标（已拉取条数偏移）
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("分页游标（已拉取条数偏移）")]
		public int Cursor { get; set; }

		/// <summary>
		/// 每页大小
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("每页大小")]
		public int PageSize { get; set; }

		public override void Clear()
		{
			Cursor = default;
			PageSize = default;
		}
	}

	/// <summary>
	/// 邮件列表响应
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("邮件列表响应")]
	[MessageTypeHandler(((500) << 16) + 11)]
	public sealed class RespMailList : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 邮件摘要列表
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("邮件摘要列表")]
		public List<MailInfo> Mails { get; set; } = new List<MailInfo>();

		/// <summary>
		/// 未读计数
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("未读计数")]
		public int UnreadCount { get; set; }

		/// <summary>
		/// 是否还有更多
		/// </summary>
		[ProtoMember(3)]
		[System.ComponentModel.Description("是否还有更多")]
		public bool HasMore { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			Mails.Clear();
			UnreadCount = default;
			HasMore = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 读信请求（标记已读，读信幂等）
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("读信请求（标记已读，读信幂等）")]
	[MessageTypeHandler(((500) << 16) + 12)]
	public sealed class ReqMailRead : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 邮件实例 ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("邮件实例 ID")]
		public long MailId { get; set; }

		public override void Clear()
		{
			MailId = default;
		}
	}

	/// <summary>
	/// 读信响应
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("读信响应")]
	[MessageTypeHandler(((500) << 16) + 13)]
	public sealed class RespMailRead : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 邮件实例 ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("邮件实例 ID")]
		public long MailId { get; set; }

		/// <summary>
		/// 标题
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("标题")]
		public string Title { get; set; }

		/// <summary>
		/// 正文
		/// </summary>
		[ProtoMember(3)]
		[System.ComponentModel.Description("正文")]
		public string Content { get; set; }

		/// <summary>
		/// 模板 ID（审计用）
		/// </summary>
		[ProtoMember(4)]
		[System.ComponentModel.Description("模板 ID（审计用）")]
		public int TemplateId { get; set; }

		/// <summary>
		/// 模板版本（审计用）
		/// </summary>
		[ProtoMember(5)]
		[System.ComponentModel.Description("模板版本（审计用）")]
		public long TemplateVersion { get; set; }

		/// <summary>
		/// 读信状态（ReadStatus）
		/// </summary>
		[ProtoMember(6)]
		[System.ComponentModel.Description("读信状态（ReadStatus）")]
		public int ReadStatus { get; set; }

		/// <summary>
		/// 邮件生命周期状态（MailStatus）
		/// </summary>
		[ProtoMember(7)]
		[System.ComponentModel.Description("邮件生命周期状态（MailStatus）")]
		public int MailStatus { get; set; }

		/// <summary>
		/// 附件视图列表（含领取状态）
		/// </summary>
		[ProtoMember(8)]
		[System.ComponentModel.Description("附件视图列表（含领取状态）")]
		public List<MailAttachmentInfo> Attachments { get; set; } = new List<MailAttachmentInfo>();

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			MailId = default;
			Title = default;
			Content = default;
			TemplateId = default;
			TemplateVersion = default;
			ReadStatus = default;
			MailStatus = default;
			Attachments.Clear();
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 删除邮件请求。未领取附件邮件会被拒绝
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("删除邮件请求。未领取附件邮件会被拒绝")]
	[MessageTypeHandler(((500) << 16) + 14)]
	public sealed class ReqMailDelete : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 邮件实例 ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("邮件实例 ID")]
		public long MailId { get; set; }

		public override void Clear()
		{
			MailId = default;
		}
	}

	/// <summary>
	/// 删除邮件响应
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("删除邮件响应")]
	[MessageTypeHandler(((500) << 16) + 15)]
	public sealed class RespMailDelete : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 邮件实例 ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("邮件实例 ID")]
		public long MailId { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			MailId = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 邮件变更通知（服务端推送）
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("邮件变更通知（服务端推送）")]
	[MessageTypeHandler(((500) << 16) + 16)]
	public sealed class NotifyMailChanged : MessageObject, INotifyMessage
	{
		/// <summary>
		/// 变更邮件实例 ID 列表（新建 / 状态变更）
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("变更邮件实例 ID 列表（新建 / 状态变更）")]
		public List<long> ChangedMailIds { get; set; } = new List<long>();

		/// <summary>
		/// 当前未读计数
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("当前未读计数")]
		public int UnreadCount { get; set; }

		public override void Clear()
		{
			ChangedMailIds.Clear();
			UnreadCount = default;
		}
	}

	/// <summary>
	/// 领取单个邮件附件请求
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("领取单个邮件附件请求")]
	[MessageTypeHandler(((500) << 16) + 17)]
	public sealed class ReqMailClaimAttachment : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 邮件实例 ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("邮件实例 ID")]
		public long MailId { get; set; }

		/// <summary>
		/// 附件槽位 ID
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("附件槽位 ID")]
		public int SlotId { get; set; }

		public override void Clear()
		{
			MailId = default;
			SlotId = default;
		}
	}

	/// <summary>
	/// 领取单个邮件附件响应
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("领取单个邮件附件响应")]
	[MessageTypeHandler(((500) << 16) + 18)]
	public sealed class RespMailClaimAttachment : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 邮件实例 ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("邮件实例 ID")]
		public long MailId { get; set; }

		/// <summary>
		/// 附件槽位 ID
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("附件槽位 ID")]
		public int SlotId { get; set; }

		/// <summary>
		/// 奖励类型（RewardType）
		/// </summary>
		[ProtoMember(3)]
		[System.ComponentModel.Description("奖励类型（RewardType）")]
		public int RewardType { get; set; }

		/// <summary>
		/// 物品 ID
		/// </summary>
		[ProtoMember(4)]
		[System.ComponentModel.Description("物品 ID")]
		public int ItemId { get; set; }

		/// <summary>
		/// 数量
		/// </summary>
		[ProtoMember(5)]
		[System.ComponentModel.Description("数量")]
		public long Count { get; set; }

		/// <summary>
		/// 本槽位领取后状态（ClaimStatus）
		/// </summary>
		[ProtoMember(6)]
		[System.ComponentModel.Description("本槽位领取后状态（ClaimStatus）")]
		public int ClaimStatus { get; set; }

		/// <summary>
		/// 邮件生命周期状态（MailStatus）
		/// </summary>
		[ProtoMember(7)]
		[System.ComponentModel.Description("邮件生命周期状态（MailStatus）")]
		public int MailStatus { get; set; }

		/// <summary>
		/// 附件整体状态（AttachmentStatus）
		/// </summary>
		[ProtoMember(8)]
		[System.ComponentModel.Description("附件整体状态（AttachmentStatus）")]
		public int AttachmentStatus { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			MailId = default;
			SlotId = default;
			RewardType = default;
			ItemId = default;
			Count = default;
			ClaimStatus = default;
			MailStatus = default;
			AttachmentStatus = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 一键领取全部可领附件请求（无参）
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("一键领取全部可领附件请求（无参）")]
	[MessageTypeHandler(((500) << 16) + 19)]
	public sealed class ReqMailClaimAllAttachment : MessageObject, IRequestMessage
	{

		public override void Clear()
		{
		}
	}

	/// <summary>
	/// 一键领取全部可领附件响应
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("一键领取全部可领附件响应")]
	[MessageTypeHandler(((500) << 16) + 20)]
	public sealed class RespMailClaimAllAttachment : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 逐槽位领取结果
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("逐槽位领取结果")]
		public List<MailClaimedSlot> Slots { get; set; } = new List<MailClaimedSlot>();

		/// <summary>
		/// 成功领取的槽位数
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("成功领取的槽位数")]
		public int ClaimedCount { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			Slots.Clear();
			ClaimedCount = default;
			ErrorCode = default;
		}
	}

}
