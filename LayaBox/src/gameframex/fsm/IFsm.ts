// namespace GameFrameX.Fsm {
    /**
     * 有限状态机接口。
     *
     * @export
     * @interface IFsm
     * @template T
     */
    export interface IFsm<T> {
        /**
         * 获取有限状态机名称。
         *
         * @type {string}
         * @memberof IFsm
         */
        readonly name: string,

        /**
         * 获取有限状态机完整名称。
         *
         * @type {string}
         * @memberof IFsm
         */
        readonly fullName: string,

        /**
         * 获取有限状态机持有者。
         *
         * @type {T}
         * @memberof IFsm
         */
        readonly owner: T,

        /**
         * 获取有限状态机中状态的数量。
         *
         * @type {number}
         * @memberof IFsm
         */
        readonly fsmStateCount: number,

        /**
         * 获取有限状态机是否正在运行。
         *
         * @type {boolean}
         * @memberof IFsm
         */
        readonly isRunning: boolean,

        /**
         * 获取有限状态机是否被销毁。
         *
         * @type {boolean}
         * @memberof IFsm
         */
        readonly isDestroyed: boolean,
        
        // Here are TypeScript doesn't support property parameters, so I'd change these to functions.
      /*   start<TState extends FsmState<T>>(state?: new () => TState): void;
        hasState<TState extends FsmState<T>>(): boolean;
        getState<TState extends FsmState<T>>(): TState; */

        // ... and so on for the rest of the methods.
        // Please change it according to methods you will use
    }
// }