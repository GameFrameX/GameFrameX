import GameEntry from "../src/gameframex/GameEntry";
import IModule from "../src/gameframex/IModule";
import GameApp from "../src/gameframex/GameApp";
import { test, assertEqual, assertThrows, assert } from "./harness";

class RecordingModule implements IModule {
    public readonly events: string[] = [];
    public async init(): Promise<void> { this.events.push('init'); }
    public update(_elapseSeconds: number, _realElapseSeconds: number): void { this.events.push('update'); }
    public shutdown(): void { this.events.push('shutdown'); }
}

class HighPriorityModule extends RecordingModule {
    public readonly priority: number = 10;
}

class LowPriorityModule extends RecordingModule {
    public readonly priority: number = -10;
}

test('GameEntry: registerModule/getModule 同名单例,未知名抛错', () => {
    const module = new RecordingModule();
    GameEntry.registerModule('TestSingleton', module);
    try {
        assert(GameEntry.getModule('TestSingleton') === module, '同名返回同一实例');
        assert(GameEntry.tryGetModule('TestSingleton') === module, 'tryGetModule 命中');
        assert(GameEntry.hasModule('TestSingleton'), 'hasModule 命中');
        assert(GameEntry.tryGetModule('Nope') == null, 'tryGetModule 未注册返回 null');
        assertThrows(() => GameEntry.getModule('Nope'), '未知名应抛错');
        assertThrows(() => GameEntry.registerModule('TestSingleton', new RecordingModule()), '重复注册应抛错');
    } finally {
        GameEntry.shutdown();
    }
});

test('GameEntry/GameApp: 优先级驱动、域组装、事件队列与关闭(共享注册表,顺序执行)', async () => {
    const high = new HighPriorityModule();
    const low = new LowPriorityModule();
    GameEntry.registerModule('High', high);
    GameEntry.registerModule('Low', low);
    await GameEntry.initialize();

    high.events.length = 0;
    low.events.length = 0;
    GameEntry.update(0.016, 0.016);
    assertEqual(high.events.join(',') + '|' + low.events.join(','), 'update|update', '两模块都被轮询');

    GameEntry.shutdown();
    assertEqual(high.events.join(','), 'update,shutdown', '高优先级模块 shutdown');
    assert(low.events.includes('shutdown'), '低优先级模块 shutdown');
    assert(!GameEntry.hasModule('High'), 'shutdown 后注册表清空');

    await GameApp.initialize();
    try {
        assert(GameApp.Fsm != null, 'Fsm 域');
        assert(GameApp.Procedure != null, 'Procedure 域');
        assert(GameApp.Event != null, 'Event 域');
        assert(GameApp.Timer != null && typeof GameApp.Timer.UnixTimeMilliseconds === 'function', 'Timer 域');
        assert(GameEntry.getModule('Fsm') === GameApp.Fsm, 'Fsm 域注册进 GameEntry');
        assert(GameEntry.getModule('Procedure') === GameApp.Procedure, 'Procedure 域注册进 GameEntry');
        assert(GameEntry.getModule('Event') === GameApp.Event, 'Event 域注册进 GameEntry');
        assert(Math.abs(GameApp.Timer.UnixTimeMilliseconds() - Date.now()) < 5000, '时间戳接近本地时钟');

        const before = Date.now();
        GameApp.Timer.SyncServerTime(Math.floor(before / 1000) + 10, before + 10000);
        assert(Math.abs(GameApp.Timer.UnixTimeMilliseconds() - (before + 10000)) < 5000, 'SyncServerTime 生效');
        assert(GameApp.Timer.DifferenceMilliseconds(before) > 9000, 'Difference 基于校时偏移');
    } finally {
        GameApp.shutdown();
    }
});
