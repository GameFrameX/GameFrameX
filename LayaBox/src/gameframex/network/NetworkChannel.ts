import type ISocketTransport from "./ISocketTransport";
import MessageObject from "./MessageObject";
import IModule from "../IModule";
import LogHelper from "../log/LogHelper";
import TimerHelper from "../timer/TimerHelper";
import ProtoMessageHelper from "./ProtoMessageHelper";
import ProtoMessageHandler from "./ProtoMessageHandler";
import { Basic } from "../protobuf/_0010_Basic";
import { encodePacketHeader, decodePacketHeader, PACKET_HEADER_LENGTH, OPERATION_TYPE_MESSAGE, OPERATION_TYPE_HEARTBEAT } from "./PacketHeader";

/** RPC 等待条目。 */
interface PendingRpc {
    resolve: (message: MessageObject) => void;
    reject: (reason: Error) => void;
    messageName: string;
    elapsedMilliseconds: number;
}

/** 通道配置(默认值对齐 Unity 实际运行值,见类注释)。 */
export interface NetworkChannelOptions {
    /** 心跳间隔秒数。Unity Hotfix DefaultPacketHeartBeatHandler = 30。 */
    heartBeatIntervalSeconds?: number;
    /** 心跳丢失多少次后关闭通道。Unity BasePacketHeartBeatHandler.MissHeartBeatCountByClose = 5。 */
    missHeartBeatCountByClose?: number;
    /** RPC 超时毫秒。Unity NetworkComponent.m_rpcTimeout = 5000。 */
    rpcTimeoutMilliseconds?: number;
    /** 是否自动重连(Unity 基线无自动重连,重连是业务层行为)。 */
    autoReconnect?: boolean;
}

const DEFAULT_HEART_BEAT_INTERVAL = 30;
const DEFAULT_MISS_HEART_BEAT_CLOSE = 5;
const DEFAULT_RPC_TIMEOUT = 5000;
/** 单包上限(64MB),防御脏数据。 */
const MAX_PACKET_LENGTH = 64 * 1024 * 1024;

/**
 * 网络通道。
 *
 * 对照 Unity `GameFrameX.Network.Runtime.NetworkChannelBase` 的客户端使用面:
 * Connect/Close/Send/Call + 心跳 + RPC 配对 + Notify 分发。
 *
 * 差异与定案(依据 Unity 源码核实,2026-09-18):
 * - 包头 14 字节大端:uint32 整包长(含头)+ byte operationType(心跳=1/普通=4)
 *   + byte zipFlag + int32 uniqueId + int32 messageId;
 * - 心跳:间隔 30s,每收到任意包 missCount 归零,missCount > 5 关闭通道;
 * - RPC:UniqueId 配对,超时 5000ms,Close 时全部以异常收场;
 * - 重连:Unity 无自动重连;本实现提供可选 autoReconnect(默认关),重连成功发
 *   NetworkReconnected 事件作为"重登钩子"入口;
 * - 轮询:由 NetworkManager(注册进 GameEntry)每帧驱动 update,不发自身定时器。
 */
export default class NetworkChannel implements IModule {
    /** 通道名(注册键)。 */
    public readonly Name: string;

    /** 是否已连接(可通信)。 */
    public get IsConnected(): boolean {
        return this.active && this.transport.connected;
    }

    /** 通道级事件回调(NetworkManager 转发为 EventSystems 事件)。 */
    public onConnected: (channel: NetworkChannel) => void = () => { };
    /** 重连成功回调(重登钩子:仅断线后重连时触发,首次连接不触发)。 */
    public onReconnected: (channel: NetworkChannel) => void = () => { };
    public onClosed: (channel: NetworkChannel, reason: string) => void = () => { };
    public onMissHeartBeat: (channel: NetworkChannel, missCount: number) => void = () => { };
    public onError: (channel: NetworkChannel, reason: string) => void = () => { };

    private readonly transport: ISocketTransport;
    private readonly options: Required<NetworkChannelOptions>;
    private url: string = '';
    private active: boolean = false;

    /** 待发送队列(Send 入队,update 中序列化发出)。 */
    private readonly sendQueue: MessageObject[] = [];
    /** 是否有过一次成功会话(用于区分首连与重连)。 */
    private hadSession: boolean = false;


    private readonly receiveQueue: MessageObject[] = [];

    /** 字节流重组缓冲(WS 帧可能粘包/半包)。 */
    private streamBuffer: Uint8Array = new Uint8Array(0);

    /** 心跳状态。 */
    private heartBeatElapse = 0;
    private missHeartBeatCount = 0;

    /** RPC 等待表:UniqueId → PendingRpc。 */
    private readonly pendingRpc: Map<number, PendingRpc> = new Map();

    private reconnectAttempts = 0;

