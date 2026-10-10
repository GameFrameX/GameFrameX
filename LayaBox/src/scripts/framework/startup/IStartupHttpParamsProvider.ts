import type StartupOptions from "./StartupOptions";

/**
 * 启动 HTTP 公共参数提供者。
 *
 * 对照 Unity `IStartupHttpParamsProvider`/`DefaultStartupHttpParamsProvider`:
 * 提供语言/设备/平台等公共请求参数;三个远程启动请求共享。
 */
export default interface IStartupHttpParamsProvider {
    /** 构造公共请求参数表。 */
    Create(options: StartupOptions): Record<string, string>;
}

/** 默认实现:Language/UserLanguage/AppVersion/Platform/Channel。 */
export class DefaultStartupHttpParamsProvider implements IStartupHttpParamsProvider {
    public Create(options: StartupOptions): Record<string, string> {
        return {
            Language: 'ChineseSimplified',
            UserLanguage: 'ChineseSimplified',
            AppVersion: options.AppVersion,
            Platform: 'laya',
            Channel: options.Channel,
            SubChannel: options.SubChannel,
            PackageName: options.PackageName,
        };
    }
}
