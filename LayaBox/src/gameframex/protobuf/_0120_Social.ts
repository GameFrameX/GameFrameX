import IRequestMessage from "../network/IRequestMessage";
import IResponseMessage from "../network/IResponseMessage";
import INotifyMessage from "../network/INotifyMessage";
import IHeartBeatMessage from "../network/IHeartBeatMessage";
import MessageObject from "../network/MessageObject";
import ProtoMessageHelper from "../network/ProtoMessageHelper";

export namespace Social {
	export class FriendInfo {
		PlayerId:number;

		PlayerName:string;

	}

	export class ReqSocialInfo extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Social.ReqSocialInfo', 7864330);
		}

		public readonly PackageName: string = 'Social.ReqSocialInfo';

	}

	export class RespSocialInfo extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Social.RespSocialInfo', 7864331);
		}

		public readonly PackageName: string = 'Social.RespSocialInfo';

		ErrorCode:number;

	}

	export class ReqDeleteFriend extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Social.ReqDeleteFriend', 7864332);
		}

		public readonly PackageName: string = 'Social.ReqDeleteFriend';

		PlayerId:number;

	}

	export class RespDeleteFriend extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Social.RespDeleteFriend', 7864333);
		}

		public readonly PackageName: string = 'Social.RespDeleteFriend';

		Success:boolean;

		ErrorCode:number;

	}

	export class ReqFriendList extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Social.ReqFriendList', 7864334);
		}

		public readonly PackageName: string = 'Social.ReqFriendList';

	}

	export class RespFriendList extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Social.RespFriendList', 7864335);
		}

		public readonly PackageName: string = 'Social.RespFriendList';

		Friends:FriendInfo;

		ErrorCode:number;

	}

	export class ReqFriendByAdd extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Social.ReqFriendByAdd', 7864336);
		}

		public readonly PackageName: string = 'Social.ReqFriendByAdd';

		PlayerId:number;

	}

	export class RespFriendByAdd extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Social.RespFriendByAdd', 7864337);
		}

		public readonly PackageName: string = 'Social.RespFriendByAdd';

		Success:boolean;

		ErrorCode:number;

	}

}
