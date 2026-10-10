import type IHotfixLauncher from "./IHotfixLauncher";
import type StartupOptions from "./StartupOptions";
import HotfixLauncher from "../../../../hotfix/HotfixLauncher";

/**
 * 热更启动桥:启动层 → hotfix/HotfixLauncher。
 *
 * 对照 Unity `ApplicationHotfixLauncher` → HotfixHelper(反射调入热更程序集);
 * TS 无程序集边界,直接静态调用。
 */
export default class HotfixLauncherBridge implements IHotfixLauncher {
    /**
     * 启动热更层。
     * @param options 启动配置
     * @returns 失败信息;null 表示成功
     */
    public StartAsync(options: StartupOptions): Promise<string | null> {
        return HotfixLauncher.startupAsync(options);
    }
}
