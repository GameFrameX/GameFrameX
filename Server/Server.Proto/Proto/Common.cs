using System;
using System.Collections.Generic;
using Server.NetWork.Messages;

namespace Server.Proto.Proto
{
	/// <summary>
	/// 返回码
	/// </summary>
	public enum ResultCode
	{
		/// <summary>
		/// 成功
		/// </summary>
		Success = 0, 

		/// <summary>
		/// 失败
		/// </summary>
		Failed = 1, 

	}

	/// <summary>
	/// 玩家基础信息
	/// </summary>
	
	[MessagePackageObject]
	public partial class UserInfo : MessageObject
	{
		/// <summary>
		///  角色名
		/// </summary>
		public string RoleName { get; set; }

		/// <summary>
		///  角色ID
		/// </summary>
		public long RoleId { get; set; }

		/// <summary>
		///  角色等级
		/// </summary>
		public int Level { get; set; }

		/// <summary>
		///  创建时间
		/// </summary>
		public DateTime CreateTime { get; set; }

		/// <summary>
		///  vip等级
		/// </summary>
		public int VipLevel { get; set; }

	}

	public enum PhoneType
	{
		/// <summary>
		///  手机
		/// </summary>
		MOBILE = 0, 

		/// <summary>
		/// 
		/// </summary>
		HOME = 1, 

		/// <summary>
		///  工作号码
		/// </summary>
		WORK = 2, 

	}

	
	[MessagePackageObject]
	public partial class PhoneNumber : MessageObject
	{
		/// <summary>
		/// 
		/// </summary>
		public string number { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public PhoneType type { get; set; }

	}

	
	[MessagePackageObject]
	public partial class Person : MessageObject
	{
		/// <summary>
		/// 
		/// </summary>
		public string name { get; set; }

		/// <summary>
		///  Unique ID number for this person.
		/// </summary>
		public int id { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string email { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public List<PhoneNumber> phones = new List<PhoneNumber>();

	}

	/// <summary>
	/// Ouraddressbookfileisjustoneofthese.
	/// </summary>
	
	[MessagePackageObject]
	public partial class AddressBook : MessageObject
	{
		/// <summary>
		/// 
		/// </summary>
		public List<Person> people = new List<Person>();

	}

}
