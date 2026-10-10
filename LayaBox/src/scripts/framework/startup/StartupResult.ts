/**
 * 启动结果。
 *
 * 对照 Unity `StartupResult`:Success/FailedProcedureName/FailedUrl/ErrorMessage。
 */
export default class StartupResult {
    private constructor(
        public readonly Success: boolean,
        public readonly FailedProcedureName: string,
        public readonly FailedUrl: string,
        public readonly ErrorMessage: string,
    ) { }

    /**
     * 成功结果。
     */
    public static Succeed(): StartupResult {
        return new StartupResult(true, '', '', '');
    }

    /**
     * 失败结果。
     * @param failedProcedureName 失败流程名
     * @param failedUrl 失败地址
     * @param errorMessage 错误信息
     */
    public static Fail(failedProcedureName: string, failedUrl: string | null, errorMessage: string | null): StartupResult {
        return new StartupResult(false, failedProcedureName, failedUrl ?? '', errorMessage ?? '');
    }
}
