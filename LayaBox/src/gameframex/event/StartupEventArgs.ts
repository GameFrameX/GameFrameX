import GameEventArgs from "./GameEventArgs";

/**
 * 启动完成事件参数。
 *
 * 对照 Unity `GameFrameX.Startup.Runtime.StartupCompletedEventArgs`:
 * StartupRunner 全部流程走完后发布。
 */
export class StartupCompletedEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'StartupCompletedEventArgs';

    public get Id(): string {
        return StartupCompletedEventArgs.EventId;
    }
}

/**
 * 启动失败事件参数。
 *
 * 对照 Unity `GameFrameX.Startup.Runtime.StartupFailedEventArgs`。
 */
export class StartupFailedEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'StartupFailedEventArgs';

    public static Create(failedProcedureName: string, errorMessage: string): StartupFailedEventArgs {
        const args = new StartupFailedEventArgs();
        args.FailedProcedureName = failedProcedureName;
        args.ErrorMessage = errorMessage;
        return args;
    }

    public get Id(): string {
        return StartupFailedEventArgs.EventId;
    }

    /** 失败时所在的流程名。 */
    public FailedProcedureName!: string;

    /** 失败原因。 */
    public ErrorMessage!: string;
}
