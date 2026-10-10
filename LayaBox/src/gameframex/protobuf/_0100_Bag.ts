import IRequestMessage from "../network/IRequestMessage";
import IResponseMessage from "../network/IResponseMessage";
import INotifyMessage from "../network/INotifyMessage";
import IHeartBeatMessage from "../network/IHeartBeatMessage";
import MessageObject from "../network/MessageObject";
import ProtoMessageHelper from "../network/ProtoMessageHelper";

export namespace Bag {
	export class BagItem {
		ItemId:number;

		Count:number;

	}

	export class ReqBagInfo extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Bag.ReqBagInfo', 6553610);
		}

		public readonly PackageName: string = 'Bag.ReqBagInfo';

	}

	export class RespBagInfo extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Bag.RespBagInfo', 6553611);
		}

		public readonly PackageName: string = 'Bag.RespBagInfo';

		ItemDic:Map<number, number>;

		ErrorCode:number;

	}

	export class NotifyBagItem extends MessageObject implements INotifyMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Bag.NotifyBagItem', 6553612);
		}

		public readonly PackageName: string = 'Bag.NotifyBagItem';

		ItemId:number;

		Count:number;

		Value:number;

	}

	export class NotifyBagInfoChanged extends MessageObject implements INotifyMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Bag.NotifyBagInfoChanged', 6553613);
		}

		public readonly PackageName: string = 'Bag.NotifyBagInfoChanged';

		ItemDic:Map<number, NotifyBagItem>;

	}

	export class ReqComposePet extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Bag.ReqComposePet', 6553614);
		}

		public readonly PackageName: string = 'Bag.ReqComposePet';

		FragmentId:number;

	}

	export class RespComposePet extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Bag.RespComposePet', 6553615);
		}

		public readonly PackageName: string = 'Bag.RespComposePet';

		PetId:number;

		ErrorCode:number;

	}

	export class ReqUseItem extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Bag.ReqUseItem', 6553616);
		}

		public readonly PackageName: string = 'Bag.ReqUseItem';

		ItemId:number;

		Count:number;

	}

	export class RespUseItem extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Bag.RespUseItem', 6553617);
		}

		public readonly PackageName: string = 'Bag.RespUseItem';

		ItemId:number;

		Count:number;

		ErrorCode:number;

	}

	export class ReqDiscardItem extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Bag.ReqDiscardItem', 6553618);
		}

		public readonly PackageName: string = 'Bag.ReqDiscardItem';

		ItemId:number;

		Count:number;

	}

	export class RespDiscardItem extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Bag.RespDiscardItem', 6553619);
		}

		public readonly PackageName: string = 'Bag.RespDiscardItem';

		ItemId:number;

		Count:number;

		ErrorCode:number;

	}

	export class ReqSellItem extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Bag.ReqSellItem', 6553620);
		}

		public readonly PackageName: string = 'Bag.ReqSellItem';

		ItemId:number;

	}

	export class RespSellItem extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Bag.RespSellItem', 6553621);
		}

		public readonly PackageName: string = 'Bag.RespSellItem';

		ItemDic:Map<number, number>;

		ErrorCode:number;

	}

	export class ReqAddItem extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Bag.ReqAddItem', 6553622);
		}

		public readonly PackageName: string = 'Bag.ReqAddItem';

		ItemDic:Map<number, number>;

	}

	export class RespAddItem extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Bag.RespAddItem', 6553623);
		}

		public readonly PackageName: string = 'Bag.RespAddItem';

		ItemDic:Map<number, number>;

		ErrorCode:number;

	}

	export class ReqRemoveItem extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Bag.ReqRemoveItem', 6553624);
		}

		public readonly PackageName: string = 'Bag.ReqRemoveItem';

		ItemDic:Map<number, number>;

	}

	export class RespRemoveItem extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Bag.RespRemoveItem', 6553625);
		}

		public readonly PackageName: string = 'Bag.RespRemoveItem';

		ItemDic:Map<number, number>;

		ErrorCode:number;

	}

}
