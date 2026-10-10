import type MessageObject from "./MessageObject";

/**
 * 网络通道接口。
 *
 * 对照 Unity `GameFrameX.Network.Runtime.INetworkChannel` 的客户端使用面:
 * 连接/关闭/发送/RPC/连接状态。实现见 NetworkChannel。
 */
export default interface INetworkChannel {
    /** 通道名称。 */
    readonly Name: string;

    /** 是否已连接(可通信状态)。 */
    readonly IsConnected: boolean;

    /** 连接服务器。 */
    Connect(url: string): void;

    /** 关闭通道。 */
    Close(reason?: string): void;

    /** 发送消息(入队)。 */
    Send(message: MessageObject): void;

    /** 发起 RPC,等待同 UniqueId 响应。 */
    Call<TResponse extends MessageObject>(message: MessageObject): Promise<TResponse>;
}
