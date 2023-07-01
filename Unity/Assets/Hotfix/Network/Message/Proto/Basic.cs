using ProtoBuf;
using System.Collections.Generic;
using Protocol;

namespace Hotfix.Message.Proto
{
	/// <summary>
	/// 请求心跳
	/// </summary>
	[MessageTypeHandler(1)]
	[ProtoContract]
	public partial class ReqHeartBeat : Protocol.Message, IRequestMessage
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
	public partial class RespHeartBeat : Protocol.Message, IResponseMessage
	{
		/// <summary>
		///  时间戳
		/// </summary>
		[ProtoMember(1)]
		public long Timestamp { get; set; }

	}

}
