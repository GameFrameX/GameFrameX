using ProtoBuf;
using System.Collections.Generic;

namespace Hotfix.Message.Proto
{
	/// <summary>
	/// 请求背包数据
	/// </summary>
	[ProtoContract]
	public partial class ReqBagInfo
	{
	}

	/// <summary>
	/// 返回背包数据
	/// </summary>
	[ProtoContract]
	public partial class ResBagInfo
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
	public partial class ReqComposePet
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
	public partial class ResComposePet
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
	public partial class ReqUseItem
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
	public partial class ReqSellItem
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
	public partial class ResItemChange
	{
		/// <summary>
		///  变化的道具
		/// </summary>
		[ProtoMember(1)]
		public Dictionary<int, long> ItemDic { get; set; }

	}

}
