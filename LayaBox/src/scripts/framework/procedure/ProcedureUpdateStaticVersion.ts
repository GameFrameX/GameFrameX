import ProcedureBase from "../../../gameframex/procedure/ProcedureBase";
import type IFsm from "../../../gameframex/fsm/IFsm";
import type IProcedureManager from "../../../gameframex/procedure/IProcedureManager";
import EventSystems from "../../../gameframex/event/EventSystems";
import { AssetPatchStatesChangeEventArgs, AssetStaticVersionUpdateFailedEventArgs, EPatchStates } from "../startup/AssetEventArgs";
import StartupContext from "./StartupContext";
import ProcedureUpdateManifest from "./ProcedureUpdateManifest";

/**
 * 更新静态版本。
 *
 * 对照 Unity `ProcedureUpdateStaticVersion`:Fire 阶段事件;请求远端版本号;
 * 失败 Fire 失败事件并自重试(Unity 基线无上限)。本地模式直接进入清单流程。
 */
export default class ProcedureUpdateStaticVersion extends ProcedureBase {
    /**
     * 进入状态。
     * @param fsm 流程 FSM
     */
    public OnEnter(fsm: IFsm<IProcedureManager>): void {
        void this.enterAsync(fsm);
    }

    private async enterAsync(fsm: IFsm<IProcedureManager>): Promise<void> {
        const context = new StartupContext(fsm);
        EventSystems.FireNow(this, AssetPatchStatesChangeEventArgs.Create(EPatchStates.UpdateStaticVersion));

        if (context.remoteRoot === '') {
            // 本地模式:无远端版本
            this.ChangeToState(fsm, ProcedureUpdateManifest);
            return;
        }

        const system = context.resourceSystem!;
        try {
            context.version = await system.requestVersion(context.remoteRoot);
            this.ChangeToState(fsm, ProcedureUpdateManifest);
        } catch (error) {
            const message = error instanceof Error ? error.message : String(error);
            EventSystems.FireNow(this, new AssetStaticVersionUpdateFailedEventArgs());
            console.error(`[Startup] UpdateStaticVersion failed: ${message}, retrying`);
            // 对照 Unity:自重试
            setTimeout(() => this.ChangeToState(fsm, ProcedureUpdateStaticVersion), 1000);
        }
    }
}
