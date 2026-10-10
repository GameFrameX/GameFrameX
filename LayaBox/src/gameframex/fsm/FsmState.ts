import IFsm from "./IFsm";
import Fsm from "./Fsm";
import TypeCtor from "./TypeCtor";

/**
 * 有限状态机状态基类。
 *
 * 对照 Unity `GameFrameX.Fsm.Runtime.FsmState<T>`,生命周期同名同参:
 * OnInit / OnEnter / OnUpdate / OnFixedUpdate / OnLeave(isShutdown) / OnDestroy。
 * 与 C# 差异:TS 泛型无运行时类型,ChangeState/ChangeToState 显式传入目标状态构造函数;
 * C# `protected internal` 以 public 承载,标注"框架内部调用",业务子类只重写不直调。
 */
export default abstract class FsmState<T extends object> {
    /**
     * 有限状态机状态初始化时调用(框架内部调用)。
     * @param fsm 有限状态机引用
     */
    public OnInit(_fsm: IFsm<T>): void { }

    /**
     * 有限状态机状态进入时调用(框架内部调用)。
     * @param fsm 有限状态机引用
     */
    public OnEnter(_fsm: IFsm<T>): void { }

    /**
     * 有限状态机状态轮询时调用(框架内部调用)。
     * @param fsm 有限状态机引用
     * @param elapseSeconds 逻辑流逝时间,以秒为单位
     * @param realElapseSeconds 真实流逝时间,以秒为单位
     */
    public OnUpdate(_fsm: IFsm<T>, _elapseSeconds: number, _realElapseSeconds: number): void { }

    /**
     * 有限状态机状态固定轮询时调用(框架内部调用)。
     * @param fsm 有限状态机引用
     * @param elapseSeconds 逻辑流逝时间,以秒为单位
     * @param realElapseSeconds 真实流逝时间,以秒为单位
     */
    public OnFixedUpdate(_fsm: IFsm<T>, _elapseSeconds: number, _realElapseSeconds: number): void { }

    /**
     * 有限状态机状态离开时调用(框架内部调用)。
     * @param fsm 有限状态机引用
     * @param isShutdown 是否是关闭有限状态机时触发
     */
    public OnLeave(_fsm: IFsm<T>, _isShutdown: boolean): void { }

    /**
     * 有限状态机状态销毁时调用(框架内部调用)。
     * @param fsm 有限状态机引用
     */
    public OnDestroy(_fsm: IFsm<T>): void { }

    /**
     * 切换当前有限状态机状态(公开入口,供状态外部驱动)。
     * @param fsm 有限状态机引用
     * @param stateType 要切换到的有限状态机状态类型
     */
    public ChangeToState<TState extends FsmState<T>>(fsm: IFsm<T>, stateType: TypeCtor<TState>): void {
        this.ChangeState(fsm, stateType);
    }

    /**
     * 切换当前有限状态机状态。
     * @param fsm 有限状态机引用
     * @param stateType 要切换到的有限状态机状态类型
     */
    protected ChangeState<TState extends FsmState<T>>(fsm: IFsm<T>, stateType: TypeCtor<TState>): void {
        const fsmImplement = fsm as Fsm<T>;
        if (fsmImplement == null) {
            throw new Error('FSM is invalid.');
        }

        if (stateType == null) {
            throw new Error('State type is invalid.');
        }

        fsmImplement.changeState(stateType as TypeCtor<FsmState<T>>);
    }
}
