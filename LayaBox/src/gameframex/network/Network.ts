import EventName from "../event/EventName";
import EventSystems from "../event/EventSystems";
import LogHelper from "../log/LogHelper";
import IMessage from "./IMessage";
import ProtoMessageHelper from "./ProtoMessageHelper";
import { NetworkSatus } from "./NetworkSatus";
import MessageObject from "./MessageObject";
import NetWorkData from "./NetWorkData";
export default class Network {
    private _reconnectCount: number = 0;
    private _isReconnect: boolean = true;
    private _socket: Laya.Socket;
    private _host: string;
    private _port: number;
    private _onConnected: Function;
    private _lastTick: number = 0;
    private _sendByte: Laya.Byte;
    private _recvByte: Laya.Byte;
    private _socketStatus: number = NetworkSatus.STATUS_DISCONNECT;
    private _curSendArr: any[] = [];
    private _curSendTime: number = 0;
    private _lastRecvTime: number = 0;
    private _sendWaitingMap: NetWorkData[] = []
    private _recvWaitingMap: NetWorkData[] = []
    private _sendingMap: NetWorkData[] = []
    private count: number = 1000;

    public call(message: MessageObject) {
        this.count++;
        let messageData: NetWorkData = new NetWorkData(this.count, message);
        this._sendWaitingMap.push(messageData);
    }

    constructor() {
        this.count = 1000;
        this._sendByte = new Laya.Byte();
        this._sendByte.endian = Laya.Byte.BIG_ENDIAN;
        this._recvByte = new Laya.Byte();
        this._recvByte.endian = Laya.Byte.BIG_ENDIAN;
        this._socket = new Laya.Socket();
        this._socket.endian = Laya.Byte.BIG_ENDIAN;
        this.addEvents();
        Laya.timer.loop(5000, this, this.onHeart);
        Laya.timer.frameLoop(1, this, this.onFrameUpdate);
    }
    onFrameUpdate() {
        if (this._socketStatus == NetworkSatus.STATUS_DISCONNECT) {
            return;
        }
        this.checkSendList();

        while (this._sendingMap.length > 0) {
            let info = this._sendingMap.shift();
            if (info) {
                this.send(info.messageObject);
                if (info.isRpc) {
                    this._recvWaitingMap.push(info);
                }
            }
        }
    }

    private addEvents() {
        this._socket.on(Laya.Event.OPEN, this, this.openHandler);
        this._socket.on(Laya.Event.MESSAGE, this, this.receiveHandler);
        this._socket.on(Laya.Event.CLOSE, this, this.closeHandler);
        this._socket.on(Laya.Event.ERROR, this, this.errorHandler);
    }

    private openHandler(event: any = null): void {
        EventSystems.dispatch(EventName.SocketConnected, event);
        let dt = Laya.timer.currTimer - this._lastTick;
        LogHelper.log(`连接成功: host:${this._host} port:${this._port} 时长:${dt}`);
        this.updateStatus(NetworkSatus.STATUS_COMMUNICATION);

        // 校验
        if (this._onConnected) {
            this._onConnected();
        }
    }

    private closeHandler(e: any = null): void {
        LogHelper.log("与服务器的连接断开了！", e);
        EventSystems.dispatch(EventName.SocketClose, e);
        this.close();
        this.checkReconnect();
    }

    private errorHandler(e: any = null): void {
        LogHelper.log("连接出错，开始重连！", e);
        EventSystems.dispatch(EventName.SocketError, e);
        this.close();
        this.checkReconnect();
    }

    private checkReconnect(): void {
        if (!this._isReconnect) {
            return;
        }
        Laya.timer.once(2000, this, this.reconnect);
    }
    //------------------------------事件结束------------------------------

    setOnConnected(ex: Function, obj: any) {
        this._onConnected = ex.bind(obj);
    }

    connect(host: string, port: number = 0): void {
        Laya.timer.clear(this, this.checkConnectTimeOut);
        Laya.timer.clear(this, this.continueReconnect);
        LogHelper.log(`开始连接: host:${host} port:${port}`);
        if (this._socketStatus != NetworkSatus.STATUS_DISCONNECT) {
            return;
        }
        Laya.timer.clear(this, this.reconnect);
        this._lastTick = Laya.timer.currTimer;
        this._host = host;
        this._port = port;
        this.updateStatus(NetworkSatus.STATUS_CONNECTING);
        this._socket.connectByUrl(host + ":" + port);
        Laya.timer.once(1000, this, this.checkConnectTimeOut);
        // if (location.protocol.indexOf("https:") != -1) {
        //     this._socket.connectByUrl(`ws://${host}:${port}`);
        // } else {
        //     this._socket.connect(host, port);
        // }
    }

