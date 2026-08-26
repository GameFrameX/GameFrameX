import MessageObject from "./MessageObject";

export default class NetWorkData {
    public _messageObject: MessageObject;
    public time: number = 0;
    private _uniqueId: number = 0;
    private _isRpc: boolean;
    public get messageObject(): MessageObject {
        return this._messageObject;
    };

    public get isRpc(): boolean {
        return this._isRpc;
    };
    public get uniqueId(): number {
        return this._uniqueId;
    }
    public constructor(_uniqueId: number, messageObject: MessageObject, isRpc: boolean = true) {
        this._uniqueId = _uniqueId;
        this._messageObject = messageObject;
        this._isRpc = isRpc;
    }
}
