using System;
using MessagePack;
using System.Collections.Generic;
using Server.Core.Net.Messages;

namespace Server.Proto.Proto
{
	/// <summary>
	/// 请求登录
	/// </summary>
	[MessageTypeHandler(10)]
	[MessagePackObject(true)]
	public partial class ReqLogin : Server.Core.Net.Messages.Message, IRequestMessage
	{
		/// <summary>
		/// 
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Platform { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int SdkType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string SdkToken { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Device { get; set; }

		/// <summary>
		///  密码
		/// </summary>
		public string Password { get; set; }

	}

	/// <summary>
	/// 请求登录返回
	/// </summary>
	[MessageTypeHandler(10)]
	[MessagePackObject(true)]
	public partial class RespLogin : Server.Core.Net.Messages.Message, IResponseMessage
	{
		/// <summary>
		/// 
		/// </summary>
		public int Code { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public UserInfo UserInfo { get; set; }

	}

	/// <summary>
	/// 客户端每次请求都会回复错误码
	/// </summary>
	
	[MessagePackObject(true)]
	public partial class RespErrorCode : Server.Core.Net.Messages.Message
	{
		/// <summary>
		///  0:表示无错误
		/// </summary>
		public long ErrCode { get; set; }

		/// <summary>
		///  错误描述（不为0时有效）
		/// </summary>
		public string Desc { get; set; }

	}

	[MessageTypeHandler(200)]
	[MessagePackObject(true)]
	public partial class RespPrompt : Server.Core.Net.Messages.Message, IResponseMessage
	{
		/// <summary>
		///  提示信息类型（1Tip提示，2跑马灯，3插队跑马灯，4弹窗，5弹窗回到登陆，6弹窗退出游戏）
		/// </summary>
		public int Type { get; set; }

		/// <summary>
		///  提示内容
		/// </summary>
		public string Content { get; set; }

	}

}
