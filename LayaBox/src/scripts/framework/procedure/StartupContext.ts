import type IFsm from "../../../gameframex/fsm/IFsm";
import type IProcedureManager from "../../../gameframex/procedure/IProcedureManager";
import type StartupOptions from "../startup/StartupOptions";
import type IStartupUIHandler from "../startup/IStartupUIHandler";
import type IHotfixLauncher from "../startup/IHotfixLauncher";
import type IStartupHttpParamsProvider from "../startup/IStartupHttpParamsProvider";
import type { StartupHttpClient } from "../startup/StartupHttp";
import { FetchStartupHttpClient } from "../startup/StartupHttp";
import { GlobalConfigState } from "../startup/GlobalConfigState";
import ResourceVersionSystem, { type ResourceIO, type FileTable } from "../startup/ResourceVersionSystem";
import { BlackBoardKeys } from "../startup/BlackBoardKeys";

/** 启动流程 BlackBoard 数据键(资源体系,Runner 注入)。 */
export class StartupResourceKeys {
    public static readonly GlobalConfig: string = '__startup_global_config__';
    public static readonly HttpClient: string = '__startup_http_client__';
    public static readonly ResourceSystem: string = '__startup_resource_system__';
    public static readonly RemoteRoot: string = '__startup_remote_root__';
    public static readonly Version: string = '__startup_version__';
    public static readonly FileTable: string = '__startup_file_table__';
    public static readonly BuiltinTable: string = '__startup_builtin_table__';
}

/**
 * 启动流程上下文:从 FSM BlackBoard 取注入对象(对照 Unity 每流程 GetData 模式)。
 */
export default class StartupContext {
    public constructor(private readonly fsm: IFsm<IProcedureManager>) { }

    /** 启动配置。 */
    public get options(): StartupOptions {
        return this.fsm.GetData<StartupOptions>(BlackBoardKeys.StartupOptions)!;
    }

    /** 启动界面处理器。 */
    public get uiHandler(): IStartupUIHandler | null {
        return this.fsm.GetData<IStartupUIHandler>(BlackBoardKeys.StartupUIHandler);
    }

    /** 热更启动器。 */
    public get hotfixLauncher(): IHotfixLauncher | null {
        return this.fsm.GetData<IHotfixLauncher>(BlackBoardKeys.StartupHotfixLauncher);
    }

    /** HTTP 参数提供者。 */
    public get httpParamsProvider(): IStartupHttpParamsProvider {
        return this.fsm.GetData<IStartupHttpParamsProvider>(BlackBoardKeys.StartupHttpParamsProvider)!;
    }

    /** 全局配置状态(Runner 注入,跨流程共享)。 */
    public get globalConfig(): GlobalConfigState {
        return this.fsm.GetData<GlobalConfigState>(StartupResourceKeys.GlobalConfig)!;
    }

    /** HTTP 客户端(默认 fetch 实现,可注入)。 */
    public get httpClient(): StartupHttpClient {
        return this.fsm.GetData<StartupHttpClient>(StartupResourceKeys.HttpClient) ?? new FetchStartupHttpClient();
    }

    /** 资源版本系统。 */
    public get resourceSystem(): ResourceVersionSystem | null {
        return this.fsm.GetData<ResourceVersionSystem>(StartupResourceKeys.ResourceSystem);
    }

    /** 远端资源根地址(空 = 本地模式)。 */
    public get remoteRoot(): string {
        return this.fsm.GetData<string>(StartupResourceKeys.RemoteRoot) ?? '';
    }

    public set remoteRoot(value: string) {
        this.fsm.SetData(StartupResourceKeys.RemoteRoot, value);
    }

    /** 当前资源版本号。 */
    public get version(): string {
        return this.fsm.GetData<string>(StartupResourceKeys.Version) ?? '';
    }

    public set version(value: string) {
        this.fsm.SetData(StartupResourceKeys.Version, value);
    }

    /** 远端文件清单。 */
    public get fileTable(): FileTable | null {
        return this.fsm.GetData<FileTable>(StartupResourceKeys.FileTable);
    }

    public set fileTable(value: FileTable) {
        this.fsm.SetData(StartupResourceKeys.FileTable, value);
    }

    /** 首包内置清单。 */
    public get builtinTable(): FileTable | null {
        return this.fsm.GetData<FileTable>(StartupResourceKeys.BuiltinTable);
    }

    public set builtinTable(value: FileTable) {
        this.fsm.SetData(StartupResourceKeys.BuiltinTable, value);
    }


}
