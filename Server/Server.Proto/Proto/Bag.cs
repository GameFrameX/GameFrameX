using ProtoBuf;
using System.Collections.Generic;
using Server.Core.Net.Messages;

namespace Server.Proto
{
	/// <summary>
	/// 请求背包数据
	/// </summary>
	[ProtoContract]
	public partial class ReqBagInfo: Message
	{
	}

	/// <summary>
	/// 返回背包数据
	/// </summary>
	[ProtoContract]
	public partial class ResBagInfo: Message
	{
		/// <summary>
		/// 
		/// </summary>
		[ProtoMember(1)]
		public Dictionary<int, long> ItemDic { get; set; }

	}

	/// <summary>
	/// 请求背包数据
	/// </summary>
	[ProtoContract]
	public partial class ReqComposePet: Message
	{
		/// <summary>
		///  碎片id
		/// </summary>
		[ProtoMember(1)]
		public int FragmentId { get; set; }

	}

	/// <summary>
	/// 返回背包数据
	/// </summary>
	[ProtoContract]
	public partial class ResComposePet: Message
	{
		/// <summary>
		///  合成宠物的Id
		/// </summary>
		[ProtoMember(1)]
		public int PetId { get; set; }

	}

	/// <summary>
	/// 请求背包数据
	/// </summary>
	[ProtoContract]
	public partial class ReqUseItem: Message
	{
		/// <summary>
		///  道具id
		/// </summary>
		[ProtoMember(1)]
		public int ItemId { get; set; }

	}

	/// <summary>
	/// 出售道具
	/// </summary>
	[ProtoContract]
	public partial class ReqSellItem: Message
	{
		/// <summary>
		///  道具id
		/// </summary>
		[ProtoMember(1)]
		public int ItemId { get; set; }

	}

	/// <summary>
	/// 出售道具
	/// </summary>
	[ProtoContract]
	public partial class ResItemChange: Message
	{
		/// <summary>
		///  变化的道具
		/// </summary>
		[ProtoMember(1)]
		public Dictionary<int, long> ItemDic { get; set; }

	}

}
