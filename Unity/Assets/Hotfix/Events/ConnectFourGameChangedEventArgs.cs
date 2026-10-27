using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;

namespace Hotfix.Events
{
    /// <summary>
    /// 四子棋玩法数据变化事件 / Connect-four game data changed event.
    /// </summary>
    public sealed class ConnectFourGameChangedEventArgs : GameEventArgs
    {
        public static readonly string EventId = typeof(ConnectFourGameChangedEventArgs).FullName;

        public override string Id
        {
            get { return EventId; }
        }

        public override void Clear()
        {
        }

        public static ConnectFourGameChangedEventArgs Create()
        {
            return ReferencePool.Acquire<ConnectFourGameChangedEventArgs>();
        }
    }
}