    private checkConnectTimeOut(): void {
        if (this._socketStatus != NetworkSatus.STATUS_CHECKING && this._socketStatus != NetworkSatus.STATUS_COMMUNICATION) {
            //防止第一次连接，没连接上，收不到后端返回消息，socket状态不清理，后续手动发起连接无效的问题
            this.updateStatus(NetworkSatus.STATUS_DISCONNECT);
        }
    }

    close(isDrive?: boolean): void {
        if (isDrive) {
            this._reconnectCount = 3;
        }

        this._curSendArr = [];
        Laya.timer.clear(this, this.checkSendList);
        this.updateStatus(NetworkSatus.STATUS_DISCONNECT);
        Laya.timer.clear(this, this.checkConnectTimeOut);
        Laya.timer.clear(this, this.continueReconnect);
        EventSystems.dispatch(EventName.SocketClose);
        if (!this._socket)
            return;
        this._socket.close();
    }

    reconnect(isClick?: boolean): void {
        if (isClick) {
            this._reconnectCount = 0;
        }

        if (this._reconnectCount > 2) {
            //网络断开了
            return;
        }
        this.connect(this._host, this._port);
        this._reconnectCount++;
        // EventSystems.dispatch(MsgUtil.SocketReconnect);
        Laya.timer.once(1000, this, this.continueReconnect);
    }

    /**
     * 使用计时器检测发起重连，不依赖于后端返回连接状态，避免socket重连中止；
     */
    private continueReconnect() {
        if (this._socketStatus === NetworkSatus.STATUS_COMMUNICATION) {
            return;
        }
        LogHelper.log('计时器触发 继续重连');
        if (this._socketStatus != NetworkSatus.STATUS_CHECKING) {
            this.reconnect();
        }
    }

    private updateStatus(status: number): void {
        if (this._socketStatus == status) {
            return;
        }
        this._socketStatus = status;
    }

    private onHeart() {
        if (this._socketStatus == NetworkSatus.STATUS_COMMUNICATION) {
            //心跳只能在socket连接成功，且登录请求成功之后 才可以发送
            // this.send("Basic.ReqHeartBeat", { Timestamp: Math.floor(Laya.timer.currTimer / 1000) });
        }
    }

    isConnected(): boolean {
        if (this._socket && this._socket.connected) {
            return true;
        } else {
            return false;
        }
    }

    public send(message: MessageObject): boolean {
        if (!this._socket.connected) {
            return false;
        }

        var MsgClass = ProtoMessageHelper.getMessageType(message.PackageName);
        var messageId = ProtoMessageHelper.getMessageIdByModule(message.PackageName);
        if (!MsgClass) {
            return false;
        }

        let messageData = MsgClass.create(message);
        var buffer = MsgClass.encode(messageData).finish();
        let len = 12 + buffer.byteLength;
        this._sendByte.writeUint16(len); // length
        this._sendByte.writeByte(4);// operationType
        this._sendByte.writeByte(0); // zipFlag
        this._sendByte.writeInt32(message.UniqueId); // uniqueId
        this._sendByte.writeInt32(messageId); // uniqueId
        this._sendByte.writeArrayBuffer(buffer);
        this._socket.send(this._sendByte.buffer);
        this._sendByte.clear();
        this._sendByte.pos = 0;
        return true;
    }
    private receiveHandler(data: any = null): void {
        this._lastRecvTime = Laya.timer.currTimer;
        this._recvByte.clear();
        this._recvByte.writeArrayBuffer(data);
        this._recvByte.pos = 0;
        let pkgLen = this._recvByte.length;

        if (pkgLen < 12) {
            return;
        }

        let msgTotalLength = this._recvByte.readUint16();
        //operationType
        let operationType = this._recvByte.readByte();
        //zipFlag
        let zipFlag = this._recvByte.readByte();
        let uniqueId = this._recvByte.readInt32();
        let MessageId = this._recvByte.readInt32();
        let messageBuffer = this._recvByte.readUint8Array(2 + 1 + 1 + 4 + 4, msgTotalLength - 12);

        let messageType: IMessage = ProtoMessageHelper.getMessageRespTypeByIndex(MessageId);
        let messageClassObject: MessageObject = new MessageObject();
        // 解析数据结果对象
        let message: any = messageType.decode(messageBuffer);
        Object.assign(messageClassObject, message);
        messageClassObject.UniqueId = uniqueId;
        messageClassObject.MessageId = MessageId;
        console.log(messageClassObject);
    }

    checkSendList(): void {
        if (this._socketStatus != NetworkSatus.STATUS_COMMUNICATION) {
            return;
        }
        while (this._sendWaitingMap.length) {
            let info = this._sendWaitingMap.shift();
            if (info) {
                this._sendingMap.push(info);
            }
        }
    }
}