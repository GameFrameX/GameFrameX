/**
 * 框架模块接口。
 *
 * 对照 Unity `GameFrameX.Runtime.GameFrameworkModule` 的 Update/Shutdown 生命周期,
 * 增加 async init 以承载需要异步就绪的模块(网络/配置/资源)。
 * 注册进 GameEntry 后:启动时依优先级调用 init,随后每帧调用 update,关闭时逆序调用 shutdown。
 */
export default interface IModule {
    /** 模块初始化,在所有模块 update 之前完成。 */
    init(): Promise<void> | void;

    /** 模块轮询,每帧调用。 */
    update(elapseSeconds: number, realElapseSeconds: number): void;

    /** 模块关闭,清理资源。 */
    shutdown(): void;
}

/**
 * 带优先级的模块:数值越大越先轮询、越后关闭(与 Unity GameFrameworkModule.Priority 语义一致)。
 */
export interface IPrioritizedModule extends IModule {
    /** 轮询优先级,默认 0。 */
    readonly priority?: number;
}
