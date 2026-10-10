/**
 * 启动配置。
 *
 * 对照 Unity `com.gameframex.unity.startup` StartupOptions(客户端可用子集):
 * 远程启动接口地址、渠道、App 版本、跳过远程请求开关。
 * Laya 差异:无 ScriptableObject,由 ApplicationStartupEntry 以代码默认值构造。
 */
export default class StartupOptions {
    /** 全局信息接口地址列表(POST,主备倒换)。 */
    public GlobalInfoUrls: string[] = [];

    /** 渠道名。 */
    public Channel: string = 'default';

    /** 子渠道名。 */
    public SubChannel: string = 'default';

    /** 包名。 */
    public PackageName: string = 'com.gameframex.layabox';

    /** 当前 App 版本(用于版本检查请求)。 */
    public AppVersion: string = '1.0.0';

    /** 启动界面资源名(FairyGUI UILauncher 包组件)。 */
    public LauncherUIResName: string = 'UILauncher';

    /** 跳过三个远程启动请求(本地/离线开发模式),直接进 Patch 流程。 */
    public SkipRemoteStartupRequests: boolean = false;

    /** 远程资源根地址(空 = 全部使用首包内置资源,即本地模式)。 */
    public RemoteResourceRoot: string = '';

    /** 单 URL 最大重试次数。 */
    public MaxAttemptsPerUrl: number = 3;

    /** 重试间隔毫秒。 */
    public RetryDelayMs: number = 1000;
}
