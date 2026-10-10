import GameApp from "../src/gameframex/GameApp";
import StartupRunner, { type StartupRunnerEnvironment } from "../src/scripts/framework/startup/StartupRunner";
import StartupOptions from "../src/scripts/framework/startup/StartupOptions";
import type IStartupUIHandler from "../src/scripts/framework/startup/IStartupUIHandler";
import type { StartupUpgradeInfo } from "../src/scripts/framework/startup/IStartupUIHandler";
import type IHotfixLauncher from "../src/scripts/framework/startup/IHotfixLauncher";
import type IStartupHttpParamsProvider from "../src/scripts/framework/startup/IStartupHttpParamsProvider";
import type { StartupHttpClient } from "../src/scripts/framework/startup/StartupHttp";
import type { ResourceIO, FileTable } from "../src/scripts/framework/startup/ResourceVersionSystem";
import EventSystems from "../src/gameframex/event/EventSystems";
import { StartupCompletedEventArgs, StartupFailedEventArgs } from "../src/gameframex/event/StartupEventArgs";
import { AssetDownloadProgressUpdateEventArgs, AssetFoundUpdateFilesEventArgs } from "../src/scripts/framework/startup/AssetEventArgs";
import type GameEventArgs from "../src/gameframex/event/GameEventArgs";
import { test, assert, assertEqual } from "./harness";

class RecordingUIHandler implements IStartupUIHandler {
    public started = false;
    public tips: string[] = [];
    public progresses: number[] = [];
    public disposed = false;

    public async StartAsync(_name: string): Promise<void> { this.started = true; }
    public SetTipText(text: string): void { this.tips.push(text); }
    public SetProgress(progress: number, _description: string): void { this.progresses.push(progress); }
    public SetProgressUpdateFinish(): void { }
    public async ShowUpgradeAsync(_info: StartupUpgradeInfo): Promise<boolean> { return true; }
    public Dispose(): void { this.disposed = true; }
}

class RecordingHotfixLauncher implements IHotfixLauncher {
    public called = 0;
    public async StartAsync(_options: StartupOptions): Promise<string | null> {
        this.called++;
        return null;
    }
}

function httpClientStub(responses: { match: (url: string) => boolean; result: { Code: number; Data: unknown } | Error }[]): StartupHttpClient {

    return {
        postJson: async (url: string) => {
            for (const entry of responses) {
                if (entry.match(url)) {
                    if (entry.result instanceof Error) {
                        throw entry.result;
                    }

                    return entry.result;
                }
            }

            throw new Error(`unexpected url ${url}`);
        },
    };
}

test('Startup: 本地模式全链 11 步走完 → 热更启动 → StartupCompleted', async () => {
    await GameApp.initialize();
    try {
        const ui = new RecordingUIHandler();
        const hotfix = new RecordingHotfixLauncher();
        const options = new StartupOptions();
        options.SkipRemoteStartupRequests = true;

        let completed = false;
        const onCompleted = (): void => { completed = true; };
        EventSystems.Subscribe(StartupCompletedEventArgs.EventId, onCompleted);
        try {
            const result = await StartupRunner.Run(options, ui, hotfix);
            assert(result.Success, `本地链应成功: ${result.ErrorMessage}`);
            assertEqual(GameApp.Procedure.CurrentProcedure?.constructor.name, 'ProcedureGameLauncherState', '停在末流程');
            assert(hotfix.called === 1, '热更入口调用一次');
            assert(ui.started && ui.disposed, '启动 UI 展示并关闭');
            EventSystems.instance.update(0, 0);
            assert(completed, 'StartupCompleted 事件');
        } finally {
            EventSystems.Unsubscribe(StartupCompletedEventArgs.EventId, onCompleted);
        }
    } finally {
        GameApp.shutdown();
    }
});

