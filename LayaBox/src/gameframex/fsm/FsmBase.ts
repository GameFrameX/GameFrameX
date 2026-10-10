import TypeCtor from "./TypeCtor";

/**
 * 有限状态机基类。
 *
 * 对照 Unity `GameFrameX.Fsm.Runtime.FsmBase` 同名成员:
 * Name/FullName/OwnerType/FsmStateCount/IsRunning/IsDestroyed/CurrentStateName/CurrentStateTime。
 * TS 无运行时 Type,OwnerType 以构造函数代替。
 */
export default abstract class FsmBase {
    private m_Name: string = '';

    /**
     * 获取有限状态机名称。
     */
    public get Name(): string {
        return this.m_Name;
    }
    protected set Name(value: string) {
        this.m_Name = value ?? '';
    }

    /**
     * 获取有限状态机完整名称(持有者类型名 + 名称)。
     */
    public get FullName(): string {
        return `${this.OwnerType.name}|${this.m_Name}`;
    }

    /**
     * 获取有限状态机持有者类型(构造函数)。
     */
    public abstract get OwnerType(): TypeCtor<object>;

    /**
     * 获取有限状态机中状态的数量。
     */
    public abstract get FsmStateCount(): number;

    /**
     * 获取有限状态机是否正在运行。
     */
    public abstract get IsRunning(): boolean;

    /**
     * 获取有限状态机是否被销毁。
     */
    public abstract get IsDestroyed(): boolean;

    /**
     * 获取当前有限状态机状态名称。
     */
    public abstract get CurrentStateName(): string | null;

    /**
     * 获取当前有限状态机状态持续时间(秒)。
     */
    public abstract get CurrentStateTime(): number;

    /**
     * 有限状态机轮询。由 FsmManager 每帧调用。
     * @param elapseSeconds 逻辑流逝时间,以秒为单位
     * @param realElapseSeconds 真实流逝时间,以秒为单位
     */
    public abstract update(elapseSeconds: number, realElapseSeconds: number): void;

    /**
     * 关闭并清理有限状态机。
     */
    public abstract shutdown(): void;
}
