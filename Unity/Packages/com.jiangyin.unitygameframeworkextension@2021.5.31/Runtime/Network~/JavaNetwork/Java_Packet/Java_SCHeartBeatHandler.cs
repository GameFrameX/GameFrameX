using GameFramework.Network;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 心跳处理
    /// </summary>
    public class Java_SCHeartBeatHandler : PacketHandlerBase
    {
        /// <summary>
        /// 心跳消息号
        /// </summary>
        public override int Id => 0;

        public override void Handle(object sender, Packet packet)
        {
            S2CMessage packetImpl = (S2CMessage) packet;
            var resp = (ResHeart) ProtobufHelper.FromBytes(typeof(ResHeart), packetImpl.data);
            GameTimeHelper.SetDifferenceTime(resp.timestamp);
            // Log.Info("Receive packet '{0}'. {1}", packetImpl.Id.ToString(), resp.timestamp);
        }
    }
}