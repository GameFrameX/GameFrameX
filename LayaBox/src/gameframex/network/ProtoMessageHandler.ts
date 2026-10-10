import MessageObject from "./MessageObject";

type MessageCtor = new () => MessageObject;

interface PendingHandler {
    /** 装饰器捕获的 target:实例方法=原型;静态方法=构造函数。 */
    target: object;
    key: string;
    ctor: MessageCtor;
}

/**
 * Notify 消息处理器注册表。
 *
 * 对照 Unity `MessageHandlerAttribute(Type message, string invokeMethodName)` +
 * `ProtoMessageHandler.Add(instance)`:
 * - `@MessageHandler(MsgClass)` 装饰器收集 (原型/类, 方法名, 消息类);
 * - 含实例方法的宿主在构造时调用 `ProtoMessageHandler.add(this)` 完成实例级绑定
 *   (直接绑原型会丢失实例状态,故实例方法必须走 add);
 * - 静态方法处理器由 `bind()` 统一绑定;
 * - 分发按消息全名取处理器列表,方法签名 `(msg: TMsg) => void`。
 */
export default class ProtoMessageHandler {
    /** 待绑定条目(装饰器在类定义期收集,全名解析推迟到绑定)。 */
    private static readonly pending: PendingHandler[] = [];

    /** 已绑定表:消息全名 → 处理器列表。 */
    private static readonly handlers: Map<string, { target: object; key: string }[]> = new Map();

    /**
     * 消息处理器方法装饰器(仅 Notify 消息;RPC 响应走 Call 配对,不进此表)。
     * @param ctor 消息类构造函数
     * @example
     * class BagManager {
     *     @MessageHandler(Bag.NotifyBagInfoChanged)
     *     private NotifyBagInfoChanged(msg: Bag.NotifyBagInfoChanged): void { ... }
     * }
     */
    public static MessageHandler(ctor: MessageCtor): (target: object, key: string) => void {
        return MessageHandler(ctor);
    }

    /**
     * 绑定宿主实例上的全部 @MessageHandler 实例方法。
     * 对照 Unity `ProtoMessageHandler.Add(instance)`;含实例状态的管理器在构造时调用。
     * @param instance 宿主实例
     */
    public static add(instance: object): void {
        const prototype = Object.getPrototypeOf(instance);
        for (let i = this.pending.length - 1; i >= 0; i--) {
            const item = this.pending[i];
            if (item.target === prototype) {
                this.bindEntry(instance, item.key, item.ctor);
                this.pending.splice(i, 1);
            }
        }
    }

    /**
     * 绑定剩余待绑定处理器(静态方法处理器)并清空待绑定表。
     */
    public static bind(): void {
        for (const item of [...this.pending]) {
            this.bindEntry(item.target, item.key, item.ctor);
        }

        this.pending.length = 0;
    }

    /**
     * 按消息全名取处理器列表。
     * @param fullName 消息全名
     */
    public static getHandlers(fullName: string): { target: object; key: string }[] {
        return this.handlers.get(fullName) ?? [];
    }

    /**
     * 分发一条消息到全部处理器。
     * @param message 消息实例(PackageName 已就位)
     */
    public static dispatch(message: MessageObject): void {
        for (const handler of this.getHandlers(message.PackageName)) {
            (handler.target as Record<string, (msg: MessageObject) => void>)[handler.key](message);
        }
    }

    /**
     * 清空(测试用)。
     */
    public static reset(): void {
        this.pending.length = 0;
        this.handlers.clear();
    }

    /** @internal 装饰器收集入口(MessageHandler 装饰器函数使用)。 */
    public static register(target: object, key: string, ctor: MessageCtor): void {
        this.pending.push({ target, key, ctor });
    }

    private static bindEntry(target: object, key: string, ctor: MessageCtor): void {
        const fullName = new ctor().PackageName;
        const list = this.handlers.get(fullName) ?? [];
        list.push({ target, key });
        this.handlers.set(fullName, list);
    }
}

/**
 * Notify 消息处理器方法装饰器。
 *
 * 独立函数而非类静态方法别名:装饰器在类定义期被裸调用,不能依赖 `this`。
 * 语义同 `ProtoMessageHandler.MessageHandler`。
 */
export function MessageHandler(ctor: MessageCtor): (target: object, key: string) => void {
    return (target: object, key: string): void => {
        ProtoMessageHandler.register(target, key, ctor);
    };
}
