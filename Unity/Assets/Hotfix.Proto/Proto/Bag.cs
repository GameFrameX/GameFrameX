using System;
using MessagePack;
using System.Collections.Generic;
using Protocol;

namespace Hotfix.Proto.Proto
{
	/// <summary>
	/// 请求背包数据
	/// </summary>
	[MessageTypeHandler(100)]
	[MessagePackObject(true)]
	public partial class ReqBagInfo : Protocol.Message, IRequestMessage
	{
	}

	/// <summary>
	/// 返回背包数据
	/// </summary>
	[MessageTypeHandler(100)]
	[MessagePackObject(true)]
	public partial class ResBagInfo : Protocol.Message, IResponseMessage
	{
		/// <summary>
		/// 
		/// </summary>
		public Dictionary<int, long> ItemDic { get; set; }

	}

	/// <summary>
	/// 请求背包数据
	/// </summary>
	[MessageTypeHandler(101)]
	[MessagePackObject(true)]
	public partial class ReqComposePet : Protocol.Message, IRequestMessage
	{
		/// <summary>
		///  碎片id
		/// </summary>
		public int FragmentId { get; set; }

	}

	/// <summary>
	/// 返回背包数据
	/// </summary>
	[MessageTypeHandler(101)]
	[MessagePackObject(true)]
	public partial class ResComposePet : Protocol.Message, IResponseMessage
	{
		/// <summary>
		///  合成宠物的Id
		/// </summary>
		public int PetId { get; set; }

	}

	/// <summary>
	/// 请求背包数据
	/// </summary>
	[MessageTypeHandler(102)]
	[MessagePackObject(true)]
	public partial class ReqUseItem : Protocol.Message, IRequestMessage
	{
		/// <summary>
		///  道具id
		/// </summary>
		public int ItemId { get; set; }

	}

	/// <summary>
	/// 出售道具
	/// </summary>
	[MessageTypeHandler(103)]
	[MessagePackObject(true)]
	public partial class ReqSellItem : Protocol.Message, IRequestMessage
	{
		/// <summary>
		///  道具id
		/// </summary>
		public int ItemId { get; set; }

	}

	/// <summary>
	/// 出售道具
	/// </summary>
	[MessageTypeHandler(103)]
	[MessagePackObject(true)]
	public partial class ResItemChange : Protocol.Message, IResponseMessage
	{
		/// <summary>
		///  变化的道具
		/// </summary>
		public Dictionary<int, long> ItemDic { get; set; }

	}

}
