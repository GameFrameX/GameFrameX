import ProcedureBase from "../../../gameframex/procedure/ProcedureBase";
import type IFsm from "../../../gameframex/fsm/IFsm";
import type IProcedureManager from "../../../gameframex/procedure/IProcedureManager";
import StartupContext from "./StartupContext";
import StartupProcedureUtility from "./StartupProcedureUtility";
import ProcedurePatchInit from "./ProcedurePatchInit";

/**
 * 获取默认资源包版本信息。
 *
 * 对照 Unity `ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState`:
 * 拉资产包版本与包地址,写入 BlackBoard 供 PatchInit 使用;失败 CompleteFailure。
 */
export default class ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState extends ProcedureBase {
    /**
     * 进入状态。
     * @param fsm 流程 FSM
     */
    public OnEnter(fsm: IFsm<IProcedureManager>): void {
        void this.enterAsync(fsm);
    }

    private async enterAsync(fsm: IFsm<IProcedureManager>): Promise<void> {
        const context = new StartupContext(fsm);
        const options = context.options;
        if (options.SkipRemoteStartupRequests) {
            this.ChangeToState(fsm, ProcedurePatchInit);
            return;
        }

        const params = context.httpParamsProvider.Create(options);
        params['AssetPackageName'] = 'resource';
        const packageVersion = await context.globalConfig.fetchAssetPackageVersion(options, context.httpClient, params);
        if (packageVersion == null) {
            StartupProcedureUtility.CompleteFailure(fsm, 'ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState', context.globalConfig.CheckResourceVersionUrl, 'Failed to get asset package version info.');
            return;
        }

        context.remoteRoot = packageVersion.AssetPackagePath !== '' ? packageVersion.AssetPackagePath : `${options.RemoteResourceRoot}/${packageVersion.Version}`;
        context.version = packageVersion.Version;
        this.ChangeToState(fsm, ProcedurePatchInit);
    }
}
