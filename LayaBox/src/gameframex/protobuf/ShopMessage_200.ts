import IRequestMessage from "../network/IRequestMessage";
import IResponseMessage from "../network/IResponseMessage";
import INotifyMessage from "../network/INotifyMessage";
import IHeartBeatMessage from "../network/IHeartBeatMessage";
import MessageObject from "../network/MessageObject";
import ProtoMessageHelper from "../network/ProtoMessageHelper";

export namespace ShopMessage {
	/// <summary>
	/// 获取商品列表
	/// </summary>
	export class C2S_GetShopItemList extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('ShopMessage.C2S_GetShopItemList', 13107210);
		}

		public readonly PackageName: string = 'ShopMessage.C2S_GetShopItemList';

		/// <summary>
		/// 
		/// </summary>
		RpcId:number;

		/// <summary>
		/// 商店类型
		/// </summary>
		shopType:number;

	}

	/// <summary>
	/// 
	/// </summary>
	export class S2C_GetShopItemList extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('ShopMessage.S2C_GetShopItemList', 13107211);
		}

		public readonly PackageName: string = 'ShopMessage.S2C_GetShopItemList';

		/// <summary>
		/// 
		/// </summary>
		RpcId:number;

		/// <summary>
		/// 
		/// </summary>
		Error:number;

		/// <summary>
		/// 
		/// </summary>
		Message:string;

		/// <summary>
		/// 商店类型
		/// </summary>
		shopType:number;

		/// <summary>
		/// 分页号（分页号如果为-1，则将该商店中的所有物品都推送给前端）
		/// </summary>
		pageId:number;

		/// <summary>
		/// 免费总刷新次数
		/// </summary>
		maxFreeRfsTimes:number;

		/// <summary>
		/// 货币总刷新次数
		/// </summary>
		maxCurrencyRfsTimes:number;

		/// <summary>
		/// 广告总刷新次数
		/// </summary>
		maxAdsRfsTimes:number;

		/// <summary>
		/// 下次刷新时间
		/// </summary>
		nextRefreshTime:number;

		/// <summary>
		/// 返回的错误码
		/// </summary>
		ErrorCode:number;

	}

	/// <summary>
	/// 获取限购列表
	/// </summary>
	export class C2S_GetLimitList extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('ShopMessage.C2S_GetLimitList', 13107212);
		}

		public readonly PackageName: string = 'ShopMessage.C2S_GetLimitList';

		/// <summary>
		/// 
		/// </summary>
		RpcId:number;

		/// <summary>
		/// 商店类型
		/// </summary>
		shopType:number;

		/// <summary>
		/// 玩家id
		/// </summary>
		playerId:number;

		/// <summary>
		/// 分页号（分页号如果为-1，则将该商店中的所有限购物品都推送给前端）
		/// </summary>
		pageId:number;

	}

	/// <summary>
	/// 存储数据
	/// </summary>
	export class S2C_GetLimitList extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('ShopMessage.S2C_GetLimitList', 13107213);
		}

		public readonly PackageName: string = 'ShopMessage.S2C_GetLimitList';

		/// <summary>
		/// 
		/// </summary>
		RpcId:number;

		/// <summary>
		/// 
		/// </summary>
		Error:number;

		/// <summary>
		/// 
		/// </summary>
		Message:string;

		/// <summary>
		/// 商店类型
		/// </summary>
		shopType:number;

		/// <summary>
		/// 玩家id
		/// </summary>
		playerId:number;

		/// <summary>
		/// 免费刷新次数
		/// </summary>
		freeRfsTimes:number;

		/// <summary>
		/// 货币刷新次数
		/// </summary>
		currencyRfsTimes:number;

		/// <summary>
		/// 广告刷新次数
		/// </summary>
		adsRfsTimes:number;

		/// <summary>
		/// 刷新时间标识 判断当前周期内是否刷新过 now - rfstime > 24
		/// </summary>
		refreshedTime:number;

		/// <summary>
		/// 返回的错误码
		/// </summary>
		ErrorCode:number;

	}

	/// <summary>
	/// 获取商品列表
	/// </summary>
	export class C2S_GetShopPaymentList extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('ShopMessage.C2S_GetShopPaymentList', 13107214);
		}

		public readonly PackageName: string = 'ShopMessage.C2S_GetShopPaymentList';

		/// <summary>
		/// 
		/// </summary>
		RpcId:number;

	}

	/// <summary>
	/// 
	/// </summary>
	export class S2C_GetShopPaymentList extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('ShopMessage.S2C_GetShopPaymentList', 13107215);
		}

		public readonly PackageName: string = 'ShopMessage.S2C_GetShopPaymentList';

		/// <summary>
		/// 
		/// </summary>
		RpcId:number;

		/// <summary>
		/// 
		/// </summary>
		Error:number;

		/// <summary>
		/// 
		/// </summary>
		Message:string;

		/// <summary>
		/// 货物类型
		/// </summary>
		goods:PaymentGood;

		/// <summary>
		/// 下次刷新时间
		/// </summary>
		nextRefreshTime:number;

		/// <summary>
		/// 返回的错误码
		/// </summary>
		ErrorCode:number;

	}

	/// <summary>
	/// 获取限购列表
	/// </summary>
	export class C2S_GetPaymentList extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('ShopMessage.C2S_GetPaymentList', 13107216);
		}

		public readonly PackageName: string = 'ShopMessage.C2S_GetPaymentList';

		/// <summary>
		/// 
		/// </summary>
		RpcId:number;

		/// <summary>
		/// 玩家id
		/// </summary>
		playerId:number;

		/// <summary>
		/// 分页号（分页号如果为-1，则将该商店中的所有限购物品都推送给前端）
		/// </summary>
		pageId:number;

	}

	/// <summary>
	/// 存储数据
	/// </summary>
	export class S2C_GetPaymentList extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('ShopMessage.S2C_GetPaymentList', 13107217);
		}

		public readonly PackageName: string = 'ShopMessage.S2C_GetPaymentList';

		/// <summary>
		/// 
		/// </summary>
		RpcId:number;

		/// <summary>
		/// 
		/// </summary>
		Error:number;

		/// <summary>
		/// 
		/// </summary>
		Message:string;

		/// <summary>
		/// 玩家id
		/// </summary>
		playerId:number;

		/// <summary>
		/// 购买存储数据
		/// </summary>
		dataList:PaymentData;

		/// <summary>
		/// 刷新时间标识 判断当前周期内是否刷新过 now - rfstime > 24
		/// </summary>
		refreshedTime:number;

		/// <summary>
		/// 返回的错误码
		/// </summary>
		ErrorCode:number;

	}

	/// <summary>
	/// ===============================数据结构==============================
	/// </summary>
	export class PaymentGood {
		/// <summary>
		/// 货物id
		/// </summary>
		goodIndex:number;

		/// <summary>
		/// 广告获取最大广告次数
		/// </summary>
		maxAdsCount:number;

		/// <summary>
		/// 广告递增数值参数
		/// </summary>
		adsAddNumber:number;

		/// <summary>
		/// 在线时长获取时间(分钟)
		/// </summary>
		onlineTime:number;

		/// <summary>
		/// 时间段登陆
		/// </summary>
		timelogin:string;

		/// <summary>
		/// 是否下架
		/// </summary>
		banned:boolean;

		/// <summary>
		/// 排序id
		/// </summary>
		sortId:number;

		/// <summary>
		/// 显示品阶
		/// </summary>
		quality:number;

		/// <summary>
		/// 名称
		/// </summary>
		name:string;

	}

	/// <summary>
	/// 
	/// </summary>
	export class PaymentData {
		/// <summary>
		/// 货物id
		/// </summary>
		goodIndex:number;

		/// <summary>
		/// 已经获取的次数
		/// </summary>
		getNum:number;

	}

}
