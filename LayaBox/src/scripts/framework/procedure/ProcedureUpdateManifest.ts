import ProcedureBase from "../../../gameframex/procedure/ProcedureBase";
import type IFsm from "../../../gameframex/fsm/IFsm";
import type IProcedureManager from "../../../gameframex/procedure/IProcedureManager";
import EventSystems from "../../../gameframex/event/EventSystems";
import { AssetPatchStatesChangeEventArgs, AssetPatchManifestUpdateFailedEventArgs, EPatchStates } from "../startup/AssetEventArgs";
import StartupContext from "./StartupContext";
import ProcedureCreateDownloader from "./ProcedureCreateDownloader";
import ProcedurePatchDone from "./ProcedurePatchDone";

/**
 * 更新资源清单。
 *
 * 对照 Unity `ProcedureUpdateManifest`:拉全量文件清单;本地模式跳过直接完成。
 */
export default class ProcedureUpdateManifest extends ProcedureBase {
    /**
     * 进入状态。
     * @param fsm 流程 FSM
     */
    public OnEnter(fsm: IFsm<IProcedureManager>): void {
        void this.enterAsync(fsm);
    }

    private async enterAsync(fsm: IFsm<IProcedureManager>): Promise<void> {
        const context = new StartupContext(fsm);
        if (context.remoteRoot === '') {
            // 本地模式:无清单差异
            this.ChangeToState(fsm, ProcedurePatchDone);
            return;
        }

        EventSystems.FireNow(this, AssetPatchStatesChangeEventArgs.Create(EPatchStates.UpdateManifest));
        const system = context.resourceSystem!;
        try {
            context.fileTable = await system.requestFileTable(context.remoteRoot, context.version);
            this.ChangeToState(fsm, ProcedureCreateDownloader);
        } catch (error) {
            const message = error instanceof Error ? error.message : String(error);
            EventSystems.FireNow(this, new AssetPatchManifestUpdateFailedEventArgs());
            console.error(`[Startup] UpdateManifest failed: ${message}, retrying`);
            // 对照 Unity:自重试
            setTimeout(() => this.ChangeToState(fsm, ProcedureUpdateManifest), 1000);
        }
    }
}
