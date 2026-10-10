import { User } from "../../src/gameframex/protobuf/_0300_User";

/**
 * 账号管理器。
 *
 * 对照 Unity `Hotfix.Manager.AccountManager`:纯状态单例,承载登录后的角色列表。
 */
export default class AccountManager {
    private static _instance: AccountManager | null = null;

    /** 全局单例。 */
    public static get Instance(): AccountManager {
        this._instance ??= new AccountManager();
        return this._instance;
    }

    /** 当前账号的角色列表。 */
    public PlayerList: User.PlayerInfo[] = [];
}
