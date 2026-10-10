import IRequestMessage from "../network/IRequestMessage";
import IResponseMessage from "../network/IResponseMessage";
import INotifyMessage from "../network/INotifyMessage";
import IHeartBeatMessage from "../network/IHeartBeatMessage";
import MessageObject from "../network/MessageObject";
import ProtoMessageHelper from "../network/ProtoMessageHelper";

export namespace Mail {
	export enum MailErrorCode
	{
		MailNotFound = 500001, 
		MailAlreadyDeleted = 500002, 
		UnclaimedAttachment = 500003, 
		AttachmentNotFound = 500004, 
		AttachmentAlreadyClaimed = 500005, 
		UnclaimableAttachment = 500006, 
	}

	export class MailAttachmentInfo {
		SlotId:number;

		RewardType:number;

		ItemId:number;

		Count:number;

		ClaimStatus:number;

	}

	export class MailInfo {
		MailId:number;

		CampaignId:number;

		CampaignVersion:number;

		MailType:number;

		Title:string;

		ReadStatus:number;

		AttachmentStatus:number;

		MailStatus:number;

		CreateTime:number;

		ExpireTime:number;

		HasAttachment:boolean;

	}

	export class MailClaimedSlot {
		MailId:number;

		SlotId:number;

		RewardType:number;

		ItemId:number;

		Count:number;

		ClaimStatus:number;

		Success:boolean;

	}

	export class ReqMailList extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Mail.ReqMailList', 32768010);
		}

		public readonly PackageName: string = 'Mail.ReqMailList';

		Cursor:number;

		PageSize:number;

	}

	export class RespMailList extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Mail.RespMailList', 32768011);
		}

		public readonly PackageName: string = 'Mail.RespMailList';

		Mails:MailInfo;

		UnreadCount:number;

		HasMore:boolean;

		ErrorCode:number;

	}

	export class ReqMailRead extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Mail.ReqMailRead', 32768012);
		}

		public readonly PackageName: string = 'Mail.ReqMailRead';

		MailId:number;

	}

	export class RespMailRead extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Mail.RespMailRead', 32768013);
		}

		public readonly PackageName: string = 'Mail.RespMailRead';

		MailId:number;

		Title:string;

		Content:string;

		TemplateId:number;

		TemplateVersion:number;

		ReadStatus:number;

		MailStatus:number;

		Attachments:MailAttachmentInfo;

		ErrorCode:number;

	}

	export class ReqMailDelete extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Mail.ReqMailDelete', 32768014);
		}

		public readonly PackageName: string = 'Mail.ReqMailDelete';

		MailId:number;

	}

	export class RespMailDelete extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Mail.RespMailDelete', 32768015);
		}

		public readonly PackageName: string = 'Mail.RespMailDelete';

		MailId:number;

		ErrorCode:number;

	}

	export class NotifyMailChanged extends MessageObject implements INotifyMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Mail.NotifyMailChanged', 32768016);
		}

		public readonly PackageName: string = 'Mail.NotifyMailChanged';

		ChangedMailIds:number;

		UnreadCount:number;

	}

	export class ReqMailClaimAttachment extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Mail.ReqMailClaimAttachment', 32768017);
		}

		public readonly PackageName: string = 'Mail.ReqMailClaimAttachment';

		MailId:number;

		SlotId:number;

	}

	export class RespMailClaimAttachment extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Mail.RespMailClaimAttachment', 32768018);
		}

		public readonly PackageName: string = 'Mail.RespMailClaimAttachment';

		MailId:number;

		SlotId:number;

		RewardType:number;

		ItemId:number;

		Count:number;

		ClaimStatus:number;

		MailStatus:number;

		AttachmentStatus:number;

		ErrorCode:number;

	}

	export class ReqMailClaimAllAttachment extends MessageObject implements IRequestMessage {

		public static register(): void{
			ProtoMessageHelper.registerReqMessage('Mail.ReqMailClaimAllAttachment', 32768019);
		}

		public readonly PackageName: string = 'Mail.ReqMailClaimAllAttachment';

	}

	export class RespMailClaimAllAttachment extends MessageObject implements IResponseMessage {

		public static register(): void{
			ProtoMessageHelper.registerRespMessage('Mail.RespMailClaimAllAttachment', 32768020);
		}

		public readonly PackageName: string = 'Mail.RespMailClaimAllAttachment';

		Slots:MailClaimedSlot;

		ClaimedCount:number;

		ErrorCode:number;

	}

}