test('Startup: 远程模式 版本检查+差异预载+进度事件 → 成功', async () => {
    await GameApp.initialize();
    try {
        const ui = new RecordingUIHandler();
        const hotfix = new RecordingHotfixLauncher();
        const options = new StartupOptions();
        options.GlobalInfoUrls = ['http://boot.test/global_info'];

        const http = httpClientStub([
            { match: (url) => url.includes('global_info'), result: { Code: 0, Data: { CheckAppVersionUrl: 'http://boot.test/app_version', CheckResourceVersionUrl: 'http://boot.test/resource_version' } } },
            { match: (url) => url.includes('app_version'), result: { Code: 0, Data: { IsUpgrade: false, IsForce: false, AppDownloadUrl: '', UpdateTitle: '', UpdateAnnouncement: '' } } },
            { match: (url) => url.includes('resource_version'), result: { Code: 0, Data: { Version: 'v2', AssetPackagePath: '' } } },
        ]);

        const prefetched: string[] = [];
        const builtinTable: FileTable = { files: { 'fairygui/UICommon/UICommon_fui.bytes': { hash: 'h1', size: 10 } } };
        const remoteTable: FileTable = {
            files: {
                'fairygui/UICommon/UICommon_fui.bytes': { hash: 'h1', size: 10 },
                'fairygui/UIMain/UIMain_fui.bytes': { hash: 'h9', size: 20 },
            },
        };
        const io: ResourceIO = {
            fetchText: async (url: string) => {
                if (url.endsWith('/version.json')) {
                    return JSON.stringify({ version: 'v2' });
                }

                if (url.endsWith('filetable.json')) {
                    return JSON.stringify(remoteTable);
                }

                throw new Error(`unexpected fetch ${url}`);
            },
            prefetch: async (url: string) => {
                prefetched.push(url);
            },
        };

        const progressEvents: number[] = [];
        const foundEvents: number[] = [];
        const onProgress = (_s: unknown, args: GameEventArgs): void => {
            progressEvents.push((args as AssetDownloadProgressUpdateEventArgs).Total);
        };
        const onFound = (_s: unknown, args: GameEventArgs): void => {
            foundEvents.push((args as AssetFoundUpdateFilesEventArgs).UpdateCount);
        };
        EventSystems.Subscribe(AssetDownloadProgressUpdateEventArgs.EventId, onProgress);
        EventSystems.Subscribe(AssetFoundUpdateFilesEventArgs.EventId, onFound);
        try {
            const env: StartupRunnerEnvironment = { httpClient: http, resourceIO: io, builtinTable };
            const result = await StartupRunner.Run(options, ui, hotfix, null, env);
            assert(result.Success, `远程链应成功: ${result.ErrorMessage}`);
            assertEqual(prefetched.length, 1, '仅差异文件预载');
            assert(prefetched[0].includes('v2/fairygui/UIMain/UIMain_fui.bytes'), `版本化 URL: ${prefetched[0]}`);
            assertEqual(foundEvents.join(','), '1', '发现 1 个差异文件事件');
            assert(progressEvents.length > 0, '下载进度事件');
            assert(ui.progresses.length > 0, 'UI 进度更新');
        } finally {
            EventSystems.Unsubscribe(AssetDownloadProgressUpdateEventArgs.EventId, onProgress);
            EventSystems.Unsubscribe(AssetFoundUpdateFilesEventArgs.EventId, onFound);
        }
    } finally {
        GameApp.shutdown();
    }
});

test('Startup: 全局信息失败 → StartupFailed + FailedProcedureName', async () => {
    await GameApp.initialize();
    try {
        const ui = new RecordingUIHandler();
        const hotfix = new RecordingHotfixLauncher();
        const options = new StartupOptions();
        options.GlobalInfoUrls = ['http://boot.test/global_info'];
        options.MaxAttemptsPerUrl = 1;
        options.RetryDelayMs = 1;

        const failing = httpClientStub([
            { match: (url) => url.includes('global_info'), result: new Error('network down') },
        ]);

        let failed: StartupFailedEventArgs | null = null;
        const onFailed = (_s: unknown, args: GameEventArgs): void => {
            failed = args as StartupFailedEventArgs;
        };
        EventSystems.Subscribe(StartupFailedEventArgs.EventId, onFailed);
        try {
            const result = await StartupRunner.Run(options, ui, hotfix, null, { httpClient: failing });
            assert(!result.Success, '应失败');
            assertEqual(result.FailedProcedureName, 'ProcedureGetGlobalInfoState', '失败流程名');
            assert(!hotfix.called, '热更未启动');
            EventSystems.instance.update(0, 0);
            assert(failed != null, 'StartupFailed 事件');
            assertEqual(failed!.FailedProcedureName, 'ProcedureGetGlobalInfoState', '事件负载');
        } finally {
            EventSystems.Unsubscribe(StartupFailedEventArgs.EventId, onFailed);
        }
    } finally {
        GameApp.shutdown();
    }
});

test('Startup: null 校验与 GlobalInfoUrls 校验同步抛错', async () => {
    await GameApp.initialize();
    GameApp.shutdown();
    const options = new StartupOptions();
    let threw = false;
    try {
        await StartupRunner.Run(options, new RecordingUIHandler(), new RecordingHotfixLauncher());
    } catch (error) {
        threw = error instanceof Error && error.message.includes('GlobalInfoUrls');
    }
    assert(threw, '无 GlobalInfoUrls 且未跳过远程应抛错');
});
