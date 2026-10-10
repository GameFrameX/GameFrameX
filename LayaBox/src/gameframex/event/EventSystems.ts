import type GameEventArgs from "./GameEventArgs";
import { EmptyEventArgs } from "./EmptyEventArgs";
import IModule from "../IModule";

type EventHandler<E extends GameEventArgs = GameEventArgs> = (sender: unknown, args: E) => void;

interface QueuedEvent {
    id: string;
    sender: unknown;
    args: GameEventArgs;
}

/**
 * 事件系统。
 *
 * 对照 Unity `GameFrameX.Event.Runtime.EventComponent/EventManager` 语义:
 * Subscribe/Unsubscribe/Check/Count 管理订阅;Fire 入队、下一帧(update)统一分发;
 * FireNow 立即分发。不依赖引擎事件派发器,可在任意宿主(含单测)运行。
 * 注册进 GameEntry 后由主循环驱动 update flush 队列。
 */
export default class EventSystems implements IModule {
    private static _instance: EventSystems | null = null;

    /** 事件处理函数表:id → 有序处理函数列表(同 id 保持订阅顺序)。 */
    private readonly handlers: Map<string, EventHandler[]> = new Map();

    /** 待分发事件队列(Fire 入队,update 出队)。 */
    private readonly queue: QueuedEvent[] = [];

    /**
     * 全局单例。GameApp.Event 域与存量调用方都走它。
     */
    public static get instance(): EventSystems {
        this._instance ??= new EventSystems();
        return this._instance;
    }

    /**
     * 检查事件处理函数是否存在。
     * @param id 事件类型标识
     * @param handler 处理函数
     */
    public static Check(id: string, handler: EventHandler): boolean {
        return this.instance.handlers.get(id)?.includes(handler) ?? false;
    }

    /**
     * 订阅事件。
     * @param id 事件类型标识
     * @param handler 处理函数
     */
    public static Subscribe(id: string, handler: EventHandler): void {
        const list = this.instance.handlers.get(id);
        if (list == null) {
            this.instance.handlers.set(id, [handler]);
            return;
        }

        if (list.includes(handler)) {
            throw new Error(`Event handler is already subscribed to '${id}'.`);
        }

        list.push(handler);
    }

    /**
     * 订阅事件(不存在才订阅)。
     * @param id 事件类型标识
     * @param handler 处理函数
     */
    public static CheckSubscribe(id: string, handler: EventHandler): void {
        if (!this.Check(id, handler)) {
            this.Subscribe(id, handler);
        }
    }

    /**
     * 退订事件。
     * @param id 事件类型标识
     * @param handler 处理函数
     */
    public static Unsubscribe(id: string, handler: EventHandler): void {
        const list = this.instance.handlers.get(id);
        if (list == null) {
            return;
        }

        const index = list.indexOf(handler);
        if (index >= 0) {
            list.splice(index, 1);
        }

        if (list.length === 0) {
            this.instance.handlers.delete(id);
        }
    }

    /**
     * 退订事件(存在才退订)。
     * @param id 事件类型标识
     * @param handler 处理函数
     */
    public static CheckUnsubscribe(id: string, handler: EventHandler): void {
        this.Unsubscribe(id, handler);
    }

    /**
     * 发布事件:入队,下一帧统一分发。
     * 字符串重载对照 Unity `Fire(object sender, string eventId)`:以 EmptyEventArgs 装箱。
     * @param sender 事件发起者
     * @param args 事件参数或事件类型标识
     */
    public static Fire(sender: unknown, args: GameEventArgs | string): void {
        const id = typeof args === 'string' ? args : args.Id;
        const payload = typeof args === 'string' ? EmptyEventArgs.Create(args) : args;
        this.instance.queue.push({ id, sender, args: payload });
    }

    /**
     * 发布事件:立即同步分发。
     * @param sender 事件发起者
     * @param args 事件参数或事件类型标识
     */
    public static FireNow(sender: unknown, args: GameEventArgs | string): void {
        if (typeof args === 'string') {
            this.instance.dispatch(args, sender, EmptyEventArgs.Create(args));
            return;
        }

        this.instance.dispatch(args.Id, sender, args);
    }

    /**
     * 指定事件的订阅数量。
     * @param id 事件类型标识
     */
    public static Count(id: string): number {
        return this.instance.handlers.get(id)?.length ?? 0;
    }

    private dispatch(id: string, sender: unknown, args: GameEventArgs): void {
        const list = this.handlers.get(id);
        if (list == null || list.length === 0) {
            return;
        }

        // 快照遍历:允许处理函数内安全退订/新增订阅。
        for (const handler of [...list]) {
            if (!this.handlers.get(id)?.includes(handler)) {
                continue;
            }

            handler(sender, args);
        }
    }

    /**
     * 模块初始化(由 GameEntry 调用)。
     */
    public async init(): Promise<void> { }

    /**
     * 模块轮询:flush 待分发事件队列(由 GameEntry 每帧调用)。
     */
    public update(_elapseSeconds: number, _realElapseSeconds: number): void {
        if (this.queue.length === 0) {
            return;
        }

        const pending = [...this.queue];
        this.queue.length = 0;
        for (const item of pending) {
            this.dispatch(item.id, item.sender, item.args);
        }
    }

    /**
     * 模块关闭:清空订阅与队列。
     */
    public shutdown(): void {
        this.handlers.clear();
        this.queue.length = 0;
    }
}
