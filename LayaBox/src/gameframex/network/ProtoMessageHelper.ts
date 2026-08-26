import ProtoMessageRegister from "../protobuf/ProtoMessageRegister";
import IMessage from "./IMessage";
import MessageObject from "./MessageObject";

export default class ProtoMessageHelper {
    private static _protobuff: any = null;
    private static pb: any;
    private static _messageReqMap: Map<number, IMessage> = new Map<number, IMessage>();
    private static _messageRespMap: Map<number, IMessage> = new Map<number, IMessage>();
    private static _messageMap: Map<number, IMessage> = new Map<number, IMessage>();
    private static _messageList: MessageClassObjectData[] = [];
    public static init(): void {
        this._protobuff = (window as any).protobuf;
        this._protobuff.load(ProtoMessageRegister.getProtoBuffList(), (err: any, root: any) => {
            if (err) {
                console.log(err);
                return;
            }

            this.pb = root;
            ProtoMessageRegister.register();
        });
    }
    public static registerRespMessage(moduleName: string, messageId: number, messageClassObject: MessageObject = null): void {
        if (!this._messageRespMap.has(messageId)) {
            let messageClass = this.getMessageType(moduleName);
            this._messageRespMap.set(messageId, messageClass);
            this._messageMap.set(messageId, messageClass);
            let messageClassObjectData: MessageClassObjectData = new MessageClassObjectData(moduleName, messageId, messageClassObject);
            this._messageList.push(messageClassObjectData);
        }
    }
    public static registerReqMessage(moduleName: string, messageId: number, messageClassObject: MessageObject = null): void {
        if (!this._messageReqMap.has(messageId)) {
            let messageClass = this.getMessageType(moduleName);
            this._messageReqMap.set(messageId, messageClass);
            this._messageMap.set(messageId, messageClass);
            let messageClassObjectData: MessageClassObjectData = new MessageClassObjectData(moduleName, messageId, messageClassObject);
            this._messageList.push(messageClassObjectData);
        }
    }

    public static getMessageIdByModule(moduleName: string): number {

        for (let index = 0; index < this._messageList.length; index++) {
            const element = this._messageList[index];
            if (element.ModuleName == moduleName) {
                return element.MessageId;
            }
        }
        return 0;

    }
    public static getMessageObjectByMessageId(messageId: number): MessageObject {
        for (let index = 0; index < this._messageList.length; index++) {
            const element = this._messageList[index];
            if (element.MessageId == messageId) {
                return element.MessageClass;
            }
        }
        return null;
    }
    public static getMessageTypeByModule(moduleName: string, typeName: string): IMessage {
        return this.pb.lookupType(moduleName + '.' + typeName);
    }

    public static getMessageType(typeName: string): IMessage {
        return this.pb.lookupType(typeName);
    }
    public static getIndexByMessageType(typeName: string): any {
        let types = typeName.split(".");
        if (types.length == 2) {
            return this._protobuff[types[0]][types[1]];
        }
    }
    public static getMessageRespTypeByIndex(MessageId: number): IMessage {
        return this._messageRespMap.get(MessageId);
    }
    public static getMessageReqTypeByIndex(MessageId: number): IMessage {
        return this._messageReqMap.get(MessageId);
    }
}
class MessageClassObjectData {

    private _moduleName: string;
    private _messageId: number;
    private _messageClass: MessageObject;
    public get MessageId(): number {
        return this._messageId;
    }
    public get MessageClass(): MessageObject {
        return this._messageClass;
    }
    public get ModuleName(): string {
        return this._moduleName;
    }

    constructor(moduleName: string, messageId: number, messageClass: MessageObject) {
        this._moduleName = moduleName;
        this._messageId = messageId;
        this._messageClass = messageClass;
    }

}