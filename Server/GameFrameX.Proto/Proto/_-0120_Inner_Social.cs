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
	/// 请求添加好友
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("请求添加好友")]
	[MessageTypeHandler(((-120) << 16) + 10)]
	public sealed class ReqInnerFriendByAdd : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 玩家ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("玩家ID")]
		public long PlayerId { get; set; }

		public override void Clear()
		{
			PlayerId = default;
		}
	}

	/// <summary>
	/// 响应添加好友
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("响应添加好友")]
	[MessageTypeHandler(((-120) << 16) + 11)]
	public sealed class RespInnerFriendByAdd : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 是否成功
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("是否成功")]
		public bool Success { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			Success = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 内部好友信息
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("内部好友信息")]
	public sealed class InnerFriendInfo
	{
		/// <summary>
		/// 玩家ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("玩家ID")]
		public long PlayerId { get; set; }

		/// <summary>
		/// 玩家名称
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("玩家名称")]
		public string PlayerName { get; set; }
	}

	/// <summary>
	/// 请求删除好友
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("请求删除好友")]
	[MessageTypeHandler(((-120) << 16) + 12)]
	public sealed class ReqInnerFriendByDelete : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 玩家ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("玩家ID")]
		public long PlayerId { get; set; }

		public override void Clear()
		{
			PlayerId = default;
		}
	}

	/// <summary>
	/// 响应删除好友
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("响应删除好友")]
	[MessageTypeHandler(((-120) << 16) + 13)]
	public sealed class RespInnerFriendByDelete : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 是否成功
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("是否成功")]
		public bool Success { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			Success = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 请求好友列表
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("请求好友列表")]
	[MessageTypeHandler(((-120) << 16) + 14)]
	public sealed class ReqInnerFriendList : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 玩家ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("玩家ID")]
		public long PlayerId { get; set; }

		public override void Clear()
		{
			PlayerId = default;
		}
	}

	/// <summary>
	/// 响应好友列表
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("响应好友列表")]
	[MessageTypeHandler(((-120) << 16) + 15)]
	public sealed class RespInnerFriendList : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 好友列表
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("好友列表")]
		public List<InnerFriendInfo> Friends { get; set; } = new List<InnerFriendInfo>();

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			Friends.Clear();
			ErrorCode = default;
		}
	}

}
