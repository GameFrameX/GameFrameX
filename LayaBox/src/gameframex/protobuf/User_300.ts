import IRequestMessage from "../network/IRequestMessage";
import IResponseMessage from "../network/IResponseMessage";
import INotifyMessage from "../network/INotifyMessage";
import IHeartBeatMessage from "../network/IHeartBeatMessage";
import MessageObject from "../network/MessageObject";
import ProtoMessageHelper from "../network/ProtoMessageHelper";

export namespace User {
	/// <summary>
	/// 请求账号登录
	/// </summary>
	export class ReqLogin extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('User.ReqLogin', 19660810);
		}

		public readonly PackageName: string = 'User.ReqLogin';

		/// <summary>
		/// 
		/// </summary>
		UserName:string;

		/// <summary>
		/// 
		/// </summary>
		Platform:string;

		/// <summary>
		/// 
		/// </summary>
		SdkType:number;

		/// <summary>
		/// 
		/// </summary>
		SdkToken:string;

		/// <summary>
		/// 
		/// </summary>
		Device:string;

		/// <summary>
		/// 密码
		/// </summary>
		Password:string;

	}

	/// <summary>
	/// 请求账号登录返回
	/// </summary>
	export class RespLogin extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('User.RespLogin', 19660811);
		}

		public readonly PackageName: string = 'User.RespLogin';

		/// <summary>
		/// 
		/// </summary>
		Code:number;

		/// <summary>
		/// 账号名
		/// </summary>
		RoleName:string;

		/// <summary>
		/// 账号ID
		/// </summary>
		Id:number;

		/// <summary>
		/// 账号等级
		/// </summary>
		Level:number;

		/// <summary>
		/// 创建时间
		/// </summary>
		CreateTime:number;

		/// <summary>
		/// 返回的错误码
		/// </summary>
		ErrorCode:number;

	}

	/// <summary>
	/// 请求角色创建
	/// </summary>
	export class ReqPlayerCreate extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('User.ReqPlayerCreate', 19660812);
		}

		public readonly PackageName: string = 'User.ReqPlayerCreate';

		/// <summary>
		/// 账号ID
		/// </summary>
		Id:number;

		/// <summary>
		/// 角色名
		/// </summary>
		Name:string;

	}

	/// <summary>
	/// 请求角色创建返回
	/// </summary>
	export class RespPlayerCreate extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('User.RespPlayerCreate', 19660813);
		}

		public readonly PackageName: string = 'User.RespPlayerCreate';

		/// <summary>
		/// 角色信息
		/// </summary>
		PlayerInfo:PlayerInfo;

		/// <summary>
		/// 返回的错误码
		/// </summary>
		ErrorCode:number;

	}

	/// <summary>
	/// 请求角色列表
	/// </summary>
	export class ReqPlayerList extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('User.ReqPlayerList', 19660814);
		}

		public readonly PackageName: string = 'User.ReqPlayerList';

		/// <summary>
		/// 账号ID
		/// </summary>
		Id:number;

	}

	/// <summary>
	/// 请求角色列表返回
	/// </summary>
	export class RespPlayerList extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('User.RespPlayerList', 19660815);
		}

		public readonly PackageName: string = 'User.RespPlayerList';

		/// <summary>
		/// 角色列表
		/// </summary>
		PlayerList:PlayerInfo;

		/// <summary>
		/// 返回的错误码
		/// </summary>
		ErrorCode:number;

	}

	/// <summary>
	/// 
	/// </summary>
	export class PlayerInfo {
		/// <summary>
		/// 角色ID
		/// </summary>
		Id:number;

		/// <summary>
		/// 角色名
		/// </summary>
		Name:string;

		/// <summary>
		/// 角色等级
		/// </summary>
		Level:number;

		/// <summary>
		/// 角色状态
		/// </summary>
		State:number;

		/// <summary>
		/// 角色头像
		/// </summary>
		Avatar:number;

		/// <summary>
		/// 角色当前经验
		/// </summary>
		CurrentExp:number;

	}

	/// <summary>
	/// 请求玩家登录
	/// </summary>
	export class ReqPlayerLogin extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('User.ReqPlayerLogin', 19660816);
		}

		public readonly PackageName: string = 'User.ReqPlayerLogin';

		/// <summary>
		/// 角色ID
		/// </summary>
		Id:number;

	}

	/// <summary>
	/// 请求玩家登录返回
	/// </summary>
	export class RespPlayerLogin extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('User.RespPlayerLogin', 19660817);
		}

		public readonly PackageName: string = 'User.RespPlayerLogin';

		/// <summary>
		/// 
		/// </summary>
		Code:number;

		/// <summary>
		/// 创建时间
		/// </summary>
		CreateTime:number;

		/// <summary>
		/// 角色信息
		/// </summary>
		PlayerInfo:PlayerInfo;

		/// <summary>
		/// 返回的错误码
		/// </summary>
		ErrorCode:number;

	}

	/// <summary>
	/// 客户端每次请求都会回复错误码
	/// </summary>
	export class RespErrorCode extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('User.RespErrorCode', 19660818);
		}

		public readonly PackageName: string = 'User.RespErrorCode';

		/// <summary>
		/// 0:表示无错误
		/// </summary>
		ErrCode:number;

		/// <summary>
		/// 错误描述（不为0时有效）
		/// </summary>
		Desc:string;

		/// <summary>
		/// 返回的错误码
		/// </summary>
		ErrorCode:number;

	}

	/// <summary>
	/// 
	/// </summary>
	export class RespPrompt extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('User.RespPrompt', 19660819);
		}

		public readonly PackageName: string = 'User.RespPrompt';

		/// <summary>
		/// 提示信息类型（1Tip提示，2跑马灯，3插队跑马灯，4弹窗，5弹窗回到登陆，6弹窗退出游戏）
		/// </summary>
		Type:number;

		/// <summary>
		/// 提示内容
		/// </summary>
		Content:string;

		/// <summary>
		/// 返回的错误码
		/// </summary>
		ErrorCode:number;

	}

}
