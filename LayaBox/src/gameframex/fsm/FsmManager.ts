import FsmBase from "./FsmBase";
import FsmState from "./FsmState";
import Fsm from "./Fsm";
import IFsm from "./IFsm";
import IFsmManager from "./IFsmManager";
import IModule from "../IModule";
import TypeCtor from "./TypeCtor";

/**
 * 有限状态机管理器。
 *
 * 对照 Unity `GameFrameX.Fsm.Runtime.FsmManager`:
 * 以 `持有者类型|名称` 复合键(等价 TypeNamePair)管理全部 Fsm;
 * 轮询采用快照遍历,允许遍历期间销毁状态机;
 * 注册进 GameEntry 后由主循环驱动 update。
 */
export default class FsmManager implements IFsmManager, IModule {
    private readonly m_Fsms: Map<string, FsmBase> = new Map();

    /**
     * 获取有限状态机数量。
     */
    public get Count(): number {
        return this.m_Fsms.size;
    }

    /**
     * 模块初始化(由 GameEntry 调用)。
     */
    public async init(): Promise<void> { }

    /**
     * 模块轮询(由 GameEntry 每帧调用)。
     * @param elapseSeconds 逻辑流逝时间,以秒为单位
     * @param realElapseSeconds 真实流逝时间,以秒为单位
     */
    public update(elapseSeconds: number, realElapseSeconds: number): void {
        if (this.m_Fsms.size <= 0) {
            return;
        }

        const snapshot = [...this.m_Fsms.values()];
        for (const fsm of snapshot) {
            if (fsm.IsDestroyed) {
                continue;
            }

            fsm.update(elapseSeconds, realElapseSeconds);
        }
    }

    /**
     * 关闭并清理管理器(由 GameEntry 调用)。
     */
    public shutdown(): void {
        for (const fsm of this.m_Fsms.values()) {
            fsm.shutdown();
        }

        this.m_Fsms.clear();
    }

    /**
     * 检查是否存在有限状态机。
     * @param ownerType 有限状态机持有者类型
     * @param name 有限状态机名称
     */
    public HasFsm<T extends object>(ownerType: TypeCtor<T>, name: string = ''): boolean {
        return this.m_Fsms.has(this.fsmKey(ownerType, name));
    }

    /**
     * 获取有限状态机。
     * @param ownerType 有限状态机持有者类型
     * @param name 有限状态机名称
     */
    public GetFsm<T extends object>(ownerType: TypeCtor<T>, name: string = ''): IFsm<T> | null {
        return (this.m_Fsms.get(this.fsmKey(ownerType, name)) as Fsm<T> | undefined) ?? null;
    }

    /**
     * 获取所有有限状态机。
     */
    public GetAllFsms(): FsmBase[] {
        return [...this.m_Fsms.values()];
    }

    /**
     * 创建有限状态机。
     * @param name 有限状态机名称
     * @param owner 有限状态机持有者
     * @param states 有限状态机状态集合
     */
    public CreateFsm<T extends object>(name: string, owner: T, states: FsmState<T>[]): IFsm<T> {
        const key = this.fsmKey(owner.constructor as TypeCtor<T>, name);
        if (this.m_Fsms.has(key)) {
            throw new Error(`FSM '${key}' is already exist.`);
        }

        const fsm = Fsm.Create(name, owner, states);
        this.m_Fsms.set(key, fsm);
        return fsm;
    }

    /**
     * 销毁有限状态机。
     * @param ownerType 有限状态机持有者类型
     * @param name 有限状态机名称
     */
    public DestroyFsm<T extends object>(ownerType: TypeCtor<T>, name: string = ''): boolean {
        const key = this.fsmKey(ownerType, name);
        const fsm = this.m_Fsms.get(key);
        if (fsm == null) {
            return false;
        }

        fsm.shutdown();
        return this.m_Fsms.delete(key);
    }

    /**
     * 销毁指定有限状态机实例。
     * @param fsm 要销毁的有限状态机
     */
    public DestroyFsmByInstance(fsm: FsmBase): boolean {
        const key = this.fsmKey(fsm.OwnerType as TypeCtor<object>, fsm.Name);
        const target = this.m_Fsms.get(key);
        if (target == null) {
            return false;
        }

        target.shutdown();
        return this.m_Fsms.delete(key);
    }

    private fsmKey<T extends object>(ownerType: TypeCtor<T>, name: string): string {
        return `${ownerType.name}|${name ?? ''}`;
    }
}
