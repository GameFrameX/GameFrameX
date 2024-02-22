using System;
using ProtoBuf;
using System.Collections.Generic;
using Server.NetWork.Messages;

namespace Server.Proto.Proto
{
	/// <summary>
	/// 请求登录
	/// </summary>
	[MessageTypeHandler(10)]
	[ProtoContract]
	public partial class ReqLogin : MessageObject, IRequestMessage
	{
		/// <summary>
		/// 
		/// </summary>
		[ProtoMember(1)]
		public string UserName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[ProtoMember(2)]
		public string Platform { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[ProtoMember(3)]
		public int SdkType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[ProtoMember(4)]
		public string SdkToken { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[ProtoMember(5)]
		public string Device { get; set; }

		/// <summary>
		///  密码
		/// </summary>
		[ProtoMember(6)]
		public string Password { get; set; }

	}

	/// <summary>
	/// 请求登录返回
	/// </summary>
	[MessageTypeHandler(10)]
	[ProtoContract]
	public partial class RespLogin : MessageObject, IResponseMessage
	{
		/// <summary>
		/// 
		/// </summary>
		[ProtoMember(1)]
		public int Code { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[ProtoMember(2)]
		public UserInfo UserInfo { get; set; }

	}

	/// <summary>
	/// 客户端每次请求都会回复错误码
	/// </summary>
	
	[ProtoContract]
	public partial class RespErrorCode : MessageObject
	{
		/// <summary>
		///  0:表示无错误
		/// </summary>
		[ProtoMember(1)]
		public long ErrCode { get; set; }

		/// <summary>
		///  错误描述（不为0时有效）
		/// </summary>
		[ProtoMember(2)]
		public string Desc { get; set; }

	}

	[MessageTypeHandler(200)]
	[ProtoContract]
	public partial class RespPrompt : MessageObject, IResponseMessage
	{
		/// <summary>
		///  提示信息类型（1Tip提示，2跑马灯，3插队跑马灯，4弹窗，5弹窗回到登陆，6弹窗退出游戏）
		/// </summary>
		[ProtoMember(1)]
		public int Type { get; set; }

		/// <summary>
		///  提示内容
		/// </summary>
		[ProtoMember(2)]
		public string Content { get; set; }

	}

}
