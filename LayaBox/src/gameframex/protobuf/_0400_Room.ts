import IRequestMessage from "../network/IRequestMessage";
import IResponseMessage from "../network/IResponseMessage";
import INotifyMessage from "../network/INotifyMessage";
import IHeartBeatMessage from "../network/IHeartBeatMessage";
import MessageObject from "../network/MessageObject";
import ProtoMessageHelper from "../network/ProtoMessageHelper";

export namespace Room {
	export enum GameType
	{
		None = 0, 
		RockPaperScissors = 1, 
	}

	export enum RoomStatus
	{
		None = 0, 
		Waiting = 1, 
		Ready = 2, 
		Playing = 3, 
		Settling = 4, 
		Settled = 5, 
		Closed = 6, 
		Disbanded = 7, 
	}

	export enum RoomChangeType
	{
		Created = 0, 
		Joined = 1, 
		Left = 2, 
		Started = 3, 
		Settling = 4, 
		Settled = 5, 
		Closed = 6, 
		Disbanded = 7, 
		Reset = 8, 
	}

	export enum RoomPlayerOnlineStatus
	{
		OnlineUnknown = 0, 
		Online = 1, 
		Reconnecting = 2, 
		Offline = 3, 
	}

	export enum RoomPlayerStatus
	{
		PlayerStatusNone = 0, 
		Idle = 1, 
		ReadyInRoom = 2, 
		InGame = 3, 
		Submitted = 4, 
		SettlingInRoom = 5, 
		SettledInRoom = 6, 
	}

	export class RoomPlayerInfo {
		RoleId:number;

		SeatIndex:number;

		IsOwner:boolean;

		Name:string;

		Avatar:number;

		OnlineStatus:RoomPlayerOnlineStatus;

		PlayerStatus:RoomPlayerStatus;

	}

	export class RoomInfo {
		RoomId:number;

		Name:string;

		GameType:GameType;

		Status:RoomStatus;

		PlayerCount:number;

		MinPlayerCount:number;

		MaxPlayerCount:number;

		OwnerRoleId:number;

		Players:RoomPlayerInfo;

		Round:number;

		CreatedTime:number;

		UpdatedTime:number;

	}

	export class ReqRoomList extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Room.ReqRoomList', 26214410);
		}

		public readonly PackageName: string = 'Room.ReqRoomList';

		GameType:GameType;

		IncludeClosed:boolean;

	}

	export class RespRoomList extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Room.RespRoomList', 26214411);
		}

		public readonly PackageName: string = 'Room.RespRoomList';

		Rooms:RoomInfo;

		ErrorCode:number;

	}

	export class ReqCreateRoom extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Room.ReqCreateRoom', 26214412);
		}

		public readonly PackageName: string = 'Room.ReqCreateRoom';

		GameType:GameType;

		Name:string;

		MinPlayerCount:number;

		MaxPlayerCount:number;

	}

	export class RespCreateRoom extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Room.RespCreateRoom', 26214413);
		}

		public readonly PackageName: string = 'Room.RespCreateRoom';

		Room:RoomInfo;

		ErrorCode:number;

	}

	export class ReqJoinRoom extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Room.ReqJoinRoom', 26214414);
		}

		public readonly PackageName: string = 'Room.ReqJoinRoom';

		RoomId:number;

	}

	export class RespJoinRoom extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Room.RespJoinRoom', 26214415);
		}

		public readonly PackageName: string = 'Room.RespJoinRoom';

		Room:RoomInfo;

		ErrorCode:number;

	}

	export class ReqLeaveRoom extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Room.ReqLeaveRoom', 26214416);
		}

		public readonly PackageName: string = 'Room.ReqLeaveRoom';

		RoomId:number;

	}

	export class RespLeaveRoom extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Room.RespLeaveRoom', 26214417);
		}

		public readonly PackageName: string = 'Room.RespLeaveRoom';

		Room:RoomInfo;

		ErrorCode:number;

	}

	export class ReqStartRoomGame extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Room.ReqStartRoomGame', 26214418);
		}

		public readonly PackageName: string = 'Room.ReqStartRoomGame';

		RoomId:number;

	}

	export class RespStartRoomGame extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Room.RespStartRoomGame', 26214419);
		}

		public readonly PackageName: string = 'Room.RespStartRoomGame';

		Room:RoomInfo;

		ErrorCode:number;

	}

	export class NotifyRoomChanged extends MessageObject implements INotifyMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Room.NotifyRoomChanged', 26214420);
		}

		public readonly PackageName: string = 'Room.NotifyRoomChanged';

		ChangeType:RoomChangeType;

		Room:RoomInfo;

	}

}
