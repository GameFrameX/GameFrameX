import IRequestMessage from "../network/IRequestMessage";
import IResponseMessage from "../network/IResponseMessage";
import INotifyMessage from "../network/INotifyMessage";
import IHeartBeatMessage from "../network/IHeartBeatMessage";
import MessageObject from "../network/MessageObject";
import ProtoMessageHelper from "../network/ProtoMessageHelper";

export namespace Bag {
	/// <summary>
	/// 
	/// </summary>
	export enum BagType
	{
		/// <summary>
		/// 默认
		/// </summary>
		Default = 0, 
		/// <summary>
		/// 宠物
		/// </summary>
		Pet = 1, 
	}

	/// <summary>
	/// 请求背包数据
	/// </summary>
	export class ReqBagInfo extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Bag.ReqBagInfo', 6553610);
		}

		public readonly PackageName: string = 'Bag.ReqBagInfo';

		/// <summary>
		/// 背包类型
		/// </summary>
		BagType:BagType;

	}

	/// <summary>
	/// 返回背包数据
	/// </summary>
	export class RespBagInfo extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Bag.RespBagInfo', 6553611);
		}

		public readonly PackageName: string = 'Bag.RespBagInfo';

		/// <summary>
		/// 
		/// </summary>
		ItemDic:Map<number, number>;

		/// <summary>
		/// 返回的错误码
		/// </summary>
		ErrorCode:number;

	}

	/// <summary>
	/// 通知背包数据变化
	/// </summary>
	export class NotifyBagInfoChanged extends MessageObject implements INotifyMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Bag.NotifyBagInfoChanged', 6553612);
		}

		public readonly PackageName: string = 'Bag.NotifyBagInfoChanged';

		/// <summary>
		/// 变化的道具，key:道具id，value:数量
		/// </summary>
		ItemDic:Map<number, number>;

	}

	/// <summary>
	/// 请求合成宠物
	/// </summary>
	export class ReqComposePet extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Bag.ReqComposePet', 6553613);
		}

		public readonly PackageName: string = 'Bag.ReqComposePet';

		/// <summary>
		/// 碎片id
		/// </summary>
		FragmentId:number;

	}

	/// <summary>
	/// 返回合成宠物
	/// </summary>
	export class RespComposePet extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Bag.RespComposePet', 6553614);
		}

		public readonly PackageName: string = 'Bag.RespComposePet';

		/// <summary>
		/// 合成宠物的Id
		/// </summary>
		PetId:number;

		/// <summary>
		/// 返回的错误码
		/// </summary>
		ErrorCode:number;

	}

	/// <summary>
	/// 请求使用道具
	/// </summary>
	export class ReqUseItem extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Bag.ReqUseItem', 6553615);
		}

		public readonly PackageName: string = 'Bag.ReqUseItem';

		/// <summary>
		/// 道具id
		/// </summary>
		ItemId:number;

	}

	/// <summary>
	/// 出售道具
	/// </summary>
	export class ReqSellItem extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Bag.ReqSellItem', 6553616);
		}

		public readonly PackageName: string = 'Bag.ReqSellItem';

		/// <summary>
		/// 道具id
		/// </summary>
		ItemId:number;

	}

	/// <summary>
	/// 出售道具
	/// </summary>
	export class RespItemChange extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Bag.RespItemChange', 6553617);
		}

		public readonly PackageName: string = 'Bag.RespItemChange';

		/// <summary>
		/// 变化的道具
		/// </summary>
		ItemDic:Map<number, number>;

		/// <summary>
		/// 返回的错误码
		/// </summary>
		ErrorCode:number;

	}

}
