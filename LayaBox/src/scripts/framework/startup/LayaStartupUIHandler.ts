import type IStartupUIHandler from "./IStartupUIHandler";
import type { StartupUpgradeInfo } from "./IStartupUIHandler";

/**
 * Laya 原生启动界面:纯代码进度条 + 文案(无资源依赖)。
 *
 * 说明:FairyGUIProject 的 UILauncher 包尚未经发布管线接入(见 notes Phase 5),
 * 启动期 UI 使用引擎原生绘制,fgui UILauncher 接入后替换本实现。
 */
export default class LayaStartupUIHandler implements IStartupUIHandler {
    private panel: Laya.Panel | null = null;
    private tip: Laya.Label | null = null;
    private progressBar: Laya.Sprite | null = null;
    private progressBg: Laya.Sprite | null = null;

    /**
     * 展示启动界面。
     * @param _launcherUIResName 启动界面资源名(原生实现忽略)
     */
    public async StartAsync(_launcherUIResName: string): Promise<void> {
        const panel = new Laya.Panel();
        panel.width = Laya.stage.width;
        panel.height = Laya.stage.height;
        panel.zOrder = 100000;

        const bg = new Laya.Sprite();
        bg.graphics.drawRect(0, 0, panel.width, panel.height, '#1a1a1a');
        panel.addChild(bg);

        const title = new Laya.Label();
        title.text = 'GameFrameX';
        title.color = '#ffffff';
        title.fontSize = 32;
        title.width = panel.width;
        title.align = 'center';
        title.y = panel.height * 0.4;
        panel.addChild(title);

        const barY = panel.height * 0.6;
        const barWidth = panel.width * 0.6;
        const barX = (panel.width - barWidth) / 2;
        this.progressBg = new Laya.Sprite();
        this.progressBg.graphics.drawRect(barX, barY, barWidth, 8, '#444444');
        panel.addChild(this.progressBg);

        this.progressBar = new Laya.Sprite();
        this.progressBar.graphics.drawRect(barX, barY, 0, 8, '#4caf50');
        panel.addChild(this.progressBar);

        this.tip = new Laya.Label();
        this.tip.color = '#aaaaaa';
        this.tip.fontSize = 18;
        this.tip.width = panel.width;
        this.tip.align = 'center';
        this.tip.y = barY + 20;
        panel.addChild(this.tip);

        Laya.stage.addChild(panel);
        this.panel = panel;
    }

    /**
     * 设置提示文案。
     * @param text 文案
     */
    public SetTipText(text: string): void {
        if (this.tip != null) {
            this.tip.text = text;
        }
    }

    /**
     * 设置进度(重绘进度条)。
     * @param progress 0~1
     * @param description 描述
     */
    public SetProgress(progress: number, description: string): void {
        if (this.progressBar == null || this.progressBg == null) {
            return;
        }

        const total = this.progressBg.width;
        this.progressBar.graphics.clear();
        this.progressBar.graphics.drawRect(this.progressBg.x, this.progressBg.y, total * Math.max(0, Math.min(1, progress)), 8, '#4caf50');
        this.SetTipText(description);
    }

    /**
     * 下载完成。
     */
    public SetProgressUpdateFinish(): void {
        this.SetTipText('');
    }

    /**
     * 升级弹窗(原生实现:控制台提示并同意;正式版替换为弹窗)。
     * @param info 升级信息
     */
    public async ShowUpgradeAsync(info: StartupUpgradeInfo): Promise<boolean> {
        console.warn(`[StartupUI] app upgrade required: ${info.UpdateTitle} -> ${info.AppDownloadUrl}`);
        return true;
    }

    /**
     * 关闭启动界面。
     */
    public Dispose(): void {
        this.panel?.removeSelf();
        this.panel = null;
        this.tip = null;
        this.progressBar = null;
        this.progressBg = null;
    }
}
