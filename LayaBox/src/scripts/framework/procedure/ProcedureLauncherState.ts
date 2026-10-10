import ProcedureBase from "../../../gameframex/procedure/ProcedureBase";
import type IFsm from "../../../gameframex/fsm/IFsm";
import type IProcedureManager from "../../../gameframex/procedure/IProcedureManager";
import StartupContext from "./StartupContext";
import ProcedureGetGlobalInfoState from "./ProcedureGetGlobalInfoState";

/**
 * 启动器状态:展示启动界面后进入全局信息流程。
 *
 * 对照 Unity `ProcedureLauncherState`:uiHandler.StartAsync(LauncherUIResName) → GetGlobalInfoState。
 */
export default class ProcedureLauncherState extends ProcedureBase {
    /**
     * 进入状态。
     * @param fsm 流程 FSM
     */
    public OnEnter(fsm: IFsm<IProcedureManager>): void {
        void this.enterAsync(fsm);
    }

    private async enterAsync(fsm: IFsm<IProcedureManager>): Promise<void> {
        const context = new StartupContext(fsm);
        const uiHandler = context.uiHandler;
        if (uiHandler == null) {
            // 对照 Unity:无界面处理器时流程挂住(无失败路径)
            return;
        }

        await uiHandler.StartAsync(context.options.LauncherUIResName);
        this.ChangeToState(fsm, ProcedureGetGlobalInfoState);
    }
}
