import FsmBase from "./FsmBase";
import FsmState from "./FsmState";
import Fsm from "./Fsm";
import IFsm from "./IFsm";
import TypeCtor from "./TypeCtor";

/**
 * 有限状态机管理器接口。
 *
 * 对照 Unity `GameFrameX.Fsm.Runtime.IFsmManager`。
 */
export default interface IFsmManager {
    /** 获取有限状态机数量。 */
    readonly Count: number;

    /** 检查是否存在有限状态机。 */
    HasFsm<T extends object>(ownerType: TypeCtor<T>, name?: string): boolean;

    /** 获取有限状态机。 */
    GetFsm<T extends object>(ownerType: TypeCtor<T>, name?: string): IFsm<T> | null;

    /** 获取所有有限状态机。 */
    GetAllFsms(): FsmBase[];

    /** 创建有限状态机。 */
    CreateFsm<T extends object>(name: string, owner: T, states: FsmState<T>[]): IFsm<T>;

    /** 销毁指定有限状态机实例。 */
    DestroyFsmByInstance(fsm: FsmBase): boolean;
    DestroyFsm<T extends object>(ownerType: TypeCtor<T>, name?: string): boolean;

    /** 销毁指定有限状态机实例。 */
    DestroyFsmByInstance<T extends object>(fsm: Fsm<T>): boolean;
}
