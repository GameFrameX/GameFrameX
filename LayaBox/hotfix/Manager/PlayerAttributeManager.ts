import GameApp from "../../src/gameframex/GameApp";
import EventSystems from "../../src/gameframex/event/EventSystems";
import { MessageHandler } from "../../src/gameframex/network/ProtoMessageHandler";
import ProtoMessageHandler from "../../src/gameframex/network/ProtoMessageHandler";
import { Attribute } from "../../src/gameframex/protobuf/_0310_Attribute";
import { PlayerAttributeChangedEventArgs } from "../Events/HotfixEventArgs";
import PlayerAttributeCache from "./PlayerAttributeCache";

/**
 * 客户端玩家属性编号映射。
 *
 * 对照 Unity `PlayerAttributeType`:数值与服务端 AttributeType 槽位一致。
 */
export enum PlayerAttributeType {
    Hp = 1,
    PhysAtk = 2,
    MagicAtk = 3,
    PhysDef = 4,
    MagicDef = 5,
    Crit = 6,
    CritDamage = 7,
    Accuracy = 8,
    Block = 9,
}

/**
 * 玩家属性管理器。
 *
 * 对照 Unity `Hotfix.Manager.PlayerAttributeManager`:消费服务端
 * NotifyPlayerAttributeSync(快照)/NotifyPlayerAttributeChanged(增量),派发变化事件。
 * 权威在服务端,客户端只缓存不计算。
 */
export default class PlayerAttributeManager {
    private static _instance: PlayerAttributeManager | null = null;

    /** 全局单例(构造时注册消息)。 */
    public static get Instance(): PlayerAttributeManager {
        this._instance ??= new PlayerAttributeManager();
        return this._instance;
    }

    private readonly cache: PlayerAttributeCache = new PlayerAttributeCache();

    private constructor() {
        ProtoMessageHandler.add(this);
    }

    /**
     * 缓存属性数量。
     */
    public get Count(): number {
        return this.cache.Count;
    }

    /**
     * 尝试读取属性值;未命中返回 null。
     * @param attributeId 属性 id
     */
    public TryGetValue(attributeId: number): number | null {
        return this.cache.TryGetValue(attributeId);
    }

    /**
     * 读取枚举类型属性值,未命中返回 0。
     * @param attributeType 属性类型
     */
    public GetValue(attributeType: PlayerAttributeType): number {
        return this.cache.TryGetValue(attributeType as number) ?? 0;
    }

    /**
     * 当前快照写入目标集合。
     * @param target 目标集合
     */
    public CopyTo(target: Map<number, number>): void {
        this.cache.CopyTo(target);
    }

    /**
     * 请求玩家属性快照(服务端登录后主动推送 Sync,此入口保持登录流程兼容)。
     */
    public async RequestGetPlayerAttribute(): Promise<void> {
        // 对照 Unity:直接完成
    }

    /**
     * 清空缓存(登出/切号)。
     */
    public Clear(): void {
        this.cache.Clear();
    }

    /**
     * 监听服务端属性完整快照通知(登录后或重连覆盖旧缓存)。
     */
    @MessageHandler(Attribute.NotifyPlayerAttributeSync)
    private NotifyPlayerAttributeSync(msg: Attribute.NotifyPlayerAttributeSync): void {
        const snapshot = new Map<number, number>();
        const attributes = (msg.Attributes ?? []) as unknown as ArrayLike<{ Type: number; Value: number }>;
        for (const attribute of Array.from(attributes)) {
            snapshot.set(attribute.Type, attribute.Value);
        }

        const oldSnapshot = this.cache.GetSnapshot();
        const changed = this.cache.ReplaceAll(snapshot, oldSnapshot);
        EventSystems.Fire(this, PlayerAttributeChangedEventArgs.Create(changed));
    }

    /**
     * 监听服务端属性增量变化通知。
     */
    @MessageHandler(Attribute.NotifyPlayerAttributeChanged)
    private NotifyPlayerAttributeChanged(msg: Attribute.NotifyPlayerAttributeChanged): void {
        const changes = new Map<number, number>();
        changes.set(msg.Type, msg.Value);
        const changed = this.cache.ApplyChanges(changes);
        if (changed.size <= 0) {
            // 无实际变化,不重复派发
            return;
        }

        EventSystems.Fire(this, PlayerAttributeChangedEventArgs.Create(changed));
    }
}
