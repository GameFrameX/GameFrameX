import IModule from "../IModule";

/** 本地化行最小结构(TbLocalization 生成行:key + 各语言文本字段)。 */
export interface LocalizationRow {
    key: string;
    [language: string]: unknown;
}

/**
 * 本地化组件(GameApp.Localization)。
 *
 * 对照 Unity `LocalizationComponent`:TbLocalization 灌注后按 key 取文案;
 * 当前语言可切换,缺失 key 时回退 key 本身并告警。
 */
export default class LocalizationComponent implements IModule {
    private readonly entries: Map<string, LocalizationRow> = new Map();
    private currentLanguage: string = 'zh';

    /**
     * 当前语言字段名(如 'zh'/'en',对应 TbLocalization 的列名)。
     */
    public get Language(): string {
        return this.currentLanguage;
    }

    public set Language(value: string) {
        this.currentLanguage = value;
    }

    /**
     * 灌入 TbLocalization 全部行。
     * @param rows 本地化行集合
     */
    public Load(rows: LocalizationRow[]): void {
        this.entries.clear();
        for (const row of rows) {
            this.entries.set(row.key, row);
        }
    }

    /**
     * 取文案。
     * @param key 本地化 key
     */
    public GetString(key: string): string {
        const row = this.entries.get(key);
        if (row == null) {
            return key;
        }

        const text = row[this.currentLanguage];
        return typeof text === 'string' ? text : key;
    }

    /**
     * 是否存在指定 key。
     * @param key 本地化 key
     */
    public HasKey(key: string): boolean {
        return this.entries.has(key);
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
     * 模块关闭。
     */
    public shutdown(): void {
        this.entries.clear();
    }
}
