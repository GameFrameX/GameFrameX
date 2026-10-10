import IRequestMessage from "../network/IRequestMessage";
import IResponseMessage from "../network/IResponseMessage";
import INotifyMessage from "../network/INotifyMessage";
import IHeartBeatMessage from "../network/IHeartBeatMessage";
import MessageObject from "../network/MessageObject";
import ProtoMessageHelper from "../network/ProtoMessageHelper";

export namespace Basic {
	export class ReqHeartBeat extends MessageObject implements IRequestMessage, IHeartBeatMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Basic.ReqHeartBeat', 655370);
		}

		public readonly PackageName: string = 'Basic.ReqHeartBeat';

		Timestamp:number;

	}

	export class NotifyHeartBeat extends MessageObject implements INotifyMessage, IHeartBeatMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Basic.NotifyHeartBeat', 655371);
		}

		public readonly PackageName: string = 'Basic.NotifyHeartBeat';

		Timestamp:number;

	}

	export class NotifyServerFullyLoaded extends MessageObject implements INotifyMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Basic.NotifyServerFullyLoaded', 655372);
		}

		public readonly PackageName: string = 'Basic.NotifyServerFullyLoaded';

	}

}
