import IModule from "../IModule";

declare const fgui: {
    UIPackage: {
        addPackage(resKey: string, descData?: ArrayBuffer): unknown;
        loadPackage(resKey: string | string[], completeHandler: (pkgs: unknown[]) => void, progressHandler?: (progress: number) => void): void;
        removePackage(packageIdOrName: string): void;
        getByName(name: string): { name: string } | null;
        createObject(pkgName: string, resName: string): GObjectLike;
    };
};

/** fgui GObject 的最小结构(适配层内使用)。 */
interface GObjectLike {
    name: string;
    visible: boolean;
    sortingOrder: number;
    dispose(): void;
    center(restraint?: boolean): void;
    makeFullScreen(): void;
}

/** fgui GComponent 的最小结构。 */
interface GComponentLike extends GObjectLike {
    addChild(child: GObjectLike): GObjectLike;
    removeChild(child: GObjectLike, dispose?: boolean): GObjectLike;
    setChildIndex(child: GObjectLike, index: number): void;
    numChildren: number;
}

/**
 * FairyGUI 包管理。
 *
 * 对照 Unity `FairyGUIPackageComponent`:AddPackageAsync(并发去重)/Has/Get/RemovePackage。
 * resKey 为资源根路径(如 'fairygui/UILogin'),运行时实际加载 `<resKey>/<包名>_fui.bytes`。
 */
export default class FairyGUIPackageComponent implements IModule {
    private static _instance: FairyGUIPackageComponent | null = null;

    /** 全局单例(UI 域包管理)。 */
    public static get instance(): FairyGUIPackageComponent {
        this._instance ??= new FairyGUIPackageComponent();
        return this._instance;
    }

    private readonly packages: Map<string, Promise<void>> = new Map();

    /**
     * 是否已加载指定包。
     * @param name 包名
     */
    public Has(name: string): boolean {
        return fgui.UIPackage.getByName(name) != null;
    }

    /**
     * 异步加载包(并发调用共享同一 Promise)。
     * @param resKey 资源根路径
     */
    public AddPackageAsync(resKey: string): Promise<void> {
        if (this.packages.has(resKey)) {
            return this.packages.get(resKey)!;
        }

        const promise = new Promise<void>((resolve, reject) => {
            fgui.UIPackage.loadPackage(resKey, (pkgs) => {
                if (pkgs == null || pkgs.length === 0) {
                    reject(new Error(`load package failed: ${resKey}`));
                    return;
                }

                resolve();
            });
        });

        this.packages.set(resKey, promise);
        promise.catch(() => this.packages.delete(resKey));
        return promise;
    }

    /**
     * 移除包。
     * @param name 包名
     */
    public RemovePackage(name: string): void {
        fgui.UIPackage.removePackage(name);
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
     * 模块关闭:卸载全部包。
     */
    public shutdown(): void {
        for (const resKey of [...this.packages.keys()]) {
            this.packages.delete(resKey);
        }
    }
}

export type { GObjectLike, GComponentLike };
