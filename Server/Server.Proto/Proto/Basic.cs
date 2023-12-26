using System;
using System.Collections.Generic;
using Server.NetWork.Messages;

namespace Server.Proto.Proto
{
	/// <summary>
	/// 请求心跳
	/// </summary>
	[MessageTypeHandler(1)]
	[MessagePackageObject]
	public partial class ReqHeartBeat : MessageObject, IRequestMessage
	{
		/// <summary>
		///  时间戳
		/// </summary>
		public long Timestamp { get; set; }

	}

	/// <summary>
	/// 返回心跳
	/// </summary>
	[MessageTypeHandler(1)]
	[MessagePackageObject]
	public partial class RespHeartBeat : MessageObject, IResponseMessage
	{
		/// <summary>
		///  时间戳
		/// </summary>
		public long Timestamp { get; set; }

	}

}
