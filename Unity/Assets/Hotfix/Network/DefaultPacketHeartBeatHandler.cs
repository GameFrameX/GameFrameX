using GameFrameX.Network.Runtime;
using GameFrameX.Runtime;
using Hotfix.Proto;

namespace Hotfix.Network
{
    public sealed class DefaultPacketHeartBeatHandler : BasePacketHeartBeatHandler
    {
        public override float HeartBeatInterval { get; } = 30;

        public override MessageObject Handler()
        {
            var reqHeartBeat = new ReqHeartBeat
            {
                Timestamp = TimerHelper.UnixTimeMilliseconds(),
            };
            reqHeartBeat.UpdateUniqueId();
            return reqHeartBeat;
        }
    }
}