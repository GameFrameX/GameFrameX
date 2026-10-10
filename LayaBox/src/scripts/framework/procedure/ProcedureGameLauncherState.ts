import ProcedureBase from "../../../gameframex/procedure/ProcedureBase";
import type IFsm from "../../../gameframex/fsm/IFsm";
import type IProcedureManager from "../../../gameframex/procedure/IProcedureManager";
import StartupContext from "./StartupContext";
import StartupProcedureUtility from "./StartupProcedureUtility";

/**
 * 游戏启动:调起热更层入口。
 *
 * 对照 Unity `ProcedureGameLauncherState`:hotfixLauncher.StartAsync(options) →
 * uiHandler.Dispose() → 成功 CompleteSuccess / 失败 CompleteFailure。
 */
export default class ProcedureGameLauncherState extends ProcedureBase {
    /**
     * 进入状态。
     * @param fsm 流程 FSM
     */
    public OnEnter(fsm: IFsm<IProcedureManager>): void {
        void this.enterAsync(fsm);
    }

    private async enterAsync(fsm: IFsm<IProcedureManager>): Promise<void> {
        const context = new StartupContext(fsm);
        const hotfixLauncher = context.hotfixLauncher;
        if (hotfixLauncher == null) {
            StartupProcedureUtility.CompleteFailure(fsm, 'ProcedureGameLauncherState', '', 'hotfix launcher missing');
            return;
        }

        const errorMessage = await hotfixLauncher.StartAsync(context.options);
        context.uiHandler?.Dispose();
        if (errorMessage == null) {
            StartupProcedureUtility.CompleteSuccess(fsm);
        } else {
            StartupProcedureUtility.CompleteFailure(fsm, 'ProcedureGameLauncherState', '', errorMessage);
        }
    }
}
