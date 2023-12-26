using GameFrameX.Network;
using GameFrameX.Runtime;
using Hotfix.Proto.Proto;

namespace Hotfix.Network
{
    public sealed class DefaultPacketHeartBeatHandler : IPacketHeartBeatHandler, IPacketHandler
    {
        private readonly ReqHeartBeat _reqHeartBeat;

        public DefaultPacketHeartBeatHandler()
        {
            _reqHeartBeat = new ReqHeartBeat();
        }

        /// <summary>
        /// 每次心跳的间隔
        /// </summary>
        public float HeartBeatInterval
        {
            get { return 5; }
        }

        /// <summary>
        /// 几次心跳丢失。触发断开网络
        /// </summary>
        public int MissHeartBeatCountByClose { get; } = 5;

        public MessageObject Handler()
        {
            _reqHeartBeat.Timestamp = GameTimeHelper.UnixTimeMilliseconds();
            return _reqHeartBeat;
        }
    }
}