import IModule from "../IModule";

/**
 * 配置组件(GameApp.Config)。
 *
 * 对照 Unity `ConfigComponent` + 生成物 `TablesComponent(loader)`:
 * loadAsync 预取全部表 JSON 入缓存 → bindTables 以同步 loader 构造生成 TablesComponent;
 * GetConfig 经选择子取表(TS 无类型反射,与 Unity GetConfig<T>() 的必要差异)。
 */
export default class ConfigComponent implements IModule {
    private readonly cache: Map<string, unknown> = new Map();
    private tables: unknown = null;

    /**
     * 表实例(生成 TablesComponent;bindTables 前 null)。
     */
    public get Tables(): unknown {
        return this.tables;
    }

    /**
     * 预取表数据文件。
     * @param files 文件名列表(无扩展名,如 'tables_tbitemconfig')
     * @param fetch 宿主取数(Laya.loader / node fs / 测试桩)
     */
    public async loadAsync(files: string[], fetch: (file: string) => Promise<unknown>): Promise<void> {
        for (const file of files) {
            this.cache.set(file, await fetch(file));
        }
    }

    /**
     * 同步取已加载的表数据(生成 TablesComponent 构造 loader 用)。
     * @param file 文件名
     */
    public getLoaded(file: string): unknown {
        const data = this.cache.get(file);
        if (data == null) {
            throw new Error(`config '${file}' not loaded`);
        }

        return data;
    }

    /**
     * 绑定生成的表实例(构造完成的 TablesComponent)。
     * @param tables 表实例
     */
    public bindTables(tables: unknown): void {
        this.tables = tables;
    }

    /**
     * 取表:选择子式访问(如 GetConfig(t => t.TbItemConfig))。
     * @param selector 表选择子
     */
    public GetConfig<T>(selector: (tables: never) => T): T {
        if (this.tables == null) {
            throw new Error('tables not bound (call bindTables first)');
        }

        return selector(this.tables as never);
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
     * 模块关闭:清空缓存与表。
     */
    public shutdown(): void {
        this.cache.clear();
        this.tables = null;
    }
}
