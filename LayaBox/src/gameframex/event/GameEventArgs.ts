/**
 * 游戏事件参数基类。
 *
 * 对照 Unity `GameFrameX.Event.Runtime.GameEventArgs`:
 * 每个具体事件类约定 `static readonly EventId = '类名'` + 实例 `Id` 返回同值。
 */
export default abstract class GameEventArgs {
    /**
     * 事件类型唯一标识。
     */
    public abstract get Id(): string;
}
