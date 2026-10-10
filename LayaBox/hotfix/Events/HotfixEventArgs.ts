import GameEventArgs from "../../src/gameframex/event/GameEventArgs";

/**
 * 背包变化事件(对照 Unity Hotfix.Events.BagChangedEventArgs)。
 */
export class BagChangedEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'BagChangedEventArgs';

    public static Create(): BagChangedEventArgs {
        return new BagChangedEventArgs();
    }

    public get Id(): string {
        return BagChangedEventArgs.EventId;
    }
}

/**
 * 房间变化事件(对照 Unity RoomChangedEventArgs)。
 */
export class RoomChangedEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'RoomChangedEventArgs';

    public static Create(): RoomChangedEventArgs {
        return new RoomChangedEventArgs();
    }

    public get Id(): string {
        return RoomChangedEventArgs.EventId;
    }
}

/**
 * 玩家属性变化事件(对照 Unity PlayerAttributeChangedEventArgs,负载为变化集)。
 */
export class PlayerAttributeChangedEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'PlayerAttributeChangedEventArgs';

    public static Create(changed: Map<number, number>): PlayerAttributeChangedEventArgs {
        const args = new PlayerAttributeChangedEventArgs();
        args.Changed = changed;
        return args;
    }

    public get Id(): string {
        return PlayerAttributeChangedEventArgs.EventId;
    }

    /** 本次发生变化的属性:attributeId → 新值。 */
    public Changed!: Map<number, number>;
}

/**
 * 猜拳玩法数据变化事件(对照 Unity RockPaperScissorsGameChangedEventArgs)。
 */
export class RockPaperScissorsGameChangedEventArgs extends GameEventArgs {
    public static readonly EventId: string = 'RockPaperScissorsGameChangedEventArgs';

    public static Create(): RockPaperScissorsGameChangedEventArgs {
        return new RockPaperScissorsGameChangedEventArgs();
    }

    public get Id(): string {
        return RockPaperScissorsGameChangedEventArgs.EventId;
    }
}
