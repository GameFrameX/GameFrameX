import FsmBase from "./FsmBase";
import FsmState from "./FsmState";
import IFsm from "./IFsm";
import TypeCtor from "./TypeCtor";

/**
 * 有限状态机。
 *
 * 对照 Unity `GameFrameX.Fsm.Runtime.Fsm<T>` 逐方法翻译:
 * Create/Start/HasState/GetState/GetAllStates/HasData/GetData/SetData/RemoveData/
 * AddState/RemoveState/ChangeState/Update/Shutdown。
 * 与 C# 的两点必要差异:
 * 1. TS 泛型不携带运行时类型,凡 C# 以 typeof 取状态类型的入口,TS 显式传入状态构造函数;
 * 2. 无 ReferencePool,Acquire/Release 池化语义由 GC 承担。
 */
export default class Fsm<T extends object> extends FsmBase implements IFsm<T> {
    private m_Owner: T | null = null;
    private readonly m_States: Map<TypeCtor<FsmState<T>>, FsmState<T>> = new Map();
    private m_Datas: Map<string, unknown> | null = null;
    private m_CurrentState: FsmState<T> | null = null;
    private m_CurrentStateTime: number = 0;
    private m_IsDestroyed: boolean = true;

    /**
     * 获取有限状态机持有者。
     */
    public get Owner(): T {
        return this.m_Owner as T;
    }

    /**
     * 获取有限状态机持有者类型。
     */
    public get OwnerType(): TypeCtor<object> {
        return this.m_Owner!.constructor as TypeCtor<object>;
    }

    /**
     * 获取有限状态机中状态的数量。
     */
    public get FsmStateCount(): number {
        return this.m_States.size;
    }

    /**
     * 获取有限状态机是否正在运行。
     */
    public get IsRunning(): boolean {
        return this.m_CurrentState != null;
    }

    /**
     * 获取有限状态机是否被销毁。
     */
    public get IsDestroyed(): boolean {
        return this.m_IsDestroyed;
    }

    /**
     * 获取当前有限状态机状态。
     */
    public get CurrentState(): FsmState<T> | null {
        return this.m_CurrentState;
    }

    /**
     * 获取当前有限状态机状态名称。
     */
    public get CurrentStateName(): string | null {
        return this.m_CurrentState != null ? this.m_CurrentState.constructor.name : null;
    }

    /**
     * 获取当前有限状态机状态持续时间(秒)。
     */
    public get CurrentStateTime(): number {
        return this.m_CurrentStateTime;
    }

    /**
     * 创建有限状态机。
     * @param name 有限状态机名称
     * @param owner 有限状态机持有者
     * @param states 有限状态机状态集合
     * @returns 创建的有限状态机
     */
    public static Create<T extends object>(name: string, owner: T, states: FsmState<T>[]): Fsm<T> {
        if (owner == null) {
            throw new Error('FSM owner is invalid.');
        }

        if (states == null || states.length < 1) {
            throw new Error('FSM states is invalid.');
        }

        const fsm = new Fsm<T>();
        fsm.Name = name;
        fsm.m_Owner = owner;
        fsm.m_IsDestroyed = false;
        for (const state of states) {
            if (state == null) {
                throw new Error('FSM states is invalid.');
            }

            const stateType = state.constructor as TypeCtor<FsmState<T>>;
            if (fsm.m_States.has(stateType)) {
                throw new Error(`FSM '${fsm.FullName}' state '${stateType.name}' is already exist.`);
            }

            fsm.m_States.set(stateType, state);
            state.OnInit(fsm);
        }

        return fsm;
    }

    /**
     * 清理有限状态机。
     */
    public Clear(): void {
        this.m_CurrentState?.OnLeave(this, true);

        for (const state of this.m_States.values()) {
            state.OnDestroy(this);
        }

        this.Name = '';
        this.m_Owner = null;
        this.m_States.clear();
        this.m_Datas?.clear();
        this.m_Datas = null;
        this.m_CurrentState = null;
        this.m_CurrentStateTime = 0;
        this.m_IsDestroyed = true;
    }

    /**
     * 开始有限状态机。
     * @param stateType 要开始的有限状态机状态类型
     */
    public Start(stateType: TypeCtor<FsmState<T>>): void {
        if (this.IsRunning) {
            throw new Error('FSM is running, can not start again.');
        }

        const state = this.GetState(stateType);
        if (state == null) {
            throw new Error(`FSM '${this.FullName}' can not start state '${stateType.name}' which is not exist.`);
        }

        this.m_CurrentStateTime = 0;
        this.m_CurrentState = state;
        this.m_CurrentState.OnEnter(this);
    }

    /**
     * 是否存在有限状态机状态。
     * @param stateType 要检查的有限状态机状态类型
     */
    public HasState(stateType: TypeCtor<FsmState<T>>): boolean {
        return this.m_States.has(stateType);
    }

    /**
     * 获取有限状态机状态。
     * @param stateType 要获取的有限状态机状态类型
     */
    public GetState(stateType: TypeCtor<FsmState<T>>): FsmState<T> | null {
        return this.m_States.get(stateType) ?? null;
    }

    /**
     * 获取有限状态机的所有状态。
     */
    public GetAllStates(): FsmState<T>[] {
        return [...this.m_States.values()];
    }

    /**
     * 是否存在有限状态机数据(BlackBoard)。
     * @param name 有限状态机数据名称
     */
    public HasData(name: string): boolean {
        if (name == null || name === '') {
            throw new Error('Data name is invalid.');
        }

        return this.m_Datas?.has(name) ?? false;
    }

    /**
     * 获取有限状态机数据(BlackBoard)。
     * @param name 有限状态机数据名称
     */
    public GetData<TData>(name: string): TData | null {
        if (name == null || name === '') {
            throw new Error('Data name is invalid.');
        }

        return (this.m_Datas?.get(name) as TData | undefined) ?? null;
    }

    /**
     * 设置有限状态机数据(BlackBoard)。
     * @param name 有限状态机数据名称
     * @param data 要设置的数据
     */
    public SetData<TData>(name: string, data: TData): void {
        if (name == null || name === '') {
            throw new Error('Data name is invalid.');
        }

        this.m_Datas ??= new Map<string, unknown>();
        this.m_Datas.set(name, data);
    }

    /**
     * 移除有限状态机数据(BlackBoard)。
     * @param name 有限状态机数据名称
     */
    public RemoveData(name: string): boolean {
        if (name == null || name === '') {
            throw new Error('Data name is invalid.');
        }

        return this.m_Datas?.delete(name) ?? false;
    }

    /**
     * 重置有限状态机。
     */
    public Reset(): void {
        if (this.m_IsDestroyed) {
            throw new Error('FSM is destroyed, can not reset.');
        }

        this.m_CurrentState?.OnLeave(this, false);
        this.m_CurrentState = null;
        this.m_CurrentStateTime = 0;
        this.m_Datas?.clear();
    }

    /**
     * 添加有限状态机状态。
     * @param state 要添加的有限状态机状态
     */
    public AddState(state: FsmState<T>): void {
        if (this.m_IsDestroyed) {
            throw new Error('FSM is destroyed, can not add state.');
        }

        if (state == null) {
            throw new Error('State is invalid.');
        }

        const stateType = state.constructor as TypeCtor<FsmState<T>>;
        if (this.m_States.has(stateType)) {
            throw new Error(`FSM '${this.FullName}' state '${stateType.name}' is already exist.`);
        }

        this.m_States.set(stateType, state);
        state.OnInit(this);
    }

    /**
     * 移除有限状态机状态。
     * @param stateType 要移除的有限状态机状态类型
     */
    public RemoveState(stateType: TypeCtor<FsmState<T>>): boolean {
        if (this.m_IsDestroyed) {
            throw new Error('FSM is destroyed, can not remove state.');
        }

        if (stateType == null) {
            throw new Error('State type is invalid.');
        }

        const state = this.m_States.get(stateType);
        if (state == null) {
            return false;
        }

        if (this.m_CurrentState === state) {
            throw new Error(`FSM '${this.FullName}' can not remove current state '${stateType.name}'.`);
        }

        state.OnDestroy(this);
        return this.m_States.delete(stateType);
    }

    /**
     * 有限状态机轮询(由 FsmManager 调用)。
     * @param elapseSeconds 逻辑流逝时间,以秒为单位
     * @param realElapseSeconds 真实流逝时间,以秒为单位
     */
    public update(elapseSeconds: number, realElapseSeconds: number): void {
        if (this.m_CurrentState == null) {
            return;
        }

        this.m_CurrentStateTime += elapseSeconds;
        this.m_CurrentState.OnUpdate(this, elapseSeconds, realElapseSeconds);
    }

    /**
     * 有限状态机固定轮询(由 FsmManager 调用)。
     * @param elapseSeconds 逻辑流逝时间,以秒为单位
     * @param realElapseSeconds 真实流逝时间,以秒为单位
     */
    public fixedUpdate(elapseSeconds: number, realElapseSeconds: number): void {
        if (this.m_CurrentState == null) {
            return;
        }

        this.m_CurrentState.OnFixedUpdate(this, elapseSeconds, realElapseSeconds);
    }

    /**
     * 关闭并清理有限状态机。
     */
    public shutdown(): void {
        this.Clear();
    }

    /**
     * 切换当前有限状态机状态(框架内部调用,由 FsmState.ChangeState 触发)。
     * @param stateType 要切换到的有限状态机状态类型
     */
    public changeState(stateType: TypeCtor<FsmState<T>>): void {
        if (this.m_CurrentState == null) {
            throw new Error('Current state is invalid.');
        }

        const state = this.m_States.get(stateType);
        if (state == null) {
            throw new Error(`FSM '${this.FullName}' can not change state to '${stateType.name}' which is not exist.`);
        }

        this.m_CurrentState.OnLeave(this, false);
        this.m_CurrentStateTime = 0;
        this.m_CurrentState = state;
        this.m_CurrentState.OnEnter(this);
    }
}
