import IModule from "../IModule";

/**
 * 本地设置存取(GameApp.Setting)。
 *
 * 对照 Unity `SettingComponent`:键值持久化;浏览器/小游戏走 localStorage,
 * 键缺失/存储不可用静默降级(读取返回默认值)。
 */
export default class SettingComponent implements IModule {
    /**
     * 读取字符串设置。
     * @param key 键
     * @param defaultValue 默认值
     */
    public GetString(key: string, defaultValue: string = ''): string {
        try {
            return localStorage.getItem(key) ?? defaultValue;
        } catch {
            return defaultValue;
        }
    }

    /**
     * 写入字符串设置。
     * @param key 键
     * @param value 值
     */
    public SetString(key: string, value: string): void {
        try {
            localStorage.setItem(key, value);
        } catch {
            // 存储不可用(隐私模式/小游戏沙箱)静默跳过
        }
    }

    /**
     * 读取整数设置。
     * @param key 键
     * @param defaultValue 默认值
     */
    public GetInt(key: string, defaultValue: number = 0): number {
        const raw = this.GetString(key, '');
        const parsed = Number.parseInt(raw, 10);
        return Number.isNaN(parsed) ? defaultValue : parsed;
    }

    /**
     * 写入整数设置。
     * @param key 键
     * @param value 值
     */
    public SetInt(key: string, value: number): void {
        this.SetString(key, String(value));
    }

    /**
     * 读取布尔设置。
     * @param key 键
     * @param defaultValue 默认值
     */
    public GetBool(key: string, defaultValue: boolean = false): boolean {
        const raw = this.GetString(key, '');
        if (raw === '1') return true;
        if (raw === '0') return false;
        return defaultValue;
    }

    /**
     * 写入布尔设置。
     * @param key 键
     * @param value 值
     */
    public SetBool(key: string, value: boolean): void {
        this.SetString(key, value ? '1' : '0');
    }

    /**
     * 删除指定键。
     * @param key 键
     */
    public Remove(key: string): void {
        try {
            localStorage.removeItem(key);
        } catch {
            // 同上
        }
    }

    /**
     * 模块初始化(由 GameEntry 调用)。
     */
    public async init(): Promise<void> { }

    /**
     * 模块轮询(无逐帧动作)。
     */
    public update(_elapseSeconds: number, _realElapseSeconds: number): void { }

    /**
     * 模块关闭(持久化由 localStorage 即时完成)。
     */
    public shutdown(): void { }
}
