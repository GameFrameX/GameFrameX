/**
 * 启动流程 FSM BlackBoard 键。
 *
 * 对照 Unity `BlackBoardKeys`(5 个注入键)。
 */
export class BlackBoardKeys {
    public static readonly StartupOptions: string = '__startup_options__';
    public static readonly StartupUIHandler: string = '__startup_ui_handler__';
    public static readonly StartupHotfixLauncher: string = '__startup_hotfix_launcher__';
    public static readonly StartupCompletionSource: string = '__startup_completion_source__';
    public static readonly StartupHttpParamsProvider: string = '__startup_http_params_provider__';
}
