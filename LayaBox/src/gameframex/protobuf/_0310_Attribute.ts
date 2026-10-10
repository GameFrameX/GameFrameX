import IRequestMessage from "../network/IRequestMessage";
import IResponseMessage from "../network/IResponseMessage";
import INotifyMessage from "../network/INotifyMessage";
import IHeartBeatMessage from "../network/IHeartBeatMessage";
import MessageObject from "../network/MessageObject";
import ProtoMessageHelper from "../network/ProtoMessageHelper";

export namespace Attribute {
	export class PlayerAttributeEntry {
		Type:number;

		Value:number;

		Base:number;

		Add:number;

		Pct:number;

	}

	export class NotifyPlayerAttributeSync extends MessageObject implements INotifyMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Attribute.NotifyPlayerAttributeSync', 20316170);
		}

		public readonly PackageName: string = 'Attribute.NotifyPlayerAttributeSync';

		Attributes:PlayerAttributeEntry;

	}

	export class NotifyPlayerAttributeChanged extends MessageObject implements INotifyMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Attribute.NotifyPlayerAttributeChanged', 20316171);
		}

		public readonly PackageName: string = 'Attribute.NotifyPlayerAttributeChanged';

		Type:number;

		Value:number;

	}

}
