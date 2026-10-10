import ProcedureBase from "../../../gameframex/procedure/ProcedureBase";
import type IFsm from "../../../gameframex/fsm/IFsm";
import type IProcedureManager from "../../../gameframex/procedure/IProcedureManager";
import StartupContext from "./StartupContext";
import StartupProcedureUtility from "./StartupProcedureUtility";
import ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState from "./ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState";
import ProcedurePatchInit from "./ProcedurePatchInit";

/**
 * 获取 App 版本信息。
 *
 * 对照 Unity `ProcedureGetAppVersionInfoState`:
 * 需升级时展示升级弹窗(用户取消则流程挂住);无升级进入资产包版本流程。
 */
export default class ProcedureGetAppVersionInfoState extends ProcedureBase {
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
        const appVersion = await context.globalConfig.fetchAppVersion(options, context.httpClient, params);
        if (appVersion == null) {
            StartupProcedureUtility.CompleteFailure(fsm, 'ProcedureGetAppVersionInfoState', context.globalConfig.CheckAppVersionUrl, 'Failed to get app version info.');
            return;
        }

        if (appVersion.IsUpgrade) {
            const uiHandler = context.uiHandler;
            if (uiHandler == null) {
                StartupProcedureUtility.CompleteFailure(fsm, 'ProcedureGetAppVersionInfoState', '', 'upgrade required but no ui handler');
                return;
            }

            const accepted = await uiHandler.ShowUpgradeAsync({
                IsForce: appVersion.IsForce,
                AppDownloadUrl: appVersion.AppDownloadUrl,
                UpdateTitle: appVersion.UpdateTitle,
                UpdateAnnouncement: appVersion.UpdateAnnouncement,
            });
            if (!accepted) {
                // 对照 Unity:用户取消升级,流程挂住不切状态
                return;
            }
        }

        this.ChangeToState(fsm, ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState);
    }
}
