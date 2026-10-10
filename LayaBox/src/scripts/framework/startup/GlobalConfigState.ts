import type StartupOptions from "./StartupOptions";
import type { StartupHttpClient } from "./StartupHttp";

/** 全局信息响应 Data 结构。 */
export interface ResponseGlobalInfo {
    CheckAppVersionUrl: string;
    CheckResourceVersionUrl: string;
}

/** App 版本响应 Data 结构。 */
export interface ResponseGameAppVersion {
    IsUpgrade: boolean;
    IsForce: boolean;
    AppDownloadUrl: string;
    UpdateTitle: string;
    UpdateAnnouncement: string;
}

/** 资产包版本响应 Data 结构。 */
export interface ResponseGameAssetPackageVersion {
    Version: string;
    AssetPackagePath: string;
}

const CACHE_KEY_PREFIX = 'GameFrameX.Startup.NetworkCache.';

/**
 * 启动链全局状态与缓存。
 *
 * 对照 Unity `GlobalConfigComponent` + `StartupNetworkCacheUtility`:
 * 三个远程接口的响应解析与 localStorage 缓存回退。
 */
export class GlobalConfigState {
    public CheckAppVersionUrl: string = '';
    public CheckResourceVersionUrl: string = '';

    /**
     * 拉取全局信息(多 URL 倒换重试)。
     * @returns 是否成功
     */
    public async fetchGlobalInfo(options: StartupOptions, http: StartupHttpClient, params: Record<string, string>): Promise<boolean> {
        let lastError = '';
        for (const url of options.GlobalInfoUrls) {
            for (let attempt = 1; attempt <= options.MaxAttemptsPerUrl; attempt++) {
                try {
                    const result = await http.postJson(url, params);
                    if (result.Code <= 0) {
                        const data = result.Data as ResponseGlobalInfo;
                        this.CheckAppVersionUrl = data.CheckAppVersionUrl;
                        this.CheckResourceVersionUrl = data.CheckResourceVersionUrl;
                        this.cache('GlobalInfo', JSON.stringify(data));
                        return true;
                    }

                    lastError = `code ${result.Code}`;
                } catch (error) {
                    lastError = error instanceof Error ? error.message : String(error);
                }
            }
        }

        return false;
    }

    /**
     * 尝试应用缓存的全局信息。
     */
    public tryApplyCachedGlobalInfo(): boolean {
        const cached = this.readCache('GlobalInfo');
        if (cached == null) {
            return false;
        }

        try {
            const data = JSON.parse(cached) as ResponseGlobalInfo;
            this.CheckAppVersionUrl = data.CheckAppVersionUrl;
            this.CheckResourceVersionUrl = data.CheckResourceVersionUrl;
            return true;
        } catch {
            return false;
        }
    }

    /**
     * 拉取 App 版本信息。
     */
    public async fetchAppVersion(options: StartupOptions, http: StartupHttpClient, params: Record<string, string>): Promise<ResponseGameAppVersion | null> {
        for (let attempt = 1; attempt <= options.MaxAttemptsPerUrl; attempt++) {
            try {
                const result = await http.postJson(this.CheckAppVersionUrl, params);
                if (result.Code <= 0) {
                    const data = result.Data as ResponseGameAppVersion;
                    this.cache('AppVersion', JSON.stringify(data));
                    return data;
                }
            } catch {
                // 重试
            }

            await delay(options.RetryDelayMs);
        }

        return this.cachedAppVersion();
    }

    /**
     * 拉取资产包版本信息。
     */
    public async fetchAssetPackageVersion(options: StartupOptions, http: StartupHttpClient, params: Record<string, string>): Promise<ResponseGameAssetPackageVersion | null> {
        for (let attempt = 1; attempt <= options.MaxAttemptsPerUrl; attempt++) {
            try {
                const result = await http.postJson(this.CheckResourceVersionUrl, params);
                if (result.Code <= 0) {
                    const data = result.Data as ResponseGameAssetPackageVersion;
                    this.cache('AssetPackageVersion', JSON.stringify(data));
                    return data;
                }
            } catch {
                // 重试
            }

            await delay(options.RetryDelayMs);
        }

        const cached = this.readCache('AssetPackageVersion');
        if (cached != null) {
            try {
                return JSON.parse(cached) as ResponseGameAssetPackageVersion;
            } catch {
                return null;
            }
        }

        return null;
    }

    private cachedAppVersion(): ResponseGameAppVersion | null {
        const cached = this.readCache('AppVersion');
        if (cached == null) {
            return null;
        }

        try {
            return JSON.parse(cached) as ResponseGameAppVersion;
        } catch {
            return null;
        }
    }

    private cache(name: string, value: string): void {
        try {
            localStorage.setItem(CACHE_KEY_PREFIX + name, value);
        } catch {
            // 无 localStorage 环境(测试/小游戏沙箱)静默跳过
        }
    }

    private readCache(name: string): string | null {
        try {
            return localStorage.getItem(CACHE_KEY_PREFIX + name);
        } catch {
            return null;
        }
    }
}

function delay(ms: number): Promise<void> {
    // executor 形式而非 Promise.withResolvers:微信小游戏 JS 内核可能不支持 es2024 API
    return new Promise<void>((resolve) => {
        setTimeout(resolve, ms);
    });
}
