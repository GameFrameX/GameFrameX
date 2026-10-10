import ProcedureBase from "../../../gameframex/procedure/ProcedureBase";
import type IFsm from "../../../gameframex/fsm/IFsm";
import type IProcedureManager from "../../../gameframex/procedure/IProcedureManager";
import EventSystems from "../../../gameframex/event/EventSystems";
import { AssetPatchStatesChangeEventArgs, AssetDownloadProgressUpdateEventArgs, AssetWebFileDownloadFailedEventArgs, EPatchStates } from "../startup/AssetEventArgs";
import StartupContext from "./StartupContext";
import { DIFF_FILES_KEY, default as ProcedureCreateDownloader } from "./ProcedureCreateDownloader";
import ProcedurePatchDone from "./ProcedurePatchDone";
import type { DiffFile } from "../startup/ResourceVersionSystem";

/**
 * 下载差异资源。
 *
 * 对照 Unity `ProcedureDownloadWebFiles`:预加载差异文件(进度事件 + UI 进度);
 * 出错 Fire 失败事件并回到 CreateDownloader 重建差异(Unity 语义)。
 * Laya 差异:Web 端"下载"= 按版本化 URL 预热宿主资源缓存。
 */
export default class ProcedureDownloadWebFiles extends ProcedureBase {
    /**
     * 进入状态。
     * @param fsm 流程 FSM
     */
    public OnEnter(fsm: IFsm<IProcedureManager>): void {
        void this.enterAsync(fsm);
    }

    private async enterAsync(fsm: IFsm<IProcedureManager>): Promise<void> {
        const context = new StartupContext(fsm);
        EventSystems.FireNow(this, AssetPatchStatesChangeEventArgs.Create(EPatchStates.DownloadWebFiles));

        const files = fsm.GetData<DiffFile[]>(DIFF_FILES_KEY) ?? [];
        const system = context.resourceSystem!;
        const uiHandler = context.uiHandler;
        try {
            await system.preload(context.remoteRoot, context.version, files, (progress) => {
                const current = Math.round(progress * files.length);
                EventSystems.FireNow(this, AssetDownloadProgressUpdateEventArgs.Create(current, files.length));
                uiHandler?.SetProgress(progress, `${current}/${files.length}`);
            });
            this.ChangeToState(fsm, ProcedurePatchDone);
        } catch (error) {
            const message = error instanceof Error ? error.message : String(error);
            console.error(`[Startup] DownloadWebFiles failed: ${message}, recreate downloader`);
            EventSystems.FireNow(this, new AssetWebFileDownloadFailedEventArgs());
            // 对照 Unity:回到 CreateDownloader 重来
            this.ChangeToState(fsm, ProcedureCreateDownloader);
        }
    }
}
