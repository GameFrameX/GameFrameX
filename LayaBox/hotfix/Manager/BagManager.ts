import GameApp from "../../src/gameframex/GameApp";
import EventSystems from "../../src/gameframex/event/EventSystems";
import LogHelper from "../../src/gameframex/log/LogHelper";
import { MessageHandler } from "../../src/gameframex/network/ProtoMessageHandler";
import ProtoMessageHandler from "../../src/gameframex/network/ProtoMessageHandler";
import { Bag } from "../../src/gameframex/protobuf/_0100_Bag";
import { BagChangedEventArgs } from "../Events/HotfixEventArgs";

/** 背包道具(对照 Unity BagItem)。 */
export class BagItem {
    public ItemId: number = 0;
    public Count: number = 0;
}

/**
 * 背包管理器。
 *
 * 对照 Unity `Hotfix.Manager.BagManager`:NotifyBagInfoChanged 增量同步 +
 * ReqBagInfo 全量拉取 + ReqUseItem 使用道具;变化发 BagChangedEventArgs。
 * 注:protobufjs 解码 map 字段为普通对象,统一按 Record 遍历。
 */
export default class BagManager {
    private static _instance: BagManager | null = null;

    /** 全局单例(构造时注册消息,对照 Unity 单例自动 Register)。 */
    public static get Instance(): BagManager {
        this._instance ??= new BagManager();
        return this._instance;
    }

    private readonly itemDic: Map<number, BagItem> = new Map();

    private constructor() {
        ProtoMessageHandler.add(this);
    }

    /**
     * 全部道具。
     */
    public GetItems(): BagItem[] {
        return [...this.itemDic.values()];
    }

    /**
     * 请求背包信息。
     */
    public async RequestGetBagInfo(): Promise<void> {
        const resp = await GameApp.Network.GetNetworkChannel('network').Call<Bag.RespBagInfo>(new Bag.ReqBagInfo());
        if (resp.ErrorCode !== 0) {
            LogHelper.log(`请求背包失败:${resp.ErrorCode}`);
            return;
        }

        const itemDic = (resp.ItemDic ?? {}) as unknown as Record<string, number>;
        for (const [key, count] of Object.entries(itemDic)) {
            const itemId = Number(key);
            this.itemDic.set(itemId, { ItemId: itemId, Count: Number(count) });
        }
    }

    /**
     * 请求使用道具。
     * @param itemId 道具 ID
     * @param count 数量
     */
    public async RequestUseItem(itemId: number, count: number = 1): Promise<void> {
        const request = new Bag.ReqUseItem();
        request.ItemId = itemId;
        request.Count = count;
        const resp = await GameApp.Network.GetNetworkChannel('network').Call<Bag.RespUseItem>(request);
        if (resp.ErrorCode !== 0) {
            LogHelper.log(`使用道具失败:${resp.ErrorCode}`);
            return;
        }

        const value = this.itemDic.get(resp.ItemId);
        if (value != null) {
            value.Count -= resp.Count;
            if (value.Count <= 0) {
                this.itemDic.delete(resp.ItemId);
            }
        }

        EventSystems.Fire(this, BagChangedEventArgs.Create());
    }

    /**
     * 监听道具变化通知。
     */
    @MessageHandler(Bag.NotifyBagInfoChanged)
    private NotifyBagInfoChanged(msg: Bag.NotifyBagInfoChanged): void {
        const entries = (msg.ItemDic ?? {}) as unknown as Record<string, Bag.NotifyBagItem>;
        for (const [key, info] of Object.entries(entries)) {
            const itemId = Number(key);
            const existing = this.itemDic.get(itemId);
            if (existing != null) {
                existing.Count = info.Count;
                if (existing.Count <= 0) {
                    this.itemDic.delete(itemId);
                }
            } else {
                this.itemDic.set(itemId, { ItemId: itemId, Count: info.Count });
            }
        }

        EventSystems.Fire(this, BagChangedEventArgs.Create());
    }
}
