import ProcedureBase from "../../../gameframex/procedure/ProcedureBase";
import type IFsm from "../../../gameframex/fsm/IFsm";
import type IProcedureManager from "../../../gameframex/procedure/IProcedureManager";
import StartupContext from "./StartupContext";
import StartupProcedureUtility from "./StartupProcedureUtility";
import ProcedureGetAppVersionInfoState from "./ProcedureGetAppVersionInfoState";
import ProcedurePatchInit from "./ProcedurePatchInit";

/**
 * 获取全局信息。
 *
 * 对照 Unity `ProcedureGetGlobalInfoState`:
 * SkipRemoteStartupRequests → PatchInit;否则多 URL 倒换拉全局信息,
 * 全失败回退缓存;无缓存则 CompleteFailure。
 */
export default class ProcedureGetGlobalInfoState extends ProcedureBase {
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
        const success = await context.globalConfig.fetchGlobalInfo(options, context.httpClient, params);
        if (success) {
            this.ChangeToState(fsm, ProcedureGetAppVersionInfoState);
            return;
        }

        if (context.globalConfig.tryApplyCachedGlobalInfo()) {
            this.ChangeToState(fsm, ProcedureGetAppVersionInfoState);
            return;
        }

        StartupProcedureUtility.CompleteFailure(fsm, 'ProcedureGetGlobalInfoState', options.GlobalInfoUrls[0] ?? '', 'Failed to get global info.');
    }
}
