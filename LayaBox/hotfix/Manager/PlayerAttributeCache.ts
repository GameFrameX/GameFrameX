/**
 * 玩家属性缓存。
 *
 * 对照 Unity `Hotfix.Manager.PlayerAttributeCache`:attributeId → value 字典,
 * 支持快照覆盖与增量应用,返回实际变化集(值未变不计入)。
 */
export default class PlayerAttributeCache {
    private readonly values: Map<number, number> = new Map();

    /**
     * 缓存属性数量。
     */
    public get Count(): number {
        return this.values.size;
    }

    /**
     * 尝试读取属性值。
     * @param attributeId 属性 id
     */
    public TryGetValue(attributeId: number): number | null {
        return this.values.get(attributeId) ?? null;
    }

    /**
     * 当前快照副本。
     */
    public GetSnapshot(): Map<number, number> {
        return new Map(this.values);
    }

    /**
     * 快照整体覆盖;返回与旧快照相比的变化集。
     * @param snapshot 新快照
     * @param oldSnapshot 覆盖前旧快照
     */
    public ReplaceAll(snapshot: Map<number, number>, oldSnapshot: Map<number, number>): Map<number, number> {
        this.values.clear();
        const changed = new Map<number, number>();
        for (const [id, value] of snapshot) {
            this.values.set(id, value);
            if (oldSnapshot.get(id) !== value) {
                changed.set(id, value);
            }
        }

        return changed;
    }

    /**
     * 应用增量;返回实际变化集(值未变不计入)。
     * @param changes 增量:attributeId → value
     */
    public ApplyChanges(changes: Map<number, number>): Map<number, number> {
        const changed = new Map<number, number>();
        for (const [id, value] of changes) {
            if (this.values.get(id) !== value) {
                this.values.set(id, value);
                changed.set(id, value);
            }
        }

        return changed;
    }

    /**
     * 快照写入目标集合。
     * @param target 目标集合
     */
    public CopyTo(target: Map<number, number>): void {
        for (const [id, value] of this.values) {
            target.set(id, value);
        }
    }

    /**
     * 清空。
     */
    public Clear(): void {
        this.values.clear();
    }
}
