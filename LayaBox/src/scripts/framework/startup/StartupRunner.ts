import StartupOptions from "./StartupOptions";
import StartupResult from "./StartupResult";
import type IStartupUIHandler from "./IStartupUIHandler";
import type IHotfixLauncher from "./IHotfixLauncher";
import type IStartupHttpParamsProvider from "./IStartupHttpParamsProvider";
import { DefaultStartupHttpParamsProvider } from "./IStartupHttpParamsProvider";
import type { StartupHttpClient } from "./StartupHttp";
import { GlobalConfigState } from "./GlobalConfigState";
import ResourceVersionSystem, { type ResourceIO, type FileTable } from "./ResourceVersionSystem";
import { BlackBoardKeys } from "./BlackBoardKeys";
import GameApp from "../../../gameframex/GameApp";
import ProcedureManager from "../../../gameframex/procedure/ProcedureManager";
import type IFsm from "../../../gameframex/fsm/IFsm";
import type IProcedureManager from "../../../gameframex/procedure/IProcedureManager";
import { StartupCompletionSource } from "../procedure/StartupProcedureUtility";
import { StartupResourceKeys } from "../procedure/StartupContext";
import ProcedureLauncherState from "../procedure/ProcedureLauncherState";
import ProcedureGetGlobalInfoState from "../procedure/ProcedureGetGlobalInfoState";
import ProcedureGetAppVersionInfoState from "../procedure/ProcedureGetAppVersionInfoState";
import ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState from "../procedure/ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState";
import ProcedurePatchInit from "../procedure/ProcedurePatchInit";
import ProcedureUpdateStaticVersion from "../procedure/ProcedureUpdateStaticVersion";
import ProcedureUpdateManifest from "../procedure/ProcedureUpdateManifest";
import ProcedureCreateDownloader from "../procedure/ProcedureCreateDownloader";
import ProcedureDownloadWebFiles from "../procedure/ProcedureDownloadWebFiles";
import ProcedurePatchDone from "../procedure/ProcedurePatchDone";
import ProcedureGameLauncherState from "../procedure/ProcedureGameLauncherState";

/** Runner 级注入(资源 IO / HTTP / 首包内置清单,宿主与测试可替换)。 */
export interface StartupRunnerEnvironment {
    resourceIO?: ResourceIO;
    httpClient?: StartupHttpClient;
    builtinTable?: FileTable;
}

/**
 * 启动流程驱动器。
 *
 * 对照 Unity `StartupRunner.Run`:同步校验 → 注册 11 个流程 → BlackBoard 注入 →
 * StartProcedure(LauncherState) → 返回结果 Promise(失败不抛异常,以 StartupResult 表达)。
 */
export default class StartupRunner {
    /**
     * 运行启动链。
     * @param options 启动配置
     * @param uiHandler 启动界面处理器
     * @param hotfixLauncher 热更启动器
     * @param httpParamsProvider HTTP 参数提供者(默认实现)
     * @param environment 环境注入(资源 IO/HTTP/内置清单)
     * @returns 启动结果
     */
    public static async Run(
        options: StartupOptions,
        uiHandler: IStartupUIHandler,
        hotfixLauncher: IHotfixLauncher,
        httpParamsProvider: IStartupHttpParamsProvider | null = null,
        environment: StartupRunnerEnvironment = {},
    ): Promise<StartupResult> {
        if (options == null || uiHandler == null || hotfixLauncher == null) {
            throw new Error('StartupRunner.Run: options/uiHandler/hotfixLauncher must not be null.');
        }

        if (!options.SkipRemoteStartupRequests && (options.GlobalInfoUrls == null || options.GlobalInfoUrls.length === 0)) {
            throw new Error('StartupRunner.Run: GlobalInfoUrls must contain at least one URL.');
        }

        GameApp.Procedure.Initialize(GameApp.Fsm, [
            new ProcedureLauncherState(),
            new ProcedureGetGlobalInfoState(),
            new ProcedureGetAppVersionInfoState(),
            new ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState(),
            new ProcedurePatchInit(),
            new ProcedureUpdateStaticVersion(),
            new ProcedureUpdateManifest(),
            new ProcedureCreateDownloader(),
            new ProcedureDownloadWebFiles(),
            new ProcedurePatchDone(),
            new ProcedureGameLauncherState(),
        ]);

        const procedureFsm = GameApp.Fsm.GetFsm<IProcedureManager>(ProcedureManager, 'Procedure') as IFsm<IProcedureManager> | null;
        if (procedureFsm == null) {
            throw new Error('StartupRunner.Run: procedure fsm not found after Initialize.');
        }

        const completion = new StartupCompletionSource();
        procedureFsm.SetData(BlackBoardKeys.StartupOptions, options);
        procedureFsm.SetData(BlackBoardKeys.StartupUIHandler, uiHandler);
        procedureFsm.SetData(BlackBoardKeys.StartupHotfixLauncher, hotfixLauncher);
        procedureFsm.SetData(BlackBoardKeys.StartupCompletionSource, completion);
        procedureFsm.SetData(BlackBoardKeys.StartupHttpParamsProvider, httpParamsProvider ?? new DefaultStartupHttpParamsProvider());
        procedureFsm.SetData(StartupResourceKeys.GlobalConfig, new GlobalConfigState());
        procedureFsm.SetData(StartupResourceKeys.HttpClient, environment.httpClient ?? null);
        procedureFsm.SetData(StartupResourceKeys.ResourceSystem, new ResourceVersionSystem(environment.resourceIO ?? defaultResourceIO()));
        procedureFsm.SetData(StartupResourceKeys.BuiltinTable, environment.builtinTable ?? { files: {} });
        procedureFsm.SetData(StartupResourceKeys.RemoteRoot, options.RemoteResourceRoot);

        GameApp.Procedure.StartProcedure(ProcedureLauncherState);
        return completion.wait();
    }
}

/** 默认资源 IO:fetch 文本;prefetch 为桩(浏览器宿主应覆盖为 Laya.loader 预热)。 */
function defaultResourceIO(): ResourceIO {
    return {
        fetchText: async (url: string) => {
            const response = await fetch(url);
            if (!response.ok) {
                throw new Error(`fetch ${url} failed: HTTP ${response.status}`);
            }

            return await response.text();
        },
        prefetch: async (_url: string) => void 0,
    };
}
