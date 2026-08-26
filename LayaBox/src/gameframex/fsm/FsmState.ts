/* import { IFsm } from "./IFsm";

// namespace GameFrameX {
export abstract class FsmState<T> {

    constructor() { }

    protected internalOnInit(fsm: IFsm<T>) { }

    protected internalOnEnter(fsm: IFsm<T>) { }

    protected internalOnUpdate(fsm: IFsm<T>, elapseSeconds: number, realElapseSeconds: number) { }

    protected internalOnLeave(fsm: IFsm<T>, isShutdown: boolean) { }

    protected internalOnDestroy(fsm: IFsm<T>) { }

    public changeToState<TState extends FsmState<T>>(fsm: IFsm<T>) {
        let fsmImplement = <Fsm<T>>fsm;
        if (fsmImplement === null || fsmImplement === undefined) {
            throw new Error("FSM is invalid.");
        }

        fsmImplement.changeState<TState>();
    }

    protected changeState<TState extends FsmState<T>>(fsm: IFsm<T>) {
        let fsmImplement = <Fsm<T>>fsm;
        if (fsmImplement === null || fsmImplement === undefined) {
            throw new Error("FSM is invalid.");
        }

        fsmImplement.changeState<TState>();
    }

    protected changeState(fsm: IFsm<T>, stateType: { new(): T }) {
        let fsmImplement = <Fsm<T>>fsm;
        if (fsmImplement === null || fsmImplement === undefined) {
            throw new Error("FSM is invalid.");
        }

        fsmImplement.changeState(stateType);
    }

}
// } */