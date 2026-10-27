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
	/// 四子棋棋盘格子状态（值与座位号对应）
	/// </summary>
	[System.ComponentModel.Description("四子棋棋盘格子状态（值与座位号对应）")]
	public enum ConnectFourCell
	{
		/// <summary>
		/// 空
		/// </summary>
		[System.ComponentModel.Description("空")]
		None = 0,

		/// <summary>
		/// 一号位棋子
		/// </summary>
		[System.ComponentModel.Description("一号位棋子")]
		PlayerOne = 1,

		/// <summary>
		/// 二号位棋子
		/// </summary>
		[System.ComponentModel.Description("二号位棋子")]
		PlayerTwo = 2,
	}

	/// <summary>
	/// 四子棋玩家信息
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("四子棋玩家信息")]
	public sealed class ConnectFourPlayerInfo
	{
		/// <summary>
		/// 玩家ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("玩家ID")]
		public long PlayerId { get; set; }

		/// <summary>
		/// 座位索引（1 或 2，与棋子取值对应）
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("座位索引（1 或 2，与棋子取值对应）")]
		public int SeatIndex { get; set; }
	}

	/// <summary>
	/// 四子棋游戏信息
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("四子棋游戏信息")]
	public sealed class ConnectFourGameInfo
	{
		/// <summary>
		/// 房间ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("房间ID")]
		public long RoomId { get; set; }

		/// <summary>
		/// 当前局数
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("当前局数")]
		public int Round { get; set; }

		/// <summary>
		/// 当前回合玩家ID。未开始或已结束时为0。
		/// </summary>
		[ProtoMember(3)]
		[System.ComponentModel.Description("当前回合玩家ID。未开始或已结束时为0。")]
		public long CurrentPlayerId { get; set; }

		/// <summary>
		/// 胜利玩家ID。未分出胜负为0；平局时为0且IsDraw为true。
		/// </summary>
		[ProtoMember(4)]
		[System.ComponentModel.Description("胜利玩家ID。未分出胜负为0；平局时为0且IsDraw为true。")]
		public long WinnerPlayerId { get; set; }

		/// <summary>
		/// 是否平局（棋盘落满且无四连）
		/// </summary>
		[ProtoMember(5)]
		[System.ComponentModel.Description("是否平局（棋盘落满且无四连）")]
		public bool IsDraw { get; set; }

		/// <summary>
		/// 已落子总数
		/// </summary>
		[ProtoMember(6)]
		[System.ComponentModel.Description("已落子总数")]
		public int MoveCount { get; set; }

		/// <summary>
		/// 最近一次落子列（0-6）。无落子为-1。
		/// </summary>
		[ProtoMember(7)]
		[System.ComponentModel.Description("最近一次落子列（0-6）。无落子为-1。")]
		public int LastMoveColumn { get; set; }

		/// <summary>
		/// 最近一次落子行（0-5，0为底行）。无落子为-1。
		/// </summary>
		[ProtoMember(8)]
		[System.ComponentModel.Description("最近一次落子行（0-5，0为底行）。无落子为-1。")]
		public int LastMoveRow { get; set; }

		/// <summary>
		/// 棋盘42格，行主序 index = row*7+column，row 0 为底行；取值见 ConnectFourCell
		/// </summary>
		[ProtoMember(9)]
		[System.ComponentModel.Description("棋盘42格，行主序 index = row*7+column，row 0 为底行；取值见 ConnectFourCell")]
		public List<int> Board { get; set; } = new List<int>();

		/// <summary>
		/// 玩家列表
		/// </summary>
		[ProtoMember(10)]
		[System.ComponentModel.Description("玩家列表")]
		public List<ConnectFourPlayerInfo> Players { get; set; } = new List<ConnectFourPlayerInfo>();
	}

	/// <summary>
	/// 请求四子棋游戏信息
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("请求四子棋游戏信息")]
	[MessageTypeHandler(((470) << 16) + 10)]
	public sealed class ReqConnectFourGameInfo : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 房间ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("房间ID")]
		public long RoomId { get; set; }

		public override void Clear()
		{
			RoomId = default;
		}
	}

	/// <summary>
	/// 返回四子棋游戏信息
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("返回四子棋游戏信息")]
	[MessageTypeHandler(((470) << 16) + 11)]
	public sealed class RespConnectFourGameInfo : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 游戏信息
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("游戏信息")]
		public ConnectFourGameInfo GameInfo { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			GameInfo = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 请求四子棋落子
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("请求四子棋落子")]
	[MessageTypeHandler(((470) << 16) + 12)]
	public sealed class ReqSubmitConnectFourMove : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 房间ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("房间ID")]
		public long RoomId { get; set; }

		/// <summary>
		/// 落子列（0-6）
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("落子列（0-6）")]
		public int Column { get; set; }

		public override void Clear()
		{
			RoomId = default;
			Column = default;
		}
	}

	/// <summary>
	/// 返回四子棋落子
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("返回四子棋落子")]
	[MessageTypeHandler(((470) << 16) + 13)]
	public sealed class RespSubmitConnectFourMove : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 游戏信息
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("游戏信息")]
		public ConnectFourGameInfo GameInfo { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			GameInfo = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 请求四子棋再来一局
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("请求四子棋再来一局")]
	[MessageTypeHandler(((470) << 16) + 14)]
	public sealed class ReqRestartConnectFourGame : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 房间ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("房间ID")]
		public long RoomId { get; set; }

		public override void Clear()
		{
			RoomId = default;
		}
	}

	/// <summary>
	/// 返回四子棋再来一局
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("返回四子棋再来一局")]
	[MessageTypeHandler(((470) << 16) + 15)]
	public sealed class RespRestartConnectFourGame : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 游戏信息
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("游戏信息")]
		public ConnectFourGameInfo GameInfo { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			GameInfo = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 通知四子棋游戏变化
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("通知四子棋游戏变化")]
	[MessageTypeHandler(((470) << 16) + 16)]
	public sealed class NotifyConnectFourGameChanged : MessageObject, INotifyMessage
	{
		/// <summary>
		/// 游戏信息
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("游戏信息")]
		public ConnectFourGameInfo GameInfo { get; set; }

		public override void Clear()
		{
			GameInfo = default;
		}
	}

}
