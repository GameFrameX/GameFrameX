/**
 * 资源版本体系(Laya 对 YooAsset HostPlayMode 的等价替代,spec §3.4/R4)。
 *
 * 契约:
 * - `<resRoot>/version.json` → { version: string }:当前资源版本号;
 * - `<resRoot>/<version>/filetable.json` → { files: Record<path, { hash, size }> }:全量文件清单;
 * - 首包内置 `resources/filetable.json`(构建产物)与之对比得差异集;
 * - 差异文件按 `<resRoot>/<version>/<path>?v=<hash>` 版本化 URL 预加载(prefetch 注入,
 *   浏览器为 Laya.loader 预热,node 测试为桩)。
 * 主备 CDN 重试在 fetchText 层实现(R4 对策)。
 */

/** 文件清单条目。 */
export interface FileEntry {
    hash: string;
    size: number;
}

/** 文件清单。 */
export interface FileTable {
    files: Record<string, FileEntry>;
}

/** 差异文件。 */
export interface DiffFile {
    path: string;
    entry: FileEntry;
}

/** 资源获取注入(fetch 文本与预加载,宿主/测试可替换)。 */
export interface ResourceIO {
    fetchText(url: string): Promise<string>;
    prefetch(url: string): Promise<void>;
}

/**
 * 资源版本系统。
 */
export default class ResourceVersionSystem {
    private readonly io: ResourceIO;

    /** 备用根地址(主根失败时倒换,R4 主备 CDN)。 */
    public fallbackRoots: string[] = [];

    constructor(io: ResourceIO) {
        this.io = io;
    }

    /**
     * 请求远端版本号。
     * @param root 资源根地址
     */
    public async requestVersion(root: string): Promise<string> {
        const text = await this.fetchTextWithFailover(`${root}/version.json`);
        return (JSON.parse(text) as { version: string }).version;
    }

    /**
     * 请求远端文件清单。
     * @param root 资源根地址
     * @param version 版本号
     */
    public async requestFileTable(root: string, version: string): Promise<FileTable> {
        const text = await this.fetchTextWithFailover(`${root}/${version}/filetable.json`);
        return JSON.parse(text) as FileTable;
    }

    /**
     * 计算差异集:远端存在且(本地缺失或 hash 不同)的文件。
     * @param localTable 首包内置清单
     * @param remoteTable 远端清单
     */
    public computeDiff(localTable: FileTable, remoteTable: FileTable): DiffFile[] {
        const diff: DiffFile[] = [];
        for (const [path, entry] of Object.entries(remoteTable.files)) {
            const local = localTable.files[path];
            if (local == null || local.hash !== entry.hash) {
                diff.push({ path, entry });
            }
        }

        return diff;
    }

    /**
     * 预加载差异文件(带进度回调)。
     * @param root 资源根地址
     * @param version 版本号
     * @param files 差异文件集
     * @param onProgress 进度回调(0~1)
     */
    public async preload(root: string, version: string, files: DiffFile[], onProgress: (progress: number) => void): Promise<void> {
        let done = 0;
        for (const file of files) {
            const url = `${root}/${version}/${file.path}?v=${file.entry.hash}`;
            await this.io.prefetch(url);
            done++;
            onProgress(files.length === 0 ? 1 : done / files.length);
        }

        onProgress(1);
    }

    private async fetchTextWithFailover(url: string): Promise<string> {
        const candidates = [url, ...this.fallbackRoots.map((root) => url.replace(/^[^/]+\/\/[^/]+/, root))];
        let lastError: unknown = null;
        for (const candidate of candidates) {
            try {
                return await this.io.fetchText(candidate);
            } catch (error) {
                lastError = error;
            }
        }

        throw lastError instanceof Error ? lastError : new Error(`fetch failed: ${url}`);
    }
}
