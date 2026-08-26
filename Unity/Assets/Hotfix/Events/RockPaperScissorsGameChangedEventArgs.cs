using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;

namespace Hotfix.Events
{
    /// <summary>
    /// 石头剪刀布玩法数据变化事件 / Rock-paper-scissors game data changed event.
    /// </summary>
    public sealed class RockPaperScissorsGameChangedEventArgs : GameEventArgs
    {
        public static readonly string EventId = typeof(RockPaperScissorsGameChangedEventArgs).FullName;

        public override string Id
        {
            get { return EventId; }
        }

        public override void Clear()
        {
        }

        public static RockPaperScissorsGameChangedEventArgs Create()
        {
            return ReferencePool.Acquire<RockPaperScissorsGameChangedEventArgs>();
        }
    }
}
