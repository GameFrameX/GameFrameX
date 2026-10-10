import ProcedureBase from "../../../gameframex/procedure/ProcedureBase";
import type IFsm from "../../../gameframex/fsm/IFsm";
import type IProcedureManager from "../../../gameframex/procedure/IProcedureManager";
import EventSystems from "../../../gameframex/event/EventSystems";
import { AssetPatchStatesChangeEventArgs, AssetFoundUpdateFilesEventArgs, EPatchStates } from "../startup/AssetEventArgs";
import StartupContext from "./StartupContext";
import ProcedurePatchDone from "./ProcedurePatchDone";
import ProcedureDownloadWebFiles from "./ProcedureDownloadWebFiles";
import type { DiffFile } from "../startup/ResourceVersionSystem";

/** 差异文件集 BlackBoard 键。 */
export const DIFF_FILES_KEY = '__startup_diff_files__';

/**
 * 创建下载器。
 *
 * 对照 Unity `ProcedureCreateDownloader`:计算差异集;无差异直接完成;
 * 有差异 Fire AssetFoundUpdateFiles 进入下载流程。
 */
export default class ProcedureCreateDownloader extends ProcedureBase {
    /**
     * 进入状态。
     * @param fsm 流程 FSM
     */
    public OnEnter(fsm: IFsm<IProcedureManager>): void {
        void this.enterAsync(fsm);
    }

    private async enterAsync(fsm: IFsm<IProcedureManager>): Promise<void> {
        const context = new StartupContext(fsm);
        EventSystems.FireNow(this, AssetPatchStatesChangeEventArgs.Create(EPatchStates.CreateDownloader));

        const system = context.resourceSystem!;
        const diff = system.computeDiff(context.builtinTable ?? { files: {} }, context.fileTable ?? { files: {} });
        fsm.SetData(DIFF_FILES_KEY, diff as DiffFile[]);
        if (diff.length === 0) {
            console.log('[Startup] No resources need to be downloaded.');
            this.ChangeToState(fsm, ProcedurePatchDone);
            return;
        }

        const totalBytes = diff.reduce((sum, file) => sum + file.entry.size, 0);
        EventSystems.FireNow(this, AssetFoundUpdateFilesEventArgs.Create('resource', diff.length, totalBytes));
        this.ChangeToState(fsm, ProcedureDownloadWebFiles);
    }
}