    /**
     * @param name 通道名
     * @param transport 传输适配器(WebSocket 等)
     * @param options 通道配置
     */
    public constructor(name: string, transport: ISocketTransport, options: NetworkChannelOptions = {}) {
        this.Name = name;
        this.transport = transport;
        this.options = {
            heartBeatIntervalSeconds: options.heartBeatIntervalSeconds ?? DEFAULT_HEART_BEAT_INTERVAL,
            missHeartBeatCountByClose: options.missHeartBeatCountByClose ?? DEFAULT_MISS_HEART_BEAT_CLOSE,
            rpcTimeoutMilliseconds: options.rpcTimeoutMilliseconds ?? DEFAULT_RPC_TIMEOUT,
            autoReconnect: options.autoReconnect ?? false,
        };

        this.transport.onOpen = () => this.handleOpen();
        this.transport.onMessage = (data) => this.handleMessage(data);
        this.transport.onClose = (reason) => this.handleClose('transport closed: ' + (reason ?? 'unknown'), true);
        this.transport.onError = (reason) => this.handleError(reason ?? 'transport error');
    }

    /**
     * 连接服务器。
     * @param url ws:// 或 wss:// 完整地址
     */
    public Connect(url: string): void {
        this.url = url;
        this.streamBuffer = new Uint8Array(0);
        LogHelper.log(`[Network:${this.Name}] connecting ${url}`);
        this.transport.connect(url);
    }

    /**
     * 关闭通道并清理全部状态;等待中的 RPC 以异常收场。
     * @param reason 关闭原因
     */
    public Close(reason: string = 'client close'): void {
        this.handleClose(reason, false);
        this.transport.close();
    }

    /**
     * 发送消息(入队,update 中发出)。未连接时消息滞留队列直到连接建立。
     * @param message 消息实例
     */
    public Send(message: MessageObject): void {
        this.sendQueue.push(message);
    }

    /**
     * 发起 RPC:发送请求并等待同 UniqueId 的响应。
     * @param message 请求消息(实现 IRequestMessage)
     * @returns 响应消息(Promise)
     */
    public Call<TResponse extends MessageObject>(message: MessageObject): Promise<TResponse> {
        return new Promise<TResponse>((resolve, reject) => {
            this.pendingRpc.set(message.UniqueId, {
                resolve: resolve as (message: MessageObject) => void,
                reject,
                messageName: message.PackageName,
                elapsedMilliseconds: 0,
            });
            this.Send(message);
        });
    }

    /**
     * 通道轮询(由 NetworkManager 每帧调用):
     * 发送队列 → 心跳 → RPC 超时 → 接收分发。
     * @param _elapseSeconds 逻辑流逝秒
     * @param realElapseSeconds 真实流逝秒
     */
    public update(_elapseSeconds: number, realElapseSeconds: number): void {
        this.processSend();
        this.processHeartBeat(realElapseSeconds);
        this.processRpcTimeout(realElapseSeconds);
        this.processReceivedMessage();
    }

    /**
     * 模块初始化(通道由 NetworkManager 生命周期管理,无异步就绪动作)。
     */
    public async init(): Promise<void> { }

    /**
     * 模块关闭。
     */
    public shutdown(): void {
        this.Close('shutdown');
    }

    private handleOpen(): void {
        this.active = true;
        this.reconnectAttempts = 0;
        this.heartBeatElapse = 0;
        this.missHeartBeatCount = 0;
        LogHelper.log(`[Network:${this.Name}] connected`);
        this.onConnected(this);
        if (this.hadSession) {
            this.onReconnected(this);
        }

        this.hadSession = true;
    }

    private handleMessage(data: ArrayBuffer): void {
        // 任一收到包:心跳丢失计数归零(对照 Unity ReceiveCallback 的 HeartBeatState.Reset)。
        this.missHeartBeatCount = 0;
        this.streamBuffer = concat(this.streamBuffer, new Uint8Array(data));
        this.parseStream();
    }

    private parseStream(): void {
        while (true) {
            const header = decodePacketHeader(this.streamBuffer.buffer as ArrayBuffer, this.streamBuffer.byteOffset);
            if (header == null) {
                return;
            }
            const body = this.streamBuffer.subarray(PACKET_HEADER_LENGTH, header.packetLength);
            if (header.zipFlag !== 0) {
                LogHelper.error(`[Network:${this.Name}] zipped packet not supported (messageId=${header.messageId}), dropped`);
            } else {
                this.decodeMessage(header, body);
            }

            this.streamBuffer = this.streamBuffer.subarray(header.packetLength);
        }
    }

    private decodeMessage(header: { uniqueId: number; messageId: number }, body: Uint8Array): void {
        const messageName = ProtoMessageHelper.getRespMessageName(header.messageId);
        if (messageName == null) {
            LogHelper.log(`[Network:${this.Name}] unknown messageId ${header.messageId}, ignored`);
            return;
        }

        const type = ProtoMessageHelper.getMessageType(messageName);
        const decoded = type.decode(body) as Record<string, unknown>;
        const message = new MessageObject();
        Object.assign(message, decoded);
        message.PackageName = messageName;
        message.UniqueId = header.uniqueId;
        message.MessageId = header.messageId;
        this.receiveQueue.push(message);
    }

