import type ISocketTransport from "./ISocketTransport";

/**
 * 标准 WebSocket 传输适配器。
 *
 * 使用宿主全局 WebSocket:浏览器、node(≥22)、微信小游戏(适配层可覆盖)通用。
 * 二进制模式(subprotocol 无),收发 ArrayBuffer。
 */
export default class WebSocketTransport implements ISocketTransport {
    private socket: WebSocket | null = null;

    public onOpen: () => void = () => { };
    public onMessage: (data: ArrayBuffer) => void = () => { };
    public onClose: (reason?: string) => void = () => { };
    public onError: (reason?: string) => void = () => { };

    /**
     * 是否已建立连接。
     */
    public get connected(): boolean {
        return this.socket?.readyState === WebSocket.OPEN;
    }

    /**
     * 建立连接;已存在旧连接时先关闭。
     * @param url ws:// 或 wss:// 完整地址
     */
    public connect(url: string): void {
        this.detach();

        const socket = new WebSocket(url);
        socket.binaryType = 'arraybuffer';
        socket.onopen = () => this.onOpen();
        socket.onmessage = (event: MessageEvent<ArrayBuffer>) => this.onMessage(event.data);
        socket.onclose = (event: CloseEvent) => this.onClose(event.reason);
        socket.onerror = () => this.onError('websocket error');
        this.socket = socket;
    }

    /**
     * 发送二进制帧。
     */
    public send(buffer: ArrayBuffer): void {
        this.socket?.send(buffer);
    }

    /**
     * 主动关闭连接。
     */
    public close(): void {
        this.socket?.close();
    }

    /** 解绑事件引用(连接复用前清理),不触发 close 回调。 */
    private detach(): void {
        if (this.socket == null) {
            return;
        }

        this.socket.onopen = null;
        this.socket.onmessage = null;
        this.socket.onclose = null;
        this.socket.onerror = null;
        this.socket = null;
    }
}
