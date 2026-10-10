import UIManager from "../src/gameframex/ui/UIManager";
import UIForm from "../src/gameframex/ui/UIForm";
import { UIGroupConstants } from "../src/gameframex/ui/UIGroupConstants";
import EventSystems from "../src/gameframex/event/EventSystems";
import type GameEventArgs from "../src/gameframex/event/GameEventArgs";
import { UIFormOpenedEventArgs, UIFormClosedEventArgs } from "../src/gameframex/ui/UIEventArgs";
import FakeUIFormHelper from "./FakeUIFormHelper";
import { test, assert, assertEqual } from "./harness";

const calls: string[] = [];

class FormA extends UIForm {
    public OnInit(): void { calls.push('A.OnInit'); }
    public OnOpen(userData: unknown): void { calls.push('A.OnOpen'); super.OnOpen(userData); }
    public BindEvent(): void { calls.push('A.BindEvent'); }
    public LoadData(): void { calls.push('A.LoadData'); }
    public UpdateLocalization(): void { calls.push('A.UpdateLocalization'); }
    public OnCover(): void { calls.push('A.OnCover'); super.OnCover(); }
    public OnPause(): void { calls.push('A.OnPause'); super.OnPause(); }
    public OnReveal(): void { calls.push('A.OnReveal'); super.OnReveal(); }
    public OnResume(): void { calls.push('A.OnResume'); super.OnResume(); }
    public OnClose(isShutdown: boolean, userData: unknown): void { calls.push('A.OnClose'); super.OnClose(isShutdown, userData); }
}

class FormB extends FormA {
    public OnInit(): void { calls.push('B.OnInit'); }
    public OnOpen(userData: unknown): void { calls.push('B.OnOpen'); super.OnOpen(userData); }
    public OnClose(isShutdown: boolean, userData: unknown): void { calls.push('B.OnClose'); super.OnClose(isShutdown, userData); }
}

function setup(): { manager: UIManager; helper: FakeUIFormHelper } {
    const helper = new FakeUIFormHelper();
    const manager = new UIManager();
    manager.SetUIFormHelper(helper);
    return { manager, helper };
}


test('UI: 打开时序 = OnInit → OnOpen → BindEvent → LoadData → UpdateLocalization → 组入栈', async () => {

    calls.length = 0;
    const { manager } = setup();
    const form = await manager.OpenAsync(FormA, { packageName: 'P', resName: 'A' }, { id: 1 });
    assertEqual(calls.join(','), 'A.OnInit,A.OnOpen,A.BindEvent,A.LoadData,A.UpdateLocalization', '固定时序');

    void form;
});

test('UI: 打开完成事件与组/序列号/用户数据', async () => {
    const { manager } = setup();
    let opened: UIForm | null = null;
    const onOpened = (_s: unknown, args: GameEventArgs): void => {
        opened = (args as UIFormOpenedEventArgs).UIForm;
    };
    EventSystems.Subscribe(UIFormOpenedEventArgs.EventId, onOpened);
    try {
        const form = await manager.OpenAsync(FormA, { packageName: 'P', resName: 'A' }, 'ud');
        EventSystems.instance.update(0, 0);
        assertEqual(opened, form, '打开事件携带界面');
        assertEqual(form.SerialId, 1, '序列号递增');
        assertEqual(form.UserData, 'ud', '用户数据透传');
        assertEqual(form.UIGroup?.Name, 'Normal', '默认入 Normal 组');
    } finally {
        EventSystems.Unsubscribe(UIFormOpenedEventArgs.EventId, onOpened);
        manager.CloseUIGroup('Normal');
    }
});

test('UI: 同组覆盖 = 旧界面 OnCover+OnPause;关闭新界面 = OnReveal+OnResume', async () => {
    calls.length = 0;
    const { manager } = setup();
    const a = await manager.OpenAsync(FormA, { packageName: 'P', resName: 'A' });
    calls.length = 0;

    const b = await manager.OpenAsync(FormB, { packageName: 'P', resName: 'B' });
    assert(calls.includes('A.OnCover'), 'A 被覆盖回调');
    assert(calls.includes('A.OnPause'), 'A 默认 PauseCoveredUIForm 被暂停');
    assert(!b.__paused, 'B 栈顶不暂停');

    calls.length = 0;
    manager.CloseUIForm(b);
    assert(calls.includes('B.OnClose'), 'B 关闭回调');
    assert(calls.includes('A.OnReveal'), 'A 恢复可见');
    assert(calls.includes('A.OnResume'), 'A 恢复活跃');
    assert(a.Available && a.Visible, 'A 状态恢复');
});

test('UI: PauseCoveredUIForm=false 时只覆盖不暂停', async () => {
    calls.length = 0;
    const { manager } = setup();
    await manager.OpenAsync(FormA, { packageName: 'P', resName: 'A' }, null, { pauseCoveredUIForm: false });
    calls.length = 0;
    await manager.OpenAsync(FormB, { packageName: 'P', resName: 'B' });

    assert(calls.includes('A.OnCover'), 'A 仍被覆盖');
    assert(!calls.includes('A.OnPause'), 'A 不暂停');
    manager.CloseUIGroup('Normal');
});

test('UI: 关闭触发 Hide→OnClose→Dispose→Release 与关闭事件', async () => {
    const { manager, helper } = setup();
    let closedForm: UIForm | null = null;
    const onClosed = (_s: unknown, args: GameEventArgs): void => {
        closedForm = (args as UIFormClosedEventArgs).UIForm;
    };
    EventSystems.Subscribe(UIFormClosedEventArgs.EventId, onClosed);
    try {
        const form = await manager.OpenAsync(FormA, { packageName: 'P', resName: 'A' });
        manager.CloseUIForm(form);
        EventSystems.instance.update(0, 0);

        assertEqual(helper.released.length, 1, '释放一次');
        assertEqual(closedForm, form, '关闭事件携带界面');
        assertEqual(form.Handle, null, 'Handle 置空');
    } finally {
        EventSystems.Unsubscribe(UIFormClosedEventArgs.EventId, onClosed);
    }
});

test('UI: 界面 Subscribe 在 Dispose 时统一退订', async () => {
    const { manager } = setup();
    let hits = 0;
    const form = await manager.OpenAsync(FormA, { packageName: 'P', resName: 'A' });
    form.Subscribe('UITestEvent', () => hits++);
    assertEqual(EventSystems.Count('UITestEvent'), 1, '订阅登记');
    manager.CloseUIForm(form);
    assertEqual(EventSystems.Count('UITestEvent'), 0, '关闭后退订');
    EventSystems.FireNow(null, 'UITestEvent');
    assertEqual(hits, 0, '不再收到');
});

test('UI: 组常量与 Unity 数值一致(抽查)', () => {
    assertEqual(UIGroupConstants.Normal.Depth, 10, 'Normal=10');
    assertEqual(UIGroupConstants.Hidden.Depth, 40, 'Hidden=40 最底');
    assertEqual(UIGroupConstants.System.Depth, -35, 'System=-35 最顶');
    assertEqual(UIGroupConstants.Normal.Name, 'Normal', '组名');
});
