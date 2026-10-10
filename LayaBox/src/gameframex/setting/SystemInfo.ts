/**
 * 系统信息(GameApp.SystemInfo)。
 *
 * 对照 Unity `SystemInfoComponent` 常用面:设备唯一标识/平台/语言。
 * 差异:Web 无稳定设备 Id,以存储生成的随机 UUID 替代(对照 Unity deviceUniqueIdentifier 的可用语义)。
 */
export default class SystemInfo {
    private static cachedDeviceId: string | null = null;

    /** 设备唯一标识(首次访问生成并持久化)。 */
    public static get deviceUniqueIdentifier(): string {
        this.cachedDeviceId ??= this.loadOrCreateDeviceId();
        return this.cachedDeviceId;
    }

    /** 当前平台标识(对齐 Unity ApplicationHelper.PlatformName 语义)。 */
    public static get platform(): string {
        if (typeof wx !== 'undefined') {
            return 'wechat_minigame';
        }

        return 'web';
    }

    /** 浏览器语言。 */
    public static get language(): string {
        try {
            return navigator.language ?? 'zh-CN';
        } catch {
            return 'zh-CN';
        }
    }

    private static loadOrCreateDeviceId(): string {
        const storageKey = '__gfx_device_id__';
        try {
            const existing = localStorage.getItem(storageKey);
            if (existing != null && existing !== '') {
                return existing;
            }

            const generated = `${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 10)}`;
            localStorage.setItem(storageKey, generated);
            return generated;
        } catch {
            // 存储不可用:会话级随机值
            return `session-${Math.random().toString(36).slice(2, 10)}`;
        }
    }
}

/** 微信小游戏全局对象存在性探测(仅类型占位,运行时按 typeof 判断)。 */
declare const wx: unknown;
