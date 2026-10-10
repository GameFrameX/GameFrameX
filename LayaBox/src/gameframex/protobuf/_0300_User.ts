import IRequestMessage from "../network/IRequestMessage";
import IResponseMessage from "../network/IResponseMessage";
import INotifyMessage from "../network/INotifyMessage";
import IHeartBeatMessage from "../network/IHeartBeatMessage";
import MessageObject from "../network/MessageObject";
import ProtoMessageHelper from "../network/ProtoMessageHelper";

export namespace User {
	export class ReqLogin extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('User.ReqLogin', 19660810);
		}

		public readonly PackageName: string = 'User.ReqLogin';

		UserName:string;

		Platform:string;

		SdkType:number;

		SdkToken:string;

		Device:string;

		Password:string;

	}

	export class RespLogin extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('User.RespLogin', 19660811);
		}

		public readonly PackageName: string = 'User.RespLogin';

		Code:number;

		RoleName:string;

		Id:number;

		Level:number;

		CreateTime:number;

		ErrorCode:number;

	}

	export class ReqPlayerCreate extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('User.ReqPlayerCreate', 19660812);
		}

		public readonly PackageName: string = 'User.ReqPlayerCreate';

		Id:number;

		Name:string;

	}

	export class RespPlayerCreate extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('User.RespPlayerCreate', 19660813);
		}

		public readonly PackageName: string = 'User.RespPlayerCreate';

		PlayerInfo:PlayerInfo;

		ErrorCode:number;

	}

	export class ReqPlayerList extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('User.ReqPlayerList', 19660814);
		}

		public readonly PackageName: string = 'User.ReqPlayerList';

		Id:number;

	}

	export class RespPlayerList extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('User.RespPlayerList', 19660815);
		}

		public readonly PackageName: string = 'User.RespPlayerList';

		PlayerList:PlayerInfo;

		ErrorCode:number;

	}

	export class PlayerInfo {
		Id:number;

		Name:string;

		Level:number;

		State:number;

		Avatar:number;

		CurrentExp:number;

	}

	export class ReqPlayerLogin extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('User.ReqPlayerLogin', 19660816);
		}

		public readonly PackageName: string = 'User.ReqPlayerLogin';

		Id:number;

	}

	export class RespPlayerLogin extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('User.RespPlayerLogin', 19660817);
		}

		public readonly PackageName: string = 'User.RespPlayerLogin';

		Code:number;

		CreateTime:number;

		PlayerInfo:PlayerInfo;

		ErrorCode:number;

	}

	export class RespErrorCode extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('User.RespErrorCode', 19660818);
		}

		public readonly PackageName: string = 'User.RespErrorCode';

		ErrCode:number;

		Desc:string;

		ErrorCode:number;

	}

	export class RespPrompt extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('User.RespPrompt', 19660819);
		}

		public readonly PackageName: string = 'User.RespPrompt';

		Type:number;

		Content:string;

		ErrorCode:number;

	}

}
