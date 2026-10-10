import ProcedureBase from "../../../gameframex/procedure/ProcedureBase";
import type IFsm from "../../../gameframex/fsm/IFsm";
import type IProcedureManager from "../../../gameframex/procedure/IProcedureManager";
import StartupContext from "./StartupContext";
import ProcedureUpdateStaticVersion from "./ProcedureUpdateStaticVersion";
import type ResourceVersionSystem from "../startup/ResourceVersionSystem";

/**
 * 补丁初始化。
 *
 * 对照 Unity `ProcedurePatchInit`:确定包地址后初始化资源系统进入静态版本流程。
 * Laya 差异:本地模式(remoteRoot 为空)直接进静态版本流程(其内部走本地快捷路径)。
 */
export default class ProcedurePatchInit extends ProcedureBase {
    /** 资源系统由 Runner 注入(StartupResourceKeys.ResourceSystem),本流程只做阶段判定。 */
    public static resourceSystemFactory: (() => ResourceVersionSystem) | null = null;

    /**
     * 进入状态。
     * @param fsm 流程 FSM
     */
    public OnEnter(fsm: IFsm<IProcedureManager>): void {
        void this.enterAsync(fsm);
    }

    private async enterAsync(fsm: IFsm<IProcedureManager>): Promise<void> {
        const context = new StartupContext(fsm);
        if (context.resourceSystem == null && ProcedurePatchInit.resourceSystemFactory != null) {
            fsm.SetData('__startup_resource_system__', ProcedurePatchInit.resourceSystemFactory());
        }

        this.ChangeToState(fsm, ProcedureUpdateStaticVersion);
    }
}
