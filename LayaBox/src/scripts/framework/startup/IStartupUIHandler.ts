import type StartupOptions from "./StartupOptions";

/** 升级提示信息(对照 Unity StartupUpgradeInfo)。 */
export interface StartupUpgradeInfo {
    IsForce: boolean;
    AppDownloadUrl: string;
    UpdateTitle: string;
    UpdateAnnouncement: string;
}

/**
 * 启动界面处理器。
 *
 * 对照 Unity `IStartupUIHandler`:启动 UI 的展示/进度/文案/升级弹窗。
 * Laya 实现挂在 UILauncher(FairyGUI);测试用控制台桩。
 */
export default interface IStartupUIHandler {
    /** 展示启动界面。 */
    StartAsync(launcherUIResName: string): Promise<void>;

    /** 设置提示文案。 */
    SetTipText(text: string): void;

    /** 设置下载进度(0~1)与描述。 */
    SetProgress(progress: number, description: string): void;

    /** 下载完成。 */
    SetProgressUpdateFinish(): void;

    /** 展示升级弹窗;返回 false 表示用户取消(流程挂住,对照 Unity 语义)。 */
    ShowUpgradeAsync(info: StartupUpgradeInfo): Promise<boolean>;

    /** 关闭启动界面。 */
    Dispose(): void;
}
