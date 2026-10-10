import type IFsm from "../../../gameframex/fsm/IFsm";
import type IProcedureManager from "../../../gameframex/procedure/IProcedureManager";
import EventSystems from "../../../gameframex/event/EventSystems";
import { StartupCompletedEventArgs, StartupFailedEventArgs } from "../../../gameframex/event/StartupEventArgs";
import StartupResult from "../startup/StartupResult";
import type IStartupUIHandler from "../startup/IStartupUIHandler";
import { BlackBoardKeys } from "../startup/BlackBoardKeys";

/** 启动完成信号(对照 Unity UniTaskCompletionSource<StartupResult> 的 resolve-once 语义)。 */
export class StartupCompletionSource {
    private settled = false;
    private result: StartupResult | null = null;
    private readonly waiters: ((result: StartupResult) => void)[] = [];

    /**
     * 是否已结束。
     */
    public get isSettled(): boolean {
        return this.settled;
    }

    /**
     * 结束并唤醒全部等待者;重复调用无效。
     */
    public trySetResult(result: StartupResult): boolean {
        if (this.settled) {
            return false;
        }

        this.settled = true;
        this.result = result;
        for (const waiter of [...this.waiters]) {
            waiter(result);
        }
        this.waiters.length = 0;
        return true;
    }

    /**
     * 等待结果(已结束时立即完成)。
     */
    public wait(): Promise<StartupResult> {
        if (this.settled) {
            return Promise.resolve(this.result!);
        }

        // executor 形式而非 Promise.withResolvers:微信小游戏 JS 内核可能不支持 es2024 API
        return new Promise<StartupResult>((resolve) => {
            this.waiters.push(resolve);
        });
    }
}

/**
 * 启动流程公共工具。
 *
 * 对照 Unity `StartupProcedureUtility`:成功/失败统一出口(事件 + completion source),
 * 上下文(options/uiHandler/completion source)全部经 FSM BlackBoard 注入。
 */
export default class StartupProcedureUtility {
    /**
     * 启动成功:Fire StartupCompleted + 结束信号。
     * @param fsm 流程 FSM
     */
    public static CompleteSuccess(fsm: IFsm<IProcedureManager>): void {
        const source = fsm.GetData<StartupCompletionSource>(BlackBoardKeys.StartupCompletionSource);
        EventSystems.Fire(null, new StartupCompletedEventArgs());
        source?.trySetResult(StartupResult.Succeed());
    }

    /**
     * 启动失败:提示 + Fire StartupFailed + 结束信号;FSM 停在失败状态。
     * @param fsm 流程 FSM
     * @param failedProcedureName 失败流程名
     * @param failedUrl 失败地址
     * @param errorMessage 错误信息
     */
    public static CompleteFailure(fsm: IFsm<IProcedureManager>, failedProcedureName: string, failedUrl: string, errorMessage: string): void {
        fsm.GetData<IStartupUIHandler>(BlackBoardKeys.StartupUIHandler)?.SetTipText(errorMessage);
        EventSystems.Fire(null, StartupFailedEventArgs.Create(failedProcedureName, errorMessage));
        fsm.GetData<StartupCompletionSource>(BlackBoardKeys.StartupCompletionSource)?.trySetResult(StartupResult.Fail(failedProcedureName, failedUrl, errorMessage));
    }
}
