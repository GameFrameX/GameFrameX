import IRequestMessage from "../network/IRequestMessage";
import IResponseMessage from "../network/IResponseMessage";
import INotifyMessage from "../network/INotifyMessage";
import IHeartBeatMessage from "../network/IHeartBeatMessage";
import MessageObject from "../network/MessageObject";
import ProtoMessageHelper from "../network/ProtoMessageHelper";

export namespace Common {
	export enum ResultCode
	{
		Success = 0, 
		Failed = 1, 
	}

	export enum OperationStatusCode
	{
		Ok = 0, 
		ConfigErr = 1, 
		ParamErr = 2, 
		CostNotEnough = 3, 
		Forbidden = 4, 
		NotFound = 5, 
		HasExist = 6, 
		AccountCannotBeNull = 7, 
		Unprocessable = 8, 
		UnknownPlatform = 9, 
		Notice = 10, 
		FuncNotOpen = 11, 
		Other = 12, 
		InternalServerError = 13, 
		ServerFullyLoaded = 14, 
		Unsupported = 15, 
		InvalidReward = 16, 
		PartialSuccess = 17, 
	}

}
