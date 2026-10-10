import { User } from "../../src/gameframex/protobuf/_0300_User";

/**
 * 玩家管理器。
 *
 * 对照 Unity `Hotfix.Manager.PlayerManager`:当前登录角色信息。
 */
export default class PlayerManager {
    private static _instance: PlayerManager | null = null;

    /** 全局单例。 */
    public static get Instance(): PlayerManager {
        this._instance ??= new PlayerManager();
        return this._instance;
    }

    /** 当前角色信息。 */
    public PlayerInfo: User.PlayerInfo = new User.PlayerInfo();
}
