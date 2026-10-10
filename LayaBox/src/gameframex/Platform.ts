/**
 * 平台编译宏(与 Unity `ENABLE_WECHAT_MINI_GAME` / Godot 对齐)。
 *
 * TS 无预处理器宏,以运行时宿主探测承载:Laya 发布到微信小游戏时存在全局 wx 对象。
 * 业务侧用法与 Unity 宏一致:`if (Platform.ENABLE_WECHAT_MINI_GAME) { ... }`。
 */
export default class Platform {
    /** 是否微信小游戏环境(ENABLE_WECHAT_MINI_GAME 等价)。 */
    public static get ENABLE_WECHAT_MINI_GAME(): boolean {
        return typeof wx !== 'undefined';
    }

    /** 是否浏览器环境。 */
    public static get IS_WEB(): boolean {
        return typeof window !== 'undefined' && typeof wx === 'undefined';
    }
}

declare const wx: unknown;
