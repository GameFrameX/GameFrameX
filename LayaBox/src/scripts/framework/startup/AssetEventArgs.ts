import GameEventArgs from "../../../gameframex/event/GameEventArgs";

/** 资源补丁阶段(scout 对照 Unity EPatchStates)。 */
export enum EPatchStates {
    UpdateStaticVersion = 1,
    UpdateManifest = 2,
    CreateDownloader = 3,
    DownloadWebFiles = 4,
    PatchDone = 5,
}

/** 资源补丁阶段变化事件。 */
export class AssetPatchStatesChangeEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'AssetPatchStatesChangeEventArgs';

    public static Create(patchStates: EPatchStates): AssetPatchStatesChangeEventArgs {
        const args = new AssetPatchStatesChangeEventArgs();
        args.PatchStates = patchStates;
        return args;
    }

    public get Id(): string {
        return AssetPatchStatesChangeEventArgs.EventId;
    }

    public PatchStates!: EPatchStates;
}

/** 静态版本文件更新失败事件。 */
export class AssetStaticVersionUpdateFailedEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'AssetStaticVersionUpdateFailedEventArgs';

    public get Id(): string {
        return AssetStaticVersionUpdateFailedEventArgs.EventId;
    }
}

/** 清单更新失败事件。 */
export class AssetPatchManifestUpdateFailedEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'AssetPatchManifestUpdateFailedEventArgs';

    public get Id(): string {
        return AssetPatchManifestUpdateFailedEventArgs.EventId;
    }
}

/** 发现待更新文件事件。 */
export class AssetFoundUpdateFilesEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'AssetFoundUpdateFilesEventArgs';

    public static Create(packageName: string, updateCount: number, updateSize: number): AssetFoundUpdateFilesEventArgs {
        const args = new AssetFoundUpdateFilesEventArgs();
        args.PackageName = packageName;
        args.UpdateCount = updateCount;
        args.UpdateSize = updateSize;
        return args;
    }

    public get Id(): string {
        return AssetFoundUpdateFilesEventArgs.EventId;
    }

    public PackageName!: string;
    public UpdateCount!: number;
    public UpdateSize!: number;
}

/** 下载进度事件。 */
export class AssetDownloadProgressUpdateEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'AssetDownloadProgressUpdateEventArgs';

    public static Create(current: number, total: number): AssetDownloadProgressUpdateEventArgs {
        const args = new AssetDownloadProgressUpdateEventArgs();
        args.Current = current;
        args.Total = total;
        return args;
    }

    public get Id(): string {
        return AssetDownloadProgressUpdateEventArgs.EventId;
    }

    public Current!: number;
    public Total!: number;
}

/** 文件下载失败事件。 */
export class AssetWebFileDownloadFailedEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'AssetWebFileDownloadFailedEventArgs';

    public get Id(): string {
        return AssetWebFileDownloadFailedEventArgs.EventId;
    }
}
