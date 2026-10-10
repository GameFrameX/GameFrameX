import NetworkChannel from "./NetworkChannel";
import IModule from "../IModule";
import EventSystems from "../event/EventSystems";
import { NetworkConnectedEventArgs, NetworkClosedEventArgs, NetworkErrorEventArgs, NetworkMissHeartBeatEventArgs, NetworkReconnectedEventArgs } from "../event/NetworkEventArgs";

/**
 * 网络管理器。
 *
 * 对照 Unity `GameFrameX.Network.Runtime.NetworkComponent`:
 * CreateNetworkChannel/GetNetworkChannel/HasNetworkChannel/DestroyNetworkChannel;
 * 通道事件转 Fire 为 EventSystems 事件(NetworkConnected/Closed/Error/MissHeartBeat/Reconnected);
 * 注册进 GameEntry 后每帧驱动全部通道。
 */
export default class NetworkManager implements IModule {
    private readonly channels: Map<string, NetworkChannel> = new Map();

    /**
     * 网络通道数量。
     */
    public get NetworkChannelCount(): number {
        return this.channels.size;
    }

    /**
     * 是否存在指定通道。
     * @param channelName 通道名
     */
    public HasNetworkChannel(channelName: string): boolean {
        return this.channels.has(channelName);
    }

    /**
     * 获取指定通道。
     * @param channelName 通道名
     */
    public GetNetworkChannel(channelName: string): NetworkChannel {
        const channel = this.channels.get(channelName);
        if (channel == null) {
            throw new Error(`network channel '${channelName}' is not created`);
        }

        return channel;
    }

    /**
     * 获取全部通道。
     */
    public GetAllNetworkChannels(): NetworkChannel[] {
        return [...this.channels.values()];
    }

    /**
     * 创建网络通道并接事件转发。
     * @param channel 通道实例(含传输适配器与配置)
     */
    public CreateNetworkChannel(channel: NetworkChannel): NetworkChannel {
        if (this.channels.has(channel.Name)) {
            throw new Error(`network channel '${channel.Name}' already exists`);
        }

        this.wireEvents(channel);
        this.channels.set(channel.Name, channel);
        return channel;
    }

    /**
     * 销毁指定通道。
     * @param channelName 通道名
     */
    public DestroyNetworkChannel(channelName: string): boolean {
        const channel = this.channels.get(channelName);
        if (channel == null) {
            return false;
        }

        channel.Close('destroy');
        return this.channels.delete(channelName);
    }

    /**
     * 模块初始化(由 GameEntry 调用)。
     */
    public async init(): Promise<void> { }

    /**
     * 模块轮询:驱动全部通道(由 GameEntry 每帧调用)。
     */
    public update(elapseSeconds: number, realElapseSeconds: number): void {
        for (const channel of this.channels.values()) {
            channel.update(elapseSeconds, realElapseSeconds);
        }
    }

    /**
     * 模块关闭:销毁全部通道。
     */
    public shutdown(): void {
        for (const name of [...this.channels.keys()]) {
            this.DestroyNetworkChannel(name);
        }
    }

    private wireEvents(channel: NetworkChannel): void {
        channel.onReconnected = (sender) => EventSystems.Fire(this, NetworkReconnectedEventArgs.Create(sender));
        channel.onClosed = (sender, reason) => EventSystems.Fire(this, NetworkClosedEventArgs.Create(sender));
        channel.onError = (sender, reason) => EventSystems.Fire(this, NetworkErrorEventArgs.Create(sender, reason));
        channel.onMissHeartBeat = (sender, missCount) => EventSystems.Fire(this, NetworkMissHeartBeatEventArgs.Create(sender, missCount));
    }
}
