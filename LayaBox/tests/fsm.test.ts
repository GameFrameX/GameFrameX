import Fsm from "../src/gameframex/fsm/Fsm";
import FsmManager from "../src/gameframex/fsm/FsmManager";
import FsmState from "../src/gameframex/fsm/FsmState";
import type IFsm from "../src/gameframex/fsm/IFsm";
import { test, assert, assertEqual, assertThrows } from "./harness";

class Owner { }

const calls: string[] = [];

class StateA extends FsmState<Owner> {
    public OnInit(_fsm: IFsm<Owner>): void { calls.push('A.Init'); }
    public OnEnter(_fsm: IFsm<Owner>): void { calls.push('A.Enter'); }
    public OnUpdate(_fsm: IFsm<Owner>, _e: number, _r: number): void { calls.push('A.Update'); }
    public OnLeave(_fsm: IFsm<Owner>, isShutdown: boolean): void { calls.push(`A.Leave:${isShutdown}`); }
    public OnDestroy(_fsm: IFsm<Owner>): void { calls.push('A.Destroy'); }
}

class StateB extends FsmState<Owner> {
    public OnEnter(_fsm: IFsm<Owner>): void { calls.push('B.Enter'); }
    public OnLeave(_fsm: IFsm<Owner>, isShutdown: boolean): void { calls.push(`B.Leave:${isShutdown}`); }
    public OnDestroy(_fsm: IFsm<Owner>): void { calls.push('B.Destroy'); }
}

class StateChanger extends FsmState<Owner> {
    public OnEnter(fsm: IFsm<Owner>): void {
        calls.push('Changer.Enter');
        this.ChangeToState(fsm, StateB);
    }
}

test('FSM: Create 时全部状态 OnInit,Start 进入首个状态', () => {
    calls.length = 0;
    const fsm = Fsm.Create('t', new Owner(), [new StateA(), new StateB()]);
    assertEqual(calls.join(','), 'A.Init', 'Create 应只触发 OnInit');
    fsm.Start(StateA);
    assertEqual(calls.join(','), 'A.Init,A.Enter', 'Start 应触发 OnEnter');
    assert(fsm.IsRunning, 'Start 后应 IsRunning');
    assertEqual(fsm.CurrentStateName, 'StateA', '当前状态名');
});

test('FSM: ChangeState 时序 = Leave(false) → 新状态 Enter', () => {
    calls.length = 0;
    const fsm = Fsm.Create('t', new Owner(), [new StateA(), new StateB()]);
    fsm.Start(StateA);
    calls.length = 0;
    fsm.changeState(StateB);
    assertEqual(calls.join(','), 'A.Leave:false,B.Enter', '切换时序');
});

test('FSM: 状态内 ChangeToState 立即切换', () => {
    calls.length = 0;
    const fsm = Fsm.Create('t', new Owner(), [new StateChanger(), new StateB()]);
    fsm.Start(StateChanger);
    assertEqual(calls.join(','), 'Changer.Enter,B.Enter', 'OnEnter 内切换直达 B');
    assertEqual(fsm.CurrentStateName, 'StateB', '当前状态应为 B');
});

test('FSM: update 只驱动当前状态并累计时间', () => {
    calls.length = 0;
    const fsm = Fsm.Create('t', new Owner(), [new StateA(), new StateB()]);
    fsm.Start(StateA);
    calls.length = 0;
    fsm.update(0.5, 0.5);
    fsm.update(0.5, 0.5);
    assertEqual(calls.join(','), 'A.Update,A.Update', '仅当前状态轮询');
    assertEqual(fsm.CurrentStateTime, 1, '状态持续时间累计');
});

test('FSM: BlackBoard 数据存取/移除/存在性', () => {
    const fsm = Fsm.Create('t', new Owner(), [new StateA()]);
    assert(!fsm.HasData('k'), '初始无数据');
    fsm.SetData('k', 42);
    assert(fsm.HasData('k'), 'SetData 后存在');
    assertEqual(fsm.GetData<number>('k')!, 42, 'GetData 返回写入值');
    fsm.SetData('k', 'over');
    assertEqual(fsm.GetData<string>('k')!, 'over', 'SetData 覆盖');
    assert(fsm.RemoveData('k'), 'RemoveData 成功');
    assert(!fsm.HasData('k'), '移除后不存在');
});

test('FSM: shutdown 时序 = 当前态 Leave(true) → 全部 OnDestroy', () => {
    calls.length = 0;
    const fsm = Fsm.Create('t', new Owner(), [new StateA(), new StateB()]);
    fsm.Start(StateA);
    calls.length = 0;
    fsm.shutdown();
    assertEqual(calls.join(','), 'A.Leave:true,A.Destroy,B.Destroy', '销毁时序');
    assert(fsm.IsDestroyed, '销毁标记');
});

test('FSM: 重复注册同名状态抛错;运行中重复 Start 抛错', () => {
    assertThrows(() => Fsm.Create('t', new Owner(), [new StateA(), new StateA()]), '重复状态类型');
    const fsm = Fsm.Create('t', new Owner(), [new StateA()]);
    fsm.Start(StateA);
    assertThrows(() => fsm.Start(StateA), '运行中重复 Start');
});

test('FsmManager: CreateFsm/GetFsm/HasFsm/DestroyFsm', () => {
    const manager = new FsmManager();
    assert(!manager.HasFsm(Owner, 'm1'), '初始不存在');
    manager.CreateFsm('m1', new Owner(), [new StateA()]);
    assert(manager.HasFsm(Owner, 'm1'), '创建后存在');
    assertEqual(manager.Count, 1, '数量统计');
    assertThrows(() => manager.CreateFsm('m1', new Owner(), [new StateA()]), '重复创建同名 FSM');
    assert(manager.DestroyFsm(Owner, 'm1'), '销毁成功');
    assert(!manager.HasFsm(Owner, 'm1'), '销毁后不存在');
    assert(!manager.DestroyFsm(Owner, 'm1'), '重复销毁返回 false');
});
