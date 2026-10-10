import NetworkManager from "../network/NetworkManager";
import GameEntry from "../GameEntry";

/**
 * Network 域注册:创建 NetworkManager 并注册进 GameEntry(驱动全部通道轮询)。
 * 通道由业务层经 GameApp.Network.CreateNetworkChannel(new NetworkChannel(...)) 创建。
 * @returns 网络管理器实例(GameApp.Network)
 */
export default function NetworkDomain(): NetworkManager {
    const manager = new NetworkManager();
    GameEntry.registerModule('Network', manager);
    return manager;
}
