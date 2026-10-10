import IModule, { IPrioritizedModule } from "./IModule";

/**
 * 框架模块注册表与主循环驱动。
 *
 * 对照 Unity `GameFrameX.Runtime.GameFrameworkEntry`:
 * 模块按优先级轮询(大者先轮询、后关闭);TS 侧以 `registerModule(name, module)` 注册、
 * `getModule<T>(name)` 解析,由 Main.ts 每帧调用 `update`。
 */
export default class GameEntry {
    private static readonly _modules: Map<string, IModule> = new Map();
    private static _sortedModules: IPrioritizedModule[] = [];
    private static _initialized: boolean = false;

    /**
     * 注册框架模块。重复名称抛错;initialize 之后不再接受注册。
     * @param name 模块名(与 GameApp 门面域同名,如 "Fsm"/"Procedure"/"Network")
     * @param module 模块实例
     */
    public static registerModule<T extends IModule>(name: string, module: T): void {
        if (this._modules.has(name)) {
            throw new Error(`Module '${name}' is already registered.`);
        }

        if (this._initialized) {
            throw new Error(`GameEntry already initialized, can not register module '${name}'.`);
        }

        this._modules.set(name, module);
    }

    /**
     * 获取框架模块。
     * @param name 模块名
     */
    public static getModule<T extends IModule>(name: string): T {
        const module = this._modules.get(name);
        if (module == null) {
            throw new Error(`Module '${name}' is not registered.`);
        }

        return module as T;
    }

    /**
     * 尝试获取框架模块,未注册返回 null。
     * @param name 模块名
     */
    public static tryGetModule<T extends IModule>(name: string): T | null {
        return (this._modules.get(name) as T | undefined) ?? null;
    }

    /**
     * 是否已注册指定模块。
     * @param name 模块名
     */
    public static hasModule(name: string): boolean {
        return this._modules.has(name);
    }

    /**
     * 初始化全部已注册模块(按优先级降序),完成后进入轮询态。
     */
    public static async initialize(): Promise<void> {
        if (this._initialized) {
            throw new Error('GameEntry already initialized.');
        }

        this._sortedModules = [...this._modules.values()].map((m) => m as IPrioritizedModule);
        this._sortedModules.sort((a, b) => (b.priority ?? 0) - (a.priority ?? 0));
        for (const module of this._sortedModules) {
            await module.init();
        }

        this._initialized = true;
    }

    /**
     * 轮询全部模块(按优先级降序)。由 Main.ts 每帧调用。
     * @param elapseSeconds 逻辑流逝时间,以秒为单位
     * @param realElapseSeconds 真实流逝时间,以秒为单位
     */
    public static update(elapseSeconds: number, realElapseSeconds: number): void {
        if (!this._initialized) {
            return;
        }

        for (const module of this._sortedModules) {
            module.update(elapseSeconds, realElapseSeconds);
        }
    }

    /**
     * 关闭全部模块(与轮询相反顺序)并复位注册表。
     */
    public static shutdown(): void {
        for (const module of [...this._sortedModules].reverse()) {
            module.shutdown();
        }

        this._sortedModules = [];
        this._modules.clear();
        this._initialized = false;
    }
}
