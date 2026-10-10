import GameEventArgs from "./GameEventArgs";
import type INetworkChannel from "../network/INetworkChannel";

/**
 * 网络连接建立事件参数。
 *
 * 对照 Unity `GameFrameX.Network.Runtime.NetworkConnectedEventArgs`。
 */
export class NetworkConnectedEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'NetworkConnectedEventArgs';

    public static Create(networkChannel: INetworkChannel): NetworkConnectedEventArgs {
        const args = new NetworkConnectedEventArgs();
        args.NetworkChannel = networkChannel;
        return args;
    }

    public get Id(): string {
        return NetworkConnectedEventArgs.EventId;
    }

    /** 建立连接的网络通道。 */
    public NetworkChannel!: INetworkChannel;
}

/**
 * 网络重连成功事件参数(重登钩子入口:业务订阅后在重连时重新走登录)。
 */
export class NetworkReconnectedEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'NetworkReconnectedEventArgs';

    public static Create(networkChannel: INetworkChannel): NetworkReconnectedEventArgs {
        const args = new NetworkReconnectedEventArgs();
        args.NetworkChannel = networkChannel;
        return args;
    }

    public get Id(): string {
        return NetworkReconnectedEventArgs.EventId;
    }

    /** 重连成功的网络通道。 */
    public NetworkChannel!: INetworkChannel;
}

/**
 * 网络心跳丢失事件参数。
 *
 * 对照 Unity `GameFrameX.Network.Runtime.NetworkMissHeartBeatEventArgs`:
 * 心跳超时丢失时由网络通道发布,业务可订阅后决定重连或提示。
 */
export class NetworkMissHeartBeatEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'NetworkMissHeartBeatEventArgs';

    public static Create(networkChannel: INetworkChannel, missCount: number): NetworkMissHeartBeatEventArgs {
        const args = new NetworkMissHeartBeatEventArgs();
        args.NetworkChannel = networkChannel;
        args.MissCount = missCount;
        return args;
    }

    public get Id(): string {
        return NetworkMissHeartBeatEventArgs.EventId;
    }

    /** 丢失心跳的网络通道。 */
    public NetworkChannel!: INetworkChannel;

    /** 连续丢失的心跳计数。 */
    public MissCount!: number;
}

/**
 * 网络连接关闭事件参数。
 *
 * 对照 Unity `GameFrameX.Network.Runtime.NetworkClosedEventArgs`。
 */
export class NetworkClosedEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'NetworkClosedEventArgs';

    public static Create(networkChannel: INetworkChannel): NetworkClosedEventArgs {
        const args = new NetworkClosedEventArgs();
        args.NetworkChannel = networkChannel;
        return args;
    }

    public get Id(): string {
        return NetworkClosedEventArgs.EventId;
    }

    /** 关闭的网络通道。 */
    public NetworkChannel!: INetworkChannel;
}

/**
 * 网络错误事件参数。
 *
 * 对照 Unity `GameFrameX.Network.Runtime.NetworkErrorEventArgs`。
 */
export class NetworkErrorEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'NetworkErrorEventArgs';

    public static Create(networkChannel: INetworkChannel, errorMessage: string): NetworkErrorEventArgs {
        const args = new NetworkErrorEventArgs();
        args.NetworkChannel = networkChannel;
        args.ErrorMessage = errorMessage;
        return args;
    }

    public get Id(): string {
        return NetworkErrorEventArgs.EventId;
    }

    /** 出错的网络通道。 */
    public NetworkChannel!: INetworkChannel;

    /** 错误信息。 */
    public ErrorMessage!: string;
}
