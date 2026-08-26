using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;

namespace Hotfix.Events
{
    /// <summary>
    /// 房间或房间玩法数据变化事件。
    /// </summary>
    public sealed class RoomChangedEventArgs : GameEventArgs
    {
        public static readonly string EventId = typeof(RoomChangedEventArgs).FullName;

        public override string Id
        {
            get { return EventId; }
        }

        public override void Clear()
        {
        }

        public static RoomChangedEventArgs Create()
        {
            return ReferencePool.Acquire<RoomChangedEventArgs>();
        }
    }
}
