// namespace GameFrameX.Fsm {
    /**
     * 有限状态机基类。
     */
    abstract class FsmBase {
        private m_Name: string;

        /**
         * 初始化有限状态机基类的新实例。
         */
        constructor() {
            this.m_Name = '';
        }

        /**
         * 获取有限状态机名称。
         */
        get Name(): string {
            return this.m_Name;
        }

        protected set Name(value: string) {
            this.m_Name = value || '';
        }

        /**
         * 获取有限状态机完整名称。
         */
        get FullName(): string {
            return this.OwnerType + '.' + this.m_Name;
        }

        /**
         * 获取有限状态机持有者类型。
         */
        abstract get OwnerType(): any; // 使用实际类型替换 any

        /**
         * 获取有限状态机中状态的数量。
         */
        abstract get FsmStateCount(): number;

        /**
         * 获取有限状态机是否正在运行。
         */
        abstract get IsRunning(): boolean;

        /**
         * 获取有限状态机是否被销毁。
         */
        abstract get IsDestroyed(): boolean;

        /**
         * 获取当前有限状态机状态名称。
         */
        abstract get CurrentStateName(): string;

        /**
         * 获取当前有限状态机状态持续时间。
         */
        abstract get CurrentStateTime(): number;

        /**
         * 有限状态机轮询。
         */
        abstract Update(elapseSeconds: number, realElapseSeconds: number): void;

        /**
         * 关闭并清理有限状态机。
         */
        abstract Shutdown(): void;
    }
// }