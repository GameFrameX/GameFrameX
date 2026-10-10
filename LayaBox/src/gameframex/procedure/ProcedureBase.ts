import FsmState from "../fsm/FsmState";
import type IProcedureManager from "./IProcedureManager";

/**
 * 流程基类。
 *
 * 对照 Unity `GameFrameX.Procedure.Runtime.ProcedureBase`:
 * 以 IProcedureManager 为持有者的 FsmState;生命周期钩子 OnInit/OnEnter/OnUpdate/OnLeave/OnDestroy
 * 继承自 FsmState,同名同参,此处不重复声明。
 */
export default abstract class ProcedureBase extends FsmState<IProcedureManager> {
}
