import GameApp from "../../../gameframex/GameApp";
import LogHelper from "../../../gameframex/log/LogHelper";
import StartupOptions from "./StartupOptions";
import StartupRunner from "./StartupRunner";
import LayaStartupUIHandler from "./LayaStartupUIHandler";
import HotfixLauncherBridge from "./HotfixLauncherBridge";

/**
 * 应用启动入口。
 *
 * 对照 Unity `ApplicationStartupEntry`(MonoBehaviour):
 * Main.ts onStart → GameApp.initialize → 本类 run → StartupRunner.Run;
 * 差异:无 HybridCLR 热更加载(TS 无 AOT 边界),热更入口直接桥接 hotfix/HotfixLauncher。
 */
export default class ApplicationStartupEntry {
    /**
     * 运行启动链(默认本地开发配置:跳过远程请求、首包内置资源)。
     */
    public static async run(): Promise<void> {
        await GameApp.initialize();

        const options = new StartupOptions();
        options.Channel = 'laya';
        options.SubChannel = 'default';
        options.PackageName = 'com.gameframex.layabox';
        options.AppVersion = '1.0.0';
        // 本地开发:跳过三个远程启动请求,首包内置资源直接进游戏
        options.SkipRemoteStartupRequests = true;
        options.RemoteResourceRoot = '';
        // 远程模式示例(联调服务器上线后启用):
        // options.GlobalInfoUrls = ['http://127.0.0.1:38080/game/api/global_info'];
        // options.RemoteResourceRoot = 'http://127.0.0.1:38080/resources';

        const result = await StartupRunner.Run(options, new LayaStartupUIHandler(), new HotfixLauncherBridge());
        if (!result.Success) {
            LogHelper.error(`Startup failed. Procedure: ${result.FailedProcedureName}, Url: ${result.FailedUrl}, Error: ${result.ErrorMessage}`);
        }
    }
}
