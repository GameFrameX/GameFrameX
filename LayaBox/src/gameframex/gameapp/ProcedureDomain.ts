import ProcedureManager from "../procedure/ProcedureManager";
import GameEntry from "../GameEntry";

/**
 * Procedure 域注册:创建 ProcedureManager 并注册进 GameEntry。
 * 流程集合由启动层(StartupRunner)通过 Initialize 注入。
 * @returns 流程管理器实例(GameApp.Procedure)
 */
export default function ProcedureDomain(): ProcedureManager {
    const manager = new ProcedureManager();
    GameEntry.registerModule('Procedure', manager);
    return manager;
}
