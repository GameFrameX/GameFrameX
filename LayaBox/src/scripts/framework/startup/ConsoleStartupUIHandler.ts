import type IStartupUIHandler from "./IStartupUIHandler";
import type { StartupUpgradeInfo } from "./IStartupUIHandler";

/**
 * 控制台启动界面处理器(node 测试/无 UI 宿主)。
 */
export default class ConsoleStartupUIHandler implements IStartupUIHandler {
    public async StartAsync(_launcherUIResName: string): Promise<void> { }
    public SetTipText(text: string): void {
        console.log(`[StartupUI] ${text}`);
    }
    public SetProgress(progress: number, description: string): void {
        console.log(`[StartupUI] ${(progress * 100).toFixed(0)}% ${description}`);
    }
    public SetProgressUpdateFinish(): void { }
    public async ShowUpgradeAsync(info: StartupUpgradeInfo): Promise<boolean> {
        console.log(`[StartupUI] upgrade: ${info.UpdateTitle} ${info.UpdateAnnouncement}`);
        return true;
    }
    public Dispose(): void { }
}
