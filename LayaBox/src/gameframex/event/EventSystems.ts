/**
 * 事件系统
 */
export default class EventSystems {

    private static eventDisptcher: Laya.EventDispatcher = new Laya.EventDispatcher();
    /**
     * 添加事件
     * eventType 通知类型
     * callback  回调方法 回调参数为event.data
     * thisObject  发起监听的对象
     */
    public static add(eventType: string, caller: any, callback: Function): void {
        this.eventDisptcher.on(eventType, caller, callback);
    }

    /**
     * 添加事件(仅执行一次)
     * eventType 通知类型
     * callback  回调方法
     * caller  发起监听的对象
     */
    public static once(eventType: string, caller: any, callback: Function): void {
        this.eventDisptcher.once(eventType, caller, callback);
    }

    /**
     * 移除事件
     * eventType 通知类型
     * callback  回调方法
     * caller  发起监听的对象
     */
    public static remove(eventType: string, caller: any, callback: Function): void {
        this.eventDisptcher.off(eventType, caller, callback);
    }

    /**
     * 发送事件
     * eventType 通知类型
     * data     参数
     */
    public static dispatch(eventType: string, data?: any): void {
        this.eventDisptcher.event(eventType, data);
    }
    /**
     * 判断是否该事件是否已存在，true：已存在
     * eventType 通知类型
     */
    public static has(eventType: string): boolean {
        return this.eventDisptcher.hasListener(eventType);
    }
}