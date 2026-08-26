/* import { FsmState } from "./FsmState";
import { IFsm } from "./IFsm";


export class Fsm<T> extends FsmBase implements IFsm<T> {
    private m_Owner: T;
    private m_States: Map<any, FsmState<T>>;
    private m_Datas: Map<string, any>;
    private m_CurrentState: FsmState<T>;
    private m_CurrentStateTime: number;
    private m_IsDestroyed: boolean;

    public constructor() {
        super();
        this.m_Owner = null;
        this.m_States = new Map<any, FsmState<T>>();
        this.m_Datas = null;
        this.m_CurrentState = null;
        this.m_CurrentStateTime = 0;
        this.m_IsDestroyed = true;
    }

    public get Owner(): T {
        return this.m_Owner;
    }

    public get OwnerType(): Type {
        return typeof T;
    }

    public get FsmStateCount(): number {
        return this.m_States.size;
    }

    public get IsRunning(): boolean {
        return this.m_CurrentState !== null;
    }

    public get IsDestroyed(): boolean {
        return this.m_IsDestroyed;
    }

    public get CurrentState(): FsmState<T> {
        return this.m_CurrentState;
    }

    public get CurrentStateName(): string {
        return this.m_CurrentState !== null ? this.m_CurrentState.GetType().FullName : null;
    }

    public get CurrentStateTime(): number {
        return this.m_CurrentStateTime;
    }

    public static Create<TState>(name: string, owner: T, ...states: FsmState<T>[]): Fsm<T> where TState : FsmState<T> {
        if (owner === null) {
            throw new GameFrameworkException('FSM owner is invalid.');
        }

        if (states === null || states.length < 1) {
            throw new GameFrameworkException('FSM states is invalid.');
        }

        const fsm = ReferencePool.Acquire<Fsm<T>>();
        fsm.Name = name;
        fsm.m_Owner = owner;
        fsm.m_IsDestroyed = false;

        for (const state of states) {
            if (state === null) {
                throw new GameFrameworkException('FSM states is invalid.');
            }

            const stateType = state.GetType();
            if (fsm.m_States.ContainsKey(stateType)) {
                throw new GameFrameworkException(Utility.Text.Format(`FSM '${new TypeNamePair<typeof T>(name)}' state '${stateType.FullName}' is already exist.`));
            }

            fsm.m_States.Add(stateType, state);
            state.OnInit(fsm);
        }

        return fsm;
    }

    public static Create<TState>(name: string, owner: T, states: List<FsmState<T>>): Fsm<T> where TState : FsmState<T> {
        ... // Similar to above Create method.
    }

    public Clear(): void { 
        ... // Similar to C# implementation. Remember to replace foreach loop with for-of loop in TypeScript
    } 

    public Start<TState>(): void where TState : FsmState<T> {
        ... // Similar to C# implementation.
    }

    public Start(stateType: Type): void {
        ... // Similar to C# implementation.
    }

    public HasState<TState>(): boolean where TState : FsmState<T> {
        return this.m_States.ContainsKey(typeof TState);
    }

    public HasState(stateType: Type): boolean {
        ... // Similar to C# implementation.
    }

    public GetState<TState>(): TState where TState : FsmState<T> {
        ... // Similar to C# implementation.
    }

    public GetState(stateType: Type): FsmState<T> {
        ... // Similar to C# implementation.
    }

    public GetAllStates(): FsmState<T>[] {
        ... // Similar to C# implementation.
    }

    public GetAllStates(results: List<FsmState<T>>): void {
        ... // Similar to C# implementation.
    }

    public HasData(name: string): boolean {
        ... // Similar to C# implementation.
    }

    public GetData<TData>(name: string): TData where TData : Variable {
        return this.GetData(name) as TData;
    }

    public GetData(name: string): Variable {
        ... // Similar to C# implementation.
    }

    public SetData<TData>(name: string, data: TData): void where TData : Variable {
        this.SetData(name, data as Variable);
    }

    public SetData(name: string, data: Variable): void {
        ... // Similar to C# implementation.
    }

    public RemoveData(name: string): boolean {
        ... // Similar to C# implementation.
    }

    public  override Update(elapseSeconds: number, realElapseSeconds: number): void {
        ... // Similar to C# implementation.
    }

    public  override Shutdown(): void {
        // ReferencePool.Release(this);
    }

    public ChangeState<TState>(): void where TState : FsmState<T> {
        this.ChangeState(typeof TState);
    }

    public ChangeState(stateType: Type): void {
        if (this. m_CurrentState == null)
        {
            throw new GameFrameworkException("Current state is invalid.");
        }

        let state =this.GetState(stateType);
        if (state == null)
        {
            throw new GameFrameworkException(Utility.Text.Format("FSM '{0}' can not change state to '{1}' which is not exist.", new TypeNamePair(typeof(T), Name), stateType.FullName));
        }

      this. m_CurrentState.OnLeave(this, false);
      this. m_CurrentStateTime = 0f;
      this. m_CurrentState = state;
      this. m_CurrentState.OnEnter(this);
    }
} */