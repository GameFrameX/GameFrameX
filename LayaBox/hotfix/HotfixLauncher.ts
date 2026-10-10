import GameApp from "../src/gameframex/GameApp";
import LogHelper from "../src/gameframex/log/LogHelper";
import UILogin from "./UI/Logic/UILogin";
import { TablesComponent } from "./Config/Generate/schema";
import type { LocalizationRow } from "../src/gameframex/localization/LocalizationComponent";
import type { SoundsConfigRow } from "../src/gameframex/sound/SoundComponent";
import type StartupOptions from "../src/scripts/framework/startup/StartupOptions";

/** 首包配置文件清单(构建产物,与 assets/resources/config 一致)。 */
const CONFIG_FILES = ['tables_tbachievementconfig', 'tables_tbsoundsconfig', 'tables_tbitemconfig', 'local_tblocalization'];

/**
 * 热更层启动器。
 *
 * 对照 Unity `HotfixLauncher.Main()` 三步:协议注册 → LoadConfig → LoadUI。
 * 差异:TS 无反射程序集,协议注册由 GameApp.initialize 的 proto 加载承担;
 * 本类的 startupAsync 由启动链 ProcedureGameLauncherState 调起。
 */
export default class HotfixLauncher {
    /**
     * 热更入口(对照 Unity Main)。
     */
    public static Main(): void {
        LogHelper.log('HotfixLauncher main');
        this.LoadConfig();
        this.LoadUI();
    }

    /**
     * 启动链入口(IHotfixLauncher.StartAsync)。
     * @param _options 启动配置
     * @returns 失败信息;null 表示成功
     */
    public static async startupAsync(_options: StartupOptions): Promise<string | null> {
        try {
            this.Main();
            return null;
        } catch (error) {
            return error instanceof Error ? error.message : String(error);
        }
    }

    private static LoadConfig(): void {
        void this.loadConfigAsync();
    }

    private static async loadConfigAsync(): Promise<void> {
        // 宿主加载器:浏览器 Laya.loader;node 测试由 GameApp.Config 测试直接灌注(此处 fetch 兜底)
        await GameApp.Config.loadAsync(CONFIG_FILES, async (file) => {
            const response = await fetch(`resources/config/${file}.json`);
            if (!response.ok) {
                throw new Error(`load config ${file} failed: HTTP ${response.status}`);
            }

            return await response.json();
        });

        const tables = new TablesComponent((file: string) => GameApp.Config.getLoaded(file));
        GameApp.Config.bindTables(tables);

        // 本地化灌注
        GameApp.Localization.Load(tables.TbLocalization.getDataList() as unknown as LocalizationRow[]);

        // 音效配置灌注(引擎播放注入在 Main.ts)
        GameApp.Sound.Load(tables.TbSoundsConfig.getDataList() as unknown as SoundsConfigRow[]);
    }

    private static LoadUI(): void {
        void this.loadUIAsync();
    }

    private static async loadUIAsync(): Promise<void> {
        await GameApp.UI.OpenAsync(UILogin, { packageName: UILogin.UIPackageName, resName: UILogin.UIResName });
        const sounds = GameApp.Config.GetConfig((tables) => (tables as TablesComponent).TbSoundsConfig);
        LogHelper.log(`sounds loaded: ${sounds.getDataList().length}`);
    }
}
