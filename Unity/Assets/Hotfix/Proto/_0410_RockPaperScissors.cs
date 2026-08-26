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
using GameFrameX.Network.Runtime;

namespace Hotfix.Proto
{
	/// <summary>
	/// 石头剪刀布出拳
	/// </summary>
	public enum RockPaperScissorsGesture
	{
		/// <summary>
		/// 未出拳
		/// </summary>
		None = 0,

		/// <summary>
		/// 石头
		/// </summary>
		Rock = 1,

		/// <summary>
		/// 剪刀
		/// </summary>
		Scissors = 2,

		/// <summary>
		/// 布
		/// </summary>
		Paper = 3,
	}

	/// <summary>
	/// 石头剪刀布玩家信息
	/// </summary>
	[ProtoContract]
	public sealed class RockPaperScissorsPlayerInfo
	{
		/// <summary>
		/// 玩家ID
		/// </summary>
		[ProtoMember(1)]
		public long RoleId { get; set; }

		/// <summary>
		/// 是否已出拳
		/// </summary>
		[ProtoMember(2)]
		public bool HasGesture { get; set; }

		/// <summary>
		/// 出拳。未结算前对外为None，结算后显示真实出拳。
		/// </summary>
		[ProtoMember(3)]
		public RockPaperScissorsGesture Gesture { get; set; }
	}

	/// <summary>
	/// 石头剪刀布游戏信息
	/// </summary>
	[ProtoContract]
	public sealed class RockPaperScissorsGameInfo
	{
		/// <summary>
		/// 房间ID
		/// </summary>
		[ProtoMember(1)]
		public long RoomId { get; set; }

		/// <summary>
		/// 当前局数
		/// </summary>
		[ProtoMember(2)]
		public int Round { get; set; }

		/// <summary>
		/// 胜利玩家ID。平局为0。
		/// </summary>
		[ProtoMember(3)]
		public long WinnerRoleId { get; set; }

		/// <summary>
		/// 玩家列表
		/// </summary>
		[ProtoMember(4)]
		public List<RockPaperScissorsPlayerInfo> Players { get; set; } = new List<RockPaperScissorsPlayerInfo>();
	}

	/// <summary>
	/// 请求石头剪刀布游戏信息
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((410) << 16) + 10)]
	public sealed class ReqRockPaperScissorsGameInfo : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 房间ID
		/// </summary>
		[ProtoMember(1)]
		public long RoomId { get; set; }

		public override void Clear()
		{
			RoomId = default;
		}
	}

	/// <summary>
	/// 返回石头剪刀布游戏信息
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((410) << 16) + 11)]
	public sealed class RespRockPaperScissorsGameInfo : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 游戏信息
		/// </summary>
		[ProtoMember(1)]
		public RockPaperScissorsGameInfo GameInfo { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			GameInfo = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 请求石头剪刀布出拳
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((410) << 16) + 12)]
	public sealed class ReqSubmitRockPaperScissorsGesture : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 房间ID
		/// </summary>
		[ProtoMember(1)]
		public long RoomId { get; set; }

		/// <summary>
		/// 出拳
		/// </summary>
		[ProtoMember(2)]
		public RockPaperScissorsGesture Gesture { get; set; }

		public override void Clear()
		{
			RoomId = default;
			Gesture = default;
		}
	}

	/// <summary>
	/// 返回石头剪刀布出拳
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((410) << 16) + 13)]
	public sealed class RespSubmitRockPaperScissorsGesture : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 游戏信息
		/// </summary>
		[ProtoMember(1)]
		public RockPaperScissorsGameInfo GameInfo { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			GameInfo = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 请求石头剪刀布再来一局
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((410) << 16) + 14)]
	public sealed class ReqRestartRockPaperScissorsGame : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 房间ID
		/// </summary>
		[ProtoMember(1)]
		public long RoomId { get; set; }

		public override void Clear()
		{
			RoomId = default;
		}
	}

	/// <summary>
	/// 返回石头剪刀布再来一局
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((410) << 16) + 15)]
	public sealed class RespRestartRockPaperScissorsGame : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 游戏信息
		/// </summary>
		[ProtoMember(1)]
		public RockPaperScissorsGameInfo GameInfo { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			GameInfo = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 通知石头剪刀布游戏变化
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((410) << 16) + 16)]
	public sealed class NotifyRockPaperScissorsGameChanged : MessageObject, INotifyMessage
	{
		/// <summary>
		/// 游戏信息
		/// </summary>
		[ProtoMember(1)]
		public RockPaperScissorsGameInfo GameInfo { get; set; }

		public override void Clear()
		{
			GameInfo = default;
		}
	}

}
