import ProcedureBase from "../../../gameframex/procedure/ProcedureBase";
import type IFsm from "../../../gameframex/fsm/IFsm";
import type IProcedureManager from "../../../gameframex/procedure/IProcedureManager";
import EventSystems from "../../../gameframex/event/EventSystems";
import { AssetPatchStatesChangeEventArgs, EPatchStates } from "../startup/AssetEventArgs";
import StartupContext from "./StartupContext";
import ProcedureGameLauncherState from "./ProcedureGameLauncherState";

/**
 * 补丁完成。
 *
 * 对照 Unity `ProcedurePatchDone`:Fire PatchDone;UI 收尾;进入游戏启动。
 */
export default class ProcedurePatchDone extends ProcedureBase {
    /**
     * 进入状态。
     * @param fsm 流程 FSM
     */
    public OnEnter(fsm: IFsm<IProcedureManager>): void {
        EventSystems.FireNow(this, AssetPatchStatesChangeEventArgs.Create(EPatchStates.PatchDone));
        const uiHandler = new StartupContext(fsm).uiHandler;
        uiHandler?.SetProgressUpdateFinish();
        uiHandler?.SetTipText('');
        this.ChangeToState(fsm, ProcedureGameLauncherState);
    }
}
