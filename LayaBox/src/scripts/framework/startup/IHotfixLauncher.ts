import type StartupOptions from "./StartupOptions";

/**
 * 热更层启动器。
 *
 * 对照 Unity `IHotfixLauncher`(ApplicationHotfixLauncher → HotfixHelper 反射调入热更程序集):
 * TS 无 AOT/热更边界,实现直接调 hotfix 层 HotfixLauncher.startupAsync。
 */
export default interface IHotfixLauncher {
    /** 启动热更层业务;失败返回错误信息(null 表示成功)。 */
    StartAsync(options: StartupOptions): Promise<string | null>;
}
