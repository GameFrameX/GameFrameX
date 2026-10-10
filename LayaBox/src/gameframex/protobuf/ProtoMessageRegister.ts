// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
// 
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
// 
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
// 
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
// 
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

import { RockPaperScissors } from "./_0410_RockPaperScissors";
import { Attribute } from "./_0310_Attribute";
import { User } from "./_0300_User";
import { Social } from "./_0120_Social";
import { Bag } from "./_0100_Bag";
import { Room } from "./_0400_Room";
import { Common } from "./_0020_Common";
import { Basic } from "./_0010_Basic";
import { Mail } from "./_0500_Mail";

export default class ProtoMessageRegister {
	public static getProtoBuffList(): string[] {
		return [
			"resources/protobuf/_0410_RockPaperScissors.proto",
			"resources/protobuf/_0310_Attribute.proto",
			"resources/protobuf/_0300_User.proto",
			"resources/protobuf/_0120_Social.proto",
			"resources/protobuf/_0100_Bag.proto",
			"resources/protobuf/_0400_Room.proto",
			"resources/protobuf/_0020_Common.proto",
			"resources/protobuf/_0010_Basic.proto",
			"resources/protobuf/_0500_Mail.proto",
		];
	}

	public static register(): void {
		RockPaperScissors.ReqRockPaperScissorsGameInfo.register();
		RockPaperScissors.RespRockPaperScissorsGameInfo.register();
		RockPaperScissors.ReqSubmitRockPaperScissorsGesture.register();
		RockPaperScissors.RespSubmitRockPaperScissorsGesture.register();
		RockPaperScissors.ReqRestartRockPaperScissorsGame.register();
		RockPaperScissors.RespRestartRockPaperScissorsGame.register();
		RockPaperScissors.NotifyRockPaperScissorsGameChanged.register();
		Attribute.NotifyPlayerAttributeSync.register();
		Attribute.NotifyPlayerAttributeChanged.register();
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
		Social.ReqSocialInfo.register();
		Social.RespSocialInfo.register();
		Social.ReqDeleteFriend.register();
		Social.RespDeleteFriend.register();
		Social.ReqFriendList.register();
		Social.RespFriendList.register();
		Social.ReqFriendByAdd.register();
		Social.RespFriendByAdd.register();
		Bag.ReqBagInfo.register();
		Bag.RespBagInfo.register();
		Bag.NotifyBagItem.register();
		Bag.NotifyBagInfoChanged.register();
		Bag.ReqComposePet.register();
		Bag.RespComposePet.register();
		Bag.ReqUseItem.register();
		Bag.RespUseItem.register();
		Bag.ReqDiscardItem.register();
		Bag.RespDiscardItem.register();
		Bag.ReqSellItem.register();
		Bag.RespSellItem.register();
		Bag.ReqAddItem.register();
		Bag.RespAddItem.register();
		Bag.ReqRemoveItem.register();
		Bag.RespRemoveItem.register();
		Room.ReqRoomList.register();
		Room.RespRoomList.register();
		Room.ReqCreateRoom.register();
		Room.RespCreateRoom.register();
		Room.ReqJoinRoom.register();
		Room.RespJoinRoom.register();
		Room.ReqLeaveRoom.register();
		Room.RespLeaveRoom.register();
		Room.ReqStartRoomGame.register();
		Room.RespStartRoomGame.register();
		Room.NotifyRoomChanged.register();
		Basic.ReqHeartBeat.register();
		Basic.NotifyHeartBeat.register();
		Basic.NotifyServerFullyLoaded.register();
		Mail.ReqMailList.register();
		Mail.RespMailList.register();
		Mail.ReqMailRead.register();
		Mail.RespMailRead.register();
		Mail.ReqMailDelete.register();
		Mail.RespMailDelete.register();
		Mail.NotifyMailChanged.register();
		Mail.ReqMailClaimAttachment.register();
		Mail.RespMailClaimAttachment.register();
		Mail.ReqMailClaimAllAttachment.register();
		Mail.RespMailClaimAllAttachment.register();
    }
}
