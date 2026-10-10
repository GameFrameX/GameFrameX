import FsmManager from "../fsm/FsmManager";
import GameEntry from "../GameEntry";

/**
 * Fsm 域注册:创建 FsmManager 并注册进 GameEntry。
 * @returns Fsm 管理器实例(GameApp.Fsm)
 */
export default function FsmDomain(): FsmManager {
    const manager = new FsmManager();
    GameEntry.registerModule('Fsm', manager);
    return manager;
}
