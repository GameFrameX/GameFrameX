import IRequestMessage from "../network/IRequestMessage";
import IResponseMessage from "../network/IResponseMessage";
import INotifyMessage from "../network/INotifyMessage";
import IHeartBeatMessage from "../network/IHeartBeatMessage";
import MessageObject from "../network/MessageObject";
import ProtoMessageHelper from "../network/ProtoMessageHelper";

export namespace Common {
	/// <summary>
	/// 返回码
	/// </summary>
	export enum ResultCode
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
	/// 
	/// </summary>
	export enum PhoneType
	{
		/// <summary>
		/// 手机
		/// </summary>
		MOBILE = 0, 
		/// <summary>
		/// 
		/// </summary>
		HOME = 1, 
		/// <summary>
		/// 工作号码
		/// </summary>
		WORK = 2, 
	}

	/// <summary>
	/// 
	/// </summary>
	export class PhoneNumber {
		/// <summary>
		/// 
		/// </summary>
		number:string;

		/// <summary>
		/// 
		/// </summary>
		type:PhoneType;

	}

	/// <summary>
	/// 
	/// </summary>
	export class Person {
		/// <summary>
		/// 
		/// </summary>
		name:string;

		/// <summary>
		/// Unique ID number for this person.
		/// </summary>
		id:number;

		/// <summary>
		/// 
		/// </summary>
		email:string;

		/// <summary>
		/// 
		/// </summary>
		phones:PhoneNumber;

	}

	/// <summary>
	/// Our address book file is just one of these.
	/// </summary>
	export class AddressBook {
		/// <summary>
		/// 
		/// </summary>
		people:Person;

	}

}
