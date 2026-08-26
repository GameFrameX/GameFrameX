import IRequestMessage from "../network/IRequestMessage";
import IResponseMessage from "../network/IResponseMessage";
import INotifyMessage from "../network/INotifyMessage";
import IHeartBeatMessage from "../network/IHeartBeatMessage";
import MessageObject from "../network/MessageObject";
import ProtoMessageHelper from "../network/ProtoMessageHelper";

export namespace Basic {
	/// <summary>
	/// 请求心跳
	/// </summary>
	export class ReqHeartBeat extends MessageObject implements IRequestMessage, IHeartBeatMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Basic.ReqHeartBeat', 655370);
		}

		public readonly PackageName: string = 'Basic.ReqHeartBeat';

		/// <summary>
		/// 时间戳
		/// </summary>
		Timestamp:number;

	}

	/// <summary>
	/// 服务器通知心跳结果，因为有些业务需要对心跳结果做处理所以不做成RPC的方式处理
	/// </summary>
	export class NotifyHeartBeat extends MessageObject implements INotifyMessage, IHeartBeatMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Basic.NotifyHeartBeat', 655371);
		}

		public readonly PackageName: string = 'Basic.NotifyHeartBeat';

		/// <summary>
		/// 时间戳
		/// </summary>
		Timestamp:number;

	}

}
