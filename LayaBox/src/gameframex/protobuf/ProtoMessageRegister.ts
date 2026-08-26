import { Bag } from "./Bag_100";
import { Basic } from "./Basic_10";
import { Common } from "./Common_20";
import { Inner_Basic } from "./Inner_Basic_2";
import { ShopMessage } from "./ShopMessage_200";
import { User } from "./User_300";

export default class ProtoMessageRegister {
	public static getProtoBuffList(): string[] {
		return [
			"resources/protobuf/Bag_100.proto",
			"resources/protobuf/Basic_10.proto",
			"resources/protobuf/Common_20.proto",
			"resources/protobuf/Inner_Basic_2.proto",
			"resources/protobuf/ShopMessage_200.proto",
			"resources/protobuf/User_300.proto",
		];
	}

	public static register(): void {
		Bag.ReqBagInfo.register();
		Bag.RespBagInfo.register();
		Bag.NotifyBagInfoChanged.register();
		Bag.ReqComposePet.register();
		Bag.RespComposePet.register();
		Bag.ReqUseItem.register();
		Bag.ReqSellItem.register();
		Bag.RespItemChange.register();
		Basic.ReqHeartBeat.register();
		Basic.NotifyHeartBeat.register();
		ShopMessage.C2S_GetShopItemList.register();
		ShopMessage.S2C_GetShopItemList.register();
		ShopMessage.C2S_GetLimitList.register();
		ShopMessage.S2C_GetLimitList.register();
		ShopMessage.C2S_GetShopPaymentList.register();
		ShopMessage.S2C_GetShopPaymentList.register();
		ShopMessage.C2S_GetPaymentList.register();
		ShopMessage.S2C_GetPaymentList.register();
		User.ReqLogin.register();
		User.RespLogin.register();
		User.ReqPlayerCreate.register();
		User.RespPlayerCreate.register();
		User.ReqPlayerList.register();
		User.RespPlayerList.register();
		User.ReqPlayerLogin.register();
		User.RespPlayerLogin.register();
		User.RespErrorCode.register();
		User.RespPrompt.register();
    }
}