    private processSend(): void {
        if (!this.IsConnected) {
            return;
        }

        while (this.sendQueue.length > 0) {
            const message = this.sendQueue.shift()!;
            this.writeMessage(message);
        }
    }

    private writeMessage(message: MessageObject): void {
        const fullName = message.PackageName;
        const type = ProtoMessageHelper.getMessageType(fullName);
        const messageId = ProtoMessageHelper.getReqMessageId(fullName);
        const body = type.encode(type.create(message as never)).finish();
        const isHeartBeat = ProtoMessageHelper.isHeartBeat(fullName);
        const header = encodePacketHeader({
            packetLength: PACKET_HEADER_LENGTH + body.byteLength,
            operationType: isHeartBeat ? OPERATION_TYPE_HEARTBEAT : OPERATION_TYPE_MESSAGE,
            zipFlag: 0,
            uniqueId: message.UniqueId,
            messageId,
        });

        const frame = new Uint8Array(PACKET_HEADER_LENGTH + body.byteLength);
        frame.set(new Uint8Array(header), 0);
        frame.set(body, PACKET_HEADER_LENGTH);
        this.transport.send(frame.buffer);
    }

    private processHeartBeat(realElapseSeconds: number): void {
        if (!this.IsConnected) {
            return;
        }

        this.heartBeatElapse += realElapseSeconds;
        if (this.heartBeatElapse < this.options.heartBeatIntervalSeconds) {
            return;
        }

        this.heartBeatElapse = 0;
        const previousMissCount = this.missHeartBeatCount;
        this.missHeartBeatCount++;

        if (previousMissCount > 0) {
            this.onMissHeartBeat(this, previousMissCount);
        }

        const heartBeat = new Basic.ReqHeartBeat();
        heartBeat.Timestamp = TimerHelper.UnixTimeMilliseconds();
        this.Send(heartBeat);

        if (this.missHeartBeatCount > this.options.missHeartBeatCountByClose) {
            this.Close('miss heart beat');
        }
    }

    private processRpcTimeout(realElapseSeconds: number): void {
        if (this.pendingRpc.size === 0) {
            return;
        }

        const elapsed = realElapseSeconds * 1000;
        for (const [uniqueId, pending] of this.pendingRpc) {
            pending.elapsedMilliseconds += elapsed;
            if (pending.elapsedMilliseconds >= this.options.rpcTimeoutMilliseconds) {
                this.pendingRpc.delete(uniqueId);
                pending.reject(new Error(`rpc timeout: ${pending.messageName} (${this.options.rpcTimeoutMilliseconds}ms)`));
            }
        }
    }

    private processReceivedMessage(): void {
        if (this.receiveQueue.length === 0) {
            return;
        }

        const messages = [...this.receiveQueue];
        this.receiveQueue.length = 0;
        for (const message of messages) {
            const pending = this.pendingRpc.get(message.UniqueId);
            if (pending != null) {
                this.pendingRpc.delete(message.UniqueId);
                pending.resolve(message);
                continue;
            }

            ProtoMessageHandler.dispatch(message);
        }
    }

    private handleClose(reason: string, fromTransport: boolean): void {
        if (!this.active && fromTransport) {
            return;
        }

        const wasActive = this.active;
        this.active = false;
        this.heartBeatElapse = 0;
        this.missHeartBeatCount = 0;
        this.streamBuffer = new Uint8Array(0);
        this.receiveQueue.length = 0;

        for (const [uniqueId, pending] of this.pendingRpc) {
            pending.reject(new Error(`network closed: ${reason}`));
        }
        this.pendingRpc.clear();

        if (wasActive || !fromTransport) {
            LogHelper.log(`[Network:${this.Name}] closed: ${reason}`);
            this.onClosed(this, reason);
        }

        if (wasActive && fromTransport && this.options.autoReconnect) {
            this.scheduleReconnect();
        }
    }

    private scheduleReconnect(): void {
        this.reconnectAttempts++;
        if (this.reconnectAttempts > 5) {
            this.onError(this, 'reconnect attempts exhausted');
            return;
        }

        setTimeout(() => {
            if (!this.active && this.url !== '') {
                LogHelper.log(`[Network:${this.Name}] reconnecting (attempt ${this.reconnectAttempts})`);
                this.transport.connect(this.url);
            }
        }, 1000 * this.reconnectAttempts);
    }

    private handleError(reason: string): void {
        LogHelper.error(`[Network:${this.Name}] ${reason}`);
        this.onError(this, reason);
    }
}

function concat(left: Uint8Array, right: Uint8Array): Uint8Array {
    const merged = new Uint8Array(left.byteLength + right.byteLength);
    merged.set(left, 0);
    merged.set(right, left.byteLength);
    return merged;
}
