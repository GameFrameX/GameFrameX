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
	/// 玩家属性条目，含最终值与 Base/Add/Pct 调试字段
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("玩家属性条目，含最终值与 Base/Add/Pct 调试字段")]
	public sealed class PlayerAttributeEntry
	{
		/// <summary>
		/// 属性编号（AttributeType）
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("属性编号（AttributeType）")]
		public int Type { get; set; }

		/// <summary>
		/// 最终值
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("最终值")]
		public long Value { get; set; }

		/// <summary>
		/// 基础值（调试字段）
		/// </summary>
		[ProtoMember(3)]
		[System.ComponentModel.Description("基础值（调试字段）")]
		public long Base { get; set; }

		/// <summary>
		/// 加法修正（调试字段）
		/// </summary>
		[ProtoMember(4)]
		[System.ComponentModel.Description("加法修正（调试字段）")]
		public long Add { get; set; }

		/// <summary>
		/// 百分比修正，10000 表示 100%（调试字段）
		/// </summary>
		[ProtoMember(5)]
		[System.ComponentModel.Description("百分比修正，10000 表示 100%（调试字段）")]
		public long Pct { get; set; }
	}

	/// <summary>
	/// 玩家属性完整快照（服务端推送，登录后或重连时发送）
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("玩家属性完整快照（服务端推送，登录后或重连时发送）")]
	[MessageTypeHandler(((310) << 16) + 10)]
	public sealed class NotifyPlayerAttributeSync : MessageObject, INotifyMessage
	{
		/// <summary>
		/// 全部最终属性条目
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("全部最终属性条目")]
		public List<PlayerAttributeEntry> Attributes { get; set; } = new List<PlayerAttributeEntry>();

		public override void Clear()
		{
			Attributes.Clear();
		}
	}

	/// <summary>
	/// 单个玩家属性最终值变化（服务端推送增量）
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("单个玩家属性最终值变化（服务端推送增量）")]
	[MessageTypeHandler(((310) << 16) + 11)]
	public sealed class NotifyPlayerAttributeChanged : MessageObject, INotifyMessage
	{
		/// <summary>
		/// 发生变化的最终属性编号（AttributeType）
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("发生变化的最终属性编号（AttributeType）")]
		public int Type { get; set; }

		/// <summary>
		/// 新的最终值
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("新的最终值")]
		public long Value { get; set; }

		public override void Clear()
		{
			Type = default;
			Value = default;
		}
	}

}
