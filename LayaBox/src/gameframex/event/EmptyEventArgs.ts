import GameEventArgs from "./GameEventArgs";

/**
 * 无负载事件参数。
 *
 * 对照 Unity `GameFrameX.Event.Runtime.EmptyEventArgs`:
 * 字符串事件标识的 Fire 重载以本类装箱,负载即事件标识本身。
 */
export class EmptyEventArgs extends GameEventArgs {
    /**
     * 创建无负载事件参数。
     * @param id 事件类型标识
     */
    public static Create(id: string): EmptyEventArgs {
        const args = new EmptyEventArgs();
        args.EventId = id;
        return args;
    }

    private EventId!: string;

    public get Id(): string {
        return this.EventId;
    }
}
