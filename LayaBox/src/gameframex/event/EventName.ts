/**
 * 简单字符串事件名常量。
 *
 * 结构化事件(带负载数据)请使用 `event/` 下的 XxxEventArgs 类
 * (NetworkEventArgs/StartupEventArgs 等),与 Unity GameEventArgs 对齐;
 * 本表只承载无参数的引擎层通知。
 */
export default class EventName {
    public static SocketConnected = 'SocketConnected';
    public static SocketClose = 'SocketClose';
    public static SocketError = 'SocketError';
    public static SocketReconnect = 'SocketReconnect';
}
