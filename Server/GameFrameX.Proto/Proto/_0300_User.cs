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
	/// 请求账号登录
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("请求账号登录")]
	[MessageTypeHandler(((300) << 16) + 10)]
	public sealed class ReqLogin : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 用户名
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("用户名")]
		public string UserName { get; set; }

		/// <summary>
		/// 平台
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("平台")]
		public string Platform { get; set; }

		/// <summary>
		/// SDK类型
		/// </summary>
		[ProtoMember(3)]
		[System.ComponentModel.Description("SDK类型")]
		public int SdkType { get; set; }

		/// <summary>
		/// SDK令牌
		/// </summary>
		[ProtoMember(4)]
		[System.ComponentModel.Description("SDK令牌")]
		public string SdkToken { get; set; }

		/// <summary>
		/// 设备信息
		/// </summary>
		[ProtoMember(5)]
		[System.ComponentModel.Description("设备信息")]
		public string Device { get; set; }

		/// <summary>
		/// 密码
		/// </summary>
		[ProtoMember(6)]
		[System.ComponentModel.Description("密码")]
		public string Password { get; set; }

		public override void Clear()
		{
			UserName = default;
			Platform = default;
			SdkType = default;
			SdkToken = default;
			Device = default;
			Password = default;
		}
	}

	/// <summary>
	/// 返回账号登录
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("返回账号登录")]
	[MessageTypeHandler(((300) << 16) + 11)]
	public sealed class RespLogin : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 返回码（ResultCode）
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("返回码（ResultCode）")]
		public int Code { get; set; }

		/// <summary>
		/// 账号名
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("账号名")]
		public string RoleName { get; set; }

		/// <summary>
		/// 账号ID
		/// </summary>
		[ProtoMember(3)]
		[System.ComponentModel.Description("账号ID")]
		public long Id { get; set; }

		/// <summary>
		/// 账号等级
		/// </summary>
		[ProtoMember(4)]
		[System.ComponentModel.Description("账号等级")]
		public uint Level { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		[ProtoMember(5)]
		[System.ComponentModel.Description("创建时间")]
		public long CreateTime { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			Code = default;
			RoleName = default;
			Id = default;
			Level = default;
			CreateTime = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 请求角色创建
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("请求角色创建")]
	[MessageTypeHandler(((300) << 16) + 12)]
	public sealed class ReqPlayerCreate : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 账号ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("账号ID")]
		public long Id { get; set; }

		/// <summary>
		/// 角色名
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("角色名")]
		public string Name { get; set; }

		public override void Clear()
		{
			Id = default;
			Name = default;
		}
	}

	/// <summary>
	/// 返回角色创建
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("返回角色创建")]
	[MessageTypeHandler(((300) << 16) + 13)]
	public sealed class RespPlayerCreate : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 角色信息
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("角色信息")]
		public PlayerInfo PlayerInfo { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			PlayerInfo = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 请求角色列表
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("请求角色列表")]
	[MessageTypeHandler(((300) << 16) + 14)]
	public sealed class ReqPlayerList : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 账号ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("账号ID")]
		public long Id { get; set; }

		public override void Clear()
		{
			Id = default;
		}
	}

	/// <summary>
	/// 返回角色列表
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("返回角色列表")]
	[MessageTypeHandler(((300) << 16) + 15)]
	public sealed class RespPlayerList : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 角色列表
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("角色列表")]
		public List<PlayerInfo> PlayerList { get; set; } = new List<PlayerInfo>();

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			PlayerList.Clear();
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 角色信息
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("角色信息")]
	public sealed class PlayerInfo
	{
		/// <summary>
		/// 角色ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("角色ID")]
		public long Id { get; set; }

		/// <summary>
		/// 角色名
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("角色名")]
		public string Name { get; set; }

		/// <summary>
		/// 角色等级
		/// </summary>
		[ProtoMember(3)]
		[System.ComponentModel.Description("角色等级")]
		public uint Level { get; set; }

		/// <summary>
		/// 角色状态
		/// </summary>
		[ProtoMember(4)]
		[System.ComponentModel.Description("角色状态")]
		public int State { get; set; }

		/// <summary>
		/// 角色头像
		/// </summary>
		[ProtoMember(5)]
		[System.ComponentModel.Description("角色头像")]
		public uint Avatar { get; set; }

		/// <summary>
		/// 角色当前经验
		/// </summary>
		[ProtoMember(6)]
		[System.ComponentModel.Description("角色当前经验")]
		public ulong CurrentExp { get; set; }
	}

	/// <summary>
	/// 请求玩家登录
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("请求玩家登录")]
	[MessageTypeHandler(((300) << 16) + 16)]
	public sealed class ReqPlayerLogin : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 角色ID
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("角色ID")]
		public long Id { get; set; }

		public override void Clear()
		{
			Id = default;
		}
	}

	/// <summary>
	/// 返回玩家登录
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("返回玩家登录")]
	[MessageTypeHandler(((300) << 16) + 17)]
	public sealed class RespPlayerLogin : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 返回码（ResultCode）
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("返回码（ResultCode）")]
		public int Code { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("创建时间")]
		public long CreateTime { get; set; }

		/// <summary>
		/// 角色信息
		/// </summary>
		[ProtoMember(3)]
		[System.ComponentModel.Description("角色信息")]
		public PlayerInfo PlayerInfo { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			Code = default;
			CreateTime = default;
			PlayerInfo = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 客户端每次请求都会回复错误码
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("客户端每次请求都会回复错误码")]
	[MessageTypeHandler(((300) << 16) + 18)]
	public sealed class RespErrorCode : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 0:表示无错误
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("0:表示无错误")]
		public long ErrCode { get; set; }

		/// <summary>
		/// 错误描述（不为0时有效）
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("错误描述（不为0时有效）")]
		public string Desc { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			ErrCode = default;
			Desc = default;
			ErrorCode = default;
		}
	}

	/// <summary>
	/// 提示信息
	/// </summary>
	[ProtoContract]
	[System.ComponentModel.Description("提示信息")]
	[MessageTypeHandler(((300) << 16) + 19)]
	public sealed class RespPrompt : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 提示信息类型（1Tip提示，2跑马灯，3插队跑马灯，4弹窗，5弹窗回到登陆，6弹窗退出游戏）
		/// </summary>
		[ProtoMember(1)]
		[System.ComponentModel.Description("提示信息类型（1Tip提示，2跑马灯，3插队跑马灯，4弹窗，5弹窗回到登陆，6弹窗退出游戏）")]
		public int Type { get; set; }

		/// <summary>
		/// 提示内容
		/// </summary>
		[ProtoMember(2)]
		[System.ComponentModel.Description("提示内容")]
		public string Content { get; set; }

		/// <summary>
		/// 返回的错误码
		/// </summary>
		[ProtoMember(2047)]
		[System.ComponentModel.Description("返回的错误码")]
		public int ErrorCode { get; set; }

		public override void Clear()
		{
			Type = default;
			Content = default;
			ErrorCode = default;
		}
	}

}
