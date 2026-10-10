import FsmManager from "../src/gameframex/fsm/FsmManager";
import ProcedureBase from "../src/gameframex/procedure/ProcedureBase";
import ProcedureManager from "../src/gameframex/procedure/ProcedureManager";
import type IFsm from "../src/gameframex/fsm/IFsm";
import type IProcedureManager from "../src/gameframex/procedure/IProcedureManager";
import { test, assertEqual, assertThrows, assert } from "./harness";

const calls: string[] = [];

class ProcedureA extends ProcedureBase {
    public OnEnter(_fsm: IFsm<IProcedureManager>): void { calls.push('A.Enter'); }
    public OnLeave(_fsm: IFsm<IProcedureManager>, isShutdown: boolean): void { calls.push(`A.Leave:${isShutdown}`); }
    public OnDestroy(_fsm: IFsm<IProcedureManager>): void { calls.push('A.Destroy'); }
}

class ProcedureB extends ProcedureBase {
    public OnEnter(_fsm: IFsm<IProcedureManager>): void { calls.push('B.Enter'); }
    public OnDestroy(_fsm: IFsm<IProcedureManager>): void { calls.push('B.Destroy'); }
}

class ProcedureAutoNext extends ProcedureBase {
    public OnEnter(fsm: IFsm<IProcedureManager>): void {
        calls.push('AutoNext.Enter');
        this.ChangeToState(fsm, ProcedureB);
    }
}

class ProcedureStepNext extends ProcedureBase {
    public OnEnter(_fsm: IFsm<IProcedureManager>): void { calls.push('A.Enter'); }
    public OnUpdate(fsm: IFsm<IProcedureManager>, _elapseSeconds: number, _realElapseSeconds: number): void {
        calls.push('A.Update');
        this.ChangeToState(fsm, ProcedureB);
    }
    public OnLeave(_fsm: IFsm<IProcedureManager>, isShutdown: boolean): void { calls.push(`A.Leave:${isShutdown}`); }
}

function createProcedureManager(procedures: ProcedureBase[]): { fsmManager: FsmManager; procedureManager: ProcedureManager } {
    const fsmManager = new FsmManager();
    const procedureManager = new ProcedureManager();
    procedureManager.Initialize(fsmManager, procedures);
    return { fsmManager, procedureManager };
}

test('Procedure: Initialize 建 FSM,StartProcedure 进入入口流程', () => {
    calls.length = 0;
    const bare = new ProcedureManager();
    assertThrows(() => bare.CurrentProcedure, '未 Initialize 访问 CurrentProcedure 应抛错');
    const { procedureManager } = createProcedureManager([new ProcedureA(), new ProcedureB()]);
    assert(procedureManager.CurrentProcedure == null, 'Initialize 后未 Start 返回 null');
    procedureManager.StartProcedure(ProcedureA);
    assertEqual(calls.join(','), 'A.Enter', '入口流程进入');
    assertEqual(procedureManager.CurrentProcedure!.constructor.name, 'ProcedureA', '当前流程');
});

test('Procedure: 流程间切换 = Leave(false) → Enter(OnUpdate 驱动)', () => {
    calls.length = 0;
    const { procedureManager, fsmManager } = createProcedureManager([new ProcedureStepNext(), new ProcedureB()]);
    procedureManager.StartProcedure(ProcedureStepNext);
    calls.length = 0;
    fsmManager.update(0.016, 0.016);
    assertEqual(calls.join(','), 'A.Update,A.Leave:false,B.Enter', '切换时序');
});

test('Procedure: OnEnter 内自动流转下一流程', () => {
    calls.length = 0;
    const { procedureManager } = createProcedureManager([new ProcedureAutoNext(), new ProcedureB()]);
    procedureManager.StartProcedure(ProcedureAutoNext);
    assertEqual(calls.join(','), 'AutoNext.Enter,B.Enter', '自动流转');
    assertEqual(procedureManager.CurrentProcedureTime, 0, '切换后持续时间归零');
});

test('Procedure: HasProcedure/GetProcedure/未初始化访问抛错', () => {
    const { procedureManager } = createProcedureManager([new ProcedureA()]);
    assert(procedureManager.HasProcedure(ProcedureA), '已注册流程存在');
    assert(!procedureManager.HasProcedure(ProcedureB), '未注册流程不存在');
    assert(procedureManager.GetProcedure(ProcedureA) != null, 'GetProcedure 命中');
    const bare = new ProcedureManager();
    assertThrows(() => bare.HasProcedure(ProcedureA), '未 Initialize 调 HasProcedure');
});

test('Procedure: DestroyProcedures 触发当前流程 Leave(true)+OnDestroy,可重建', () => {
    calls.length = 0;
    const { procedureManager } = createProcedureManager([new ProcedureA(), new ProcedureB()]);
    procedureManager.StartProcedure(ProcedureA);
    calls.length = 0;
    procedureManager.DestroyProcedures();
    assertEqual(calls.join(','), 'A.Leave:true,A.Destroy,B.Destroy', '销毁时序');
    procedureManager.ReinitializeProcedures([new ProcedureA()]);
    procedureManager.StartProcedure(ProcedureA);
    assertEqual(procedureManager.CurrentProcedure!.constructor.name, 'ProcedureA', '重建后可启动');
});
