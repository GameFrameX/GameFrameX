// //------------------------------------------------------------
// // Game Framework
// // Copyright © 2013-2021 Jiang Yin. All rights reserved.
// // Homepage: https://gameframework.cn/
// // Feedback: mailto:ellan@gameframework.cn
// //------------------------------------------------------------
//

using GameFrameX;
using GameFrameX.Network;

namespace GameFrameX.Runtime
{
    public sealed class SCHeartBeatHandler : PacketHandlerBase
    {
        public override int Id => 1;

        public override void Handle(object sender, Packet packet)
        {
            var resp = (SCHeartBeat) packet;
            GameTimeHelper.SetDifferenceTime(resp.Timestamp);
            Log.Info("Receive packet '{0}'. {1}", resp.Id.ToString(), resp.Timestamp);
        }
    }
}