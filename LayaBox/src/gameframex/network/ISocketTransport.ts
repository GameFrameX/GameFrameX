/**
 * Socket 传输适配器接口。
 *
 * NetworkChannel 的传输无关抽象:浏览器/微信小游戏/node 各自提供实现,
 * 网络通道本体(状态机/心跳/RPC/分发)不感知引擎。
 */
export default interface ISocketTransport {
    /** 是否已建立连接。 */
    readonly connected: boolean;

    /** 建立连接。重复调用由实现自行忽略。 */
    connect(url: string): void;

    /** 发送二进制帧。 */
    send(buffer: ArrayBuffer): void;

    /** 主动关闭连接。 */
    close(): void;

    /** 连接建立回调。 */
    onOpen: () => void;
    /** 收到二进制帧回调。 */
    onMessage: (data: ArrayBuffer) => void;
    /** 连接关闭回调(主动/被动)。 */
    onClose: (reason?: string) => void;
    /** 错误回调。 */
    onError: (reason?: string) => void;
}
