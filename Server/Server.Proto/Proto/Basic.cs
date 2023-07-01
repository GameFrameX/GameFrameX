using ProtoBuf;
using System.Collections.Generic;
using Server.Core.Net.Messages;

namespace Server.Proto
{
	/// <summary>
	/// 请求心跳
	/// </summary>
	[MessageTypeHandler(1)]
	[ProtoContract]
	public partial class ReqHeartBeat : Server.Core.Net.Messages.Message, IRequestMessage
	{
		/// <summary>
		///  时间戳
		/// </summary>
		[ProtoMember(1)]
		public long Timestamp { get; set; }

	}

	/// <summary>
	/// 返回心跳
	/// </summary>
	[MessageTypeHandler(1)]
	[ProtoContract]
	public partial class RespHeartBeat : Server.Core.Net.Messages.Message, IResponseMessage
	{
		/// <summary>
		///  时间戳
		/// </summary>
		[ProtoMember(1)]
		public long Timestamp { get; set; }

	}

}
