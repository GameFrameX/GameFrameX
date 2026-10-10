import IRequestMessage from "../network/IRequestMessage";
import IResponseMessage from "../network/IResponseMessage";
import INotifyMessage from "../network/INotifyMessage";
import IHeartBeatMessage from "../network/IHeartBeatMessage";
import MessageObject from "../network/MessageObject";
import ProtoMessageHelper from "../network/ProtoMessageHelper";

export namespace RockPaperScissors {
	export enum RockPaperScissorsGesture
	{
		None = 0, 
		Rock = 1, 
		Scissors = 2, 
		Paper = 3, 
	}

	export class RockPaperScissorsPlayerInfo {
		RoleId:number;

		HasGesture:boolean;

		Gesture:RockPaperScissorsGesture;

	}

	export class RockPaperScissorsGameInfo {
		RoomId:number;

		Round:number;

		WinnerRoleId:number;

		Players:RockPaperScissorsPlayerInfo;

	}

	export class ReqRockPaperScissorsGameInfo extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('RockPaperScissors.ReqRockPaperScissorsGameInfo', 26869770);
		}

		public readonly PackageName: string = 'RockPaperScissors.ReqRockPaperScissorsGameInfo';

		RoomId:number;

	}

	export class RespRockPaperScissorsGameInfo extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('RockPaperScissors.RespRockPaperScissorsGameInfo', 26869771);
		}

		public readonly PackageName: string = 'RockPaperScissors.RespRockPaperScissorsGameInfo';

		GameInfo:RockPaperScissorsGameInfo;

		ErrorCode:number;

	}

	export class ReqSubmitRockPaperScissorsGesture extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('RockPaperScissors.ReqSubmitRockPaperScissorsGesture', 26869772);
		}

		public readonly PackageName: string = 'RockPaperScissors.ReqSubmitRockPaperScissorsGesture';

		RoomId:number;

		Gesture:RockPaperScissorsGesture;

	}

	export class RespSubmitRockPaperScissorsGesture extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('RockPaperScissors.RespSubmitRockPaperScissorsGesture', 26869773);
		}

		public readonly PackageName: string = 'RockPaperScissors.RespSubmitRockPaperScissorsGesture';

		GameInfo:RockPaperScissorsGameInfo;

		ErrorCode:number;

	}

	export class ReqRestartRockPaperScissorsGame extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('RockPaperScissors.ReqRestartRockPaperScissorsGame', 26869774);
		}

		public readonly PackageName: string = 'RockPaperScissors.ReqRestartRockPaperScissorsGame';

		RoomId:number;

	}

	export class RespRestartRockPaperScissorsGame extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('RockPaperScissors.RespRestartRockPaperScissorsGame', 26869775);
		}

		public readonly PackageName: string = 'RockPaperScissors.RespRestartRockPaperScissorsGame';

		GameInfo:RockPaperScissorsGameInfo;

		ErrorCode:number;

	}

	export class NotifyRockPaperScissorsGameChanged extends MessageObject implements INotifyMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('RockPaperScissors.NotifyRockPaperScissorsGameChanged', 26869776);
		}

		public readonly PackageName: string = 'RockPaperScissors.NotifyRockPaperScissorsGameChanged';

		GameInfo:RockPaperScissorsGameInfo;

	}

}
