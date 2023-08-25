using System;
using MessagePack;
using System.Collections.Generic;
using Server.Core.Net.Messages;

namespace Server.Proto.Proto
{
	/// <summary>
	/// 请求心跳
	/// </summary>
	[MessageTypeHandler(1)]
	[MessagePackObject(true)]
	public partial class ReqHeartBeat : Server.Core.Net.Messages.Message, IRequestMessage
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
	[MessagePackObject(true)]
	public partial class RespHeartBeat : Server.Core.Net.Messages.Message, IResponseMessage
	{
		/// <summary>
		///  时间戳
		/// </summary>
		public long Timestamp { get; set; }

	}

}
