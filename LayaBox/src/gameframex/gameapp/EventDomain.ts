import EventSystems from "../event/EventSystems";
import GameEntry from "../GameEntry";

/**
 * Event 域注册:复用 EventSystems 单例并注册进 GameEntry(驱动队列 flush)。
 * @returns 事件系统实例(GameApp.Event)
 */
export default function EventDomain(): EventSystems {
    const manager = EventSystems.instance;
    GameEntry.registerModule('Event', manager);
    return manager;
}
