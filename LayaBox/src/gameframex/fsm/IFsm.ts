import FsmState from "./FsmState";
import TypeCtor from "./TypeCtor";

/**
 * 有限状态机接口。
 *
 * 对照 Unity `GameFrameX.Fsm.Runtime.IFsm<T>`;C# 属性在 TS 中以只读 getter 形式呈现,
 * 凡 C# 以 typeof 取状态类型的入口,TS 显式传入状态构造函数。
 */
export default interface IFsm<T extends object> {
    /** 获取有限状态机名称。 */
    readonly Name: string;

    /** 获取有限状态机完整名称。 */
    readonly FullName: string;

    /** 获取有限状态机持有者。 */
    readonly Owner: T;

    /** 获取有限状态机中状态的数量。 */
    readonly FsmStateCount: number;

    /** 获取有限状态机是否正在运行。 */
    readonly IsRunning: boolean;

    /** 获取有限状态机是否被销毁。 */
    readonly IsDestroyed: boolean;

    /** 获取当前有限状态机状态。 */
    readonly CurrentState: FsmState<T> | null;

    /** 获取当前有限状态机状态名称。 */
    readonly CurrentStateName: string | null;

    /** 获取当前有限状态机状态持续时间(秒)。 */
    readonly CurrentStateTime: number;

    /** 开始有限状态机。 */
    Start(stateType: TypeCtor<FsmState<T>>): void;

    /** 是否存在有限状态机状态。 */
    HasState(stateType: TypeCtor<FsmState<T>>): boolean;

    /** 获取有限状态机状态。 */
    GetState(stateType: TypeCtor<FsmState<T>>): FsmState<T> | null;

    /** 获取有限状态机的所有状态。 */
    GetAllStates(): FsmState<T>[];

    /** 是否存在有限状态机数据(BlackBoard)。 */
    HasData(name: string): boolean;

    /** 获取有限状态机数据(BlackBoard)。 */
    GetData<TData>(name: string): TData | null;

    /** 设置有限状态机数据(BlackBoard)。 */
    SetData<TData>(name: string, data: TData): void;

    /** 移除有限状态机数据(BlackBoard)。 */
    RemoveData(name: string): boolean;
}
