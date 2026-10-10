import type IFsmManager from "../fsm/IFsmManager";
import ProcedureBase from "./ProcedureBase";
import TypeCtor from "../fsm/TypeCtor";

/**
 * 流程管理器接口。
 *
 * 对照 Unity `GameFrameX.Procedure.Runtime.IProcedureManager`。
 */
export default interface IProcedureManager {
    /** 获取当前流程。 */
    readonly CurrentProcedure: ProcedureBase | null;

    /** 获取当前流程持续时间(秒)。 */
    readonly CurrentProcedureTime: number;

    /** 初始化流程管理器。 */
    Initialize(fsmManager: IFsmManager, procedures: ProcedureBase[]): void;

    /** 开始流程。 */
    StartProcedure(procedureType: TypeCtor<ProcedureBase>): void;

    /** 是否存在流程。 */
    HasProcedure(procedureType: TypeCtor<ProcedureBase>): boolean;

    /** 获取流程。 */
    GetProcedure(procedureType: TypeCtor<ProcedureBase>): ProcedureBase | null;

    /** 销毁当前流程状态机并清空注册表。 */
    DestroyProcedures(): void;

    /** 销毁后以新的流程集合重建。 */
    ReinitializeProcedures(procedures: ProcedureBase[]): void;
}
