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
	/// 游戏类型
	/// </summary>
	public enum GameType
	{
		/// <summary>
		/// 未知
		/// </summary>
		None = 0,

		/// <summary>
		/// 石头剪刀布
		/// </summary>
		RockPaperScissors = 1,
	}

	/// <summary>
	/// 通用房间状态
	/// </summary>
	public enum RoomStatus
	{
		/// <summary>
		/// 无效状态
		/// </summary>
		None = 0,

		/// <summary>
		/// 等待中
		/// </summary>
		Waiting = 1,

		/// <summary>
		/// 可开始
		/// </summary>
		Ready = 2,

		/// <summary>
		/// 游戏中
		/// </summary>
		Playing = 3,

		/// <summary>
		/// 结算中
		/// </summary>
		Settling = 4,

		/// <summary>
		/// 已结算
		/// </summary>
		Settled = 5,

		/// <summary>
		/// 已关闭
		/// </summary>
		Closed = 6,

		/// <summary>
		/// 已解散
		/// </summary>
		Disbanded = 7,
	}

	/// <summary>
	/// 通用房间变更类型
	/// </summary>
	public enum RoomChangeType
	{
		/// <summary>
		/// 创建
		/// </summary>
		Created = 0,

		/// <summary>
		/// 加入
		/// </summary>
		Joined = 1,

		/// <summary>
		/// 退出
		/// </summary>
		Left = 2,

		/// <summary>
		/// 开始
		/// </summary>
		Started = 3,

		/// <summary>
		/// 进入结算
		/// </summary>
		Settling = 4,

		/// <summary>
		/// 已结算
		/// </summary>
		Settled = 5,

		/// <summary>
		/// 关闭
		/// </summary>
		Closed = 6,

		/// <summary>
		/// 解散
		/// </summary>
		Disbanded = 7,

		/// <summary>
		/// 重置
		/// </summary>
		Reset = 8,
	}

	/// <summary>
	/// 通用房间玩家在线状态
	/// </summary>
	public enum RoomPlayerOnlineStatus
	{
		/// <summary>
		/// 未知
		/// </summary>
		OnlineUnknown = 0,

		/// <summary>
		/// 在线
		/// </summary>
		Online = 1,

		/// <summary>
		/// 断线重连中
		/// </summary>
		Reconnecting = 2,

		/// <summary>
		/// 离线
		/// </summary>
		Offline = 3,
	}

	/// <summary>
	/// 通用房间玩家状态
	/// </summary>
	public enum RoomPlayerStatus
	{
		/// <summary>
		/// 无状态
		/// </summary>
		PlayerStatusNone = 0,

		/// <summary>
		/// 等待中
		/// </summary>
		Idle = 1,

		/// <summary>
		/// 可开始
		/// </summary>
		ReadyInRoom = 2,

		/// <summary>
		/// 游戏中
		/// </summary>
		InGame = 3,

		/// <summary>
		/// 已提交操作
		/// </summary>
		Submitted = 4,

		/// <summary>
		/// 结算中
		/// </summary>
		SettlingInRoom = 5,

		/// <summary>
		/// 已结算
		/// </summary>
		SettledInRoom = 6,
	}

	/// <summary>
	/// 通用房间玩家信息
	/// </summary>
	[ProtoContract]
	public sealed class RoomPlayerInfo
	{
		/// <summary>
		/// 玩家ID
		/// </summary>
		[ProtoMember(1)]
		public long RoleId { get; set; }

		/// <summary>
		/// 座位索引
		/// </summary>
		[ProtoMember(2)]
		public int SeatIndex { get; set; }

		/// <summary>
		/// 是否房主
		/// </summary>
		[ProtoMember(3)]
		public bool IsOwner { get; set; }

		/// <summary>
		/// 玩家昵称
		/// </summary>
		[ProtoMember(4)]
		public string Name { get; set; }

		/// <summary>
		/// 头像ID
		/// </summary>
		[ProtoMember(5)]
		public uint Avatar { get; set; }

		/// <summary>
		/// 在线状态
		/// </summary>
		[ProtoMember(6)]
		public RoomPlayerOnlineStatus OnlineStatus { get; set; }

		/// <summary>
		/// 房间内状态
		/// </summary>
		[ProtoMember(7)]
		public RoomPlayerStatus PlayerStatus { get; set; }
	}

	/// <summary>
	/// 通用房间信息
	/// </summary>
	[ProtoContract]
	public sealed class RoomInfo
	{
		/// <summary>
		/// 房间ID
		/// </summary>
		[ProtoMember(1)]
		public long RoomId { get; set; }

		/// <summary>
		/// 房间名称
		/// </summary>
		[ProtoMember(2)]
		public string Name { get; set; }

		/// <summary>
		/// 游戏类型
		/// </summary>
		[ProtoMember(3)]
		public GameType GameType { get; set; }

		/// <summary>
		/// 当前状态
		/// </summary>
		[ProtoMember(4)]
		public RoomStatus Status { get; set; }

		/// <summary>
		/// 当前玩家数量
		/// </summary>
		[ProtoMember(5)]
		public int PlayerCount { get; set; }

		/// <summary>
		/// 最少玩家数量
		/// </summary>
		[ProtoMember(6)]
		public int MinPlayerCount { get; set; }

		/// <summary>
		/// 最大玩家数量
		/// </summary>
		[ProtoMember(7)]
		public int MaxPlayerCount { get; set; }

		/// <summary>
		/// 房主玩家ID
		/// </summary>
		[ProtoMember(8)]
		public long OwnerRoleId { get; set; }

		/// <summary>
		/// 玩家列表
		/// </summary>
		[ProtoMember(9)]
		public List<RoomPlayerInfo> Players { get; set; } = new List<RoomPlayerInfo>();

		/// <summary>
		/// 当前局数
		/// </summary>
		[ProtoMember(10)]
		public int Round { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		[ProtoMember(11)]
		public long CreatedTime { get; set; }

		/// <summary>
		/// 更新时间
		/// </summary>
		[ProtoMember(12)]
		public long UpdatedTime { get; set; }
	}

	/// <summary>
	/// 请求通用房间列表
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((400) << 16) + 10)]
	public sealed class ReqRoomList : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 游戏类型。None表示全部。
		/// </summary>
		[ProtoMember(1)]
		public GameType GameType { get; set; }

		/// <summary>
		/// 是否包含已关闭/已解散房间
		/// </summary>
		[ProtoMember(2)]
		public bool IncludeClosed { get; set; }

		public override void Clear()
		{
			GameType = default;
			IncludeClosed = default;
		}
	}

	/// <summary>
	/// 返回通用房间列表
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((400) << 16) + 11)]
	public sealed class RespRoomList : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 房间列表
		/// </summary>
		[ProtoMember(1)]
		public List<RoomInfo> Rooms { get; set; } = new List<RoomInfo>();

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			Rooms.Clear();
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 请求创建通用房间
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((400) << 16) + 12)]
	public sealed class ReqCreateRoom : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 游戏类型
		/// </summary>
		[ProtoMember(1)]
		public GameType GameType { get; set; }

		/// <summary>
		/// 房间名称
		/// </summary>
		[ProtoMember(2)]
		public string Name { get; set; }

		/// <summary>
		/// 最少玩家数量。不传则使用游戏默认配置。
		/// </summary>
		[ProtoMember(3)]
		public int MinPlayerCount { get; set; }

		/// <summary>
		/// 最大玩家数量。不传则使用游戏默认配置。
		/// </summary>
		[ProtoMember(4)]
		public int MaxPlayerCount { get; set; }

		public override void Clear()
		{
			GameType = default;
			Name = default;
			MinPlayerCount = default;
			MaxPlayerCount = default;
		}
	}

	/// <summary>
	/// 返回创建通用房间
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((400) << 16) + 13)]
	public sealed class RespCreateRoom : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 房间信息
		/// </summary>
		[ProtoMember(1)]
		public RoomInfo Room { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			Room = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 请求加入通用房间
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((400) << 16) + 14)]
	public sealed class ReqJoinRoom : MessageObject, IRequestMessage
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
	/// 返回加入通用房间
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((400) << 16) + 15)]
	public sealed class RespJoinRoom : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 房间信息
		/// </summary>
		[ProtoMember(1)]
		public RoomInfo Room { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			Room = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 请求退出通用房间
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((400) << 16) + 16)]
	public sealed class ReqLeaveRoom : MessageObject, IRequestMessage
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
	/// 返回退出通用房间
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((400) << 16) + 17)]
	public sealed class RespLeaveRoom : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 房间信息
		/// </summary>
		[ProtoMember(1)]
		public RoomInfo Room { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			Room = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 请求开始通用房间游戏
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((400) << 16) + 18)]
	public sealed class ReqStartRoomGame : MessageObject, IRequestMessage
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
	/// 返回开始通用房间游戏
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((400) << 16) + 19)]
	public sealed class RespStartRoomGame : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 房间信息
		/// </summary>
		[ProtoMember(1)]
		public RoomInfo Room { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			Room = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 通知通用房间变化
	/// </summary>
	[ProtoContract]
	[MessageTypeHandler(((400) << 16) + 20)]
	public sealed class NotifyRoomChanged : MessageObject, INotifyMessage
	{
		/// <summary>
		/// 变化类型
		/// </summary>
		[ProtoMember(1)]
		public RoomChangeType ChangeType { get; set; }

		/// <summary>
		/// 房间信息
		/// </summary>
		[ProtoMember(2)]
		public RoomInfo Room { get; set; }

		public override void Clear()
		{
			ChangeType = default;
			Room = default;
		}
	}

}
