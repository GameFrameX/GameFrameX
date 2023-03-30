//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2019 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

using ProtoBuf;
using System;

namespace UnityGameFramework.Runtime
{
    [Serializable, ProtoContract(Name = @"CSHeartBeat")]
    public partial class CSHeartBeat : CSPacketBase
    {
        public CSHeartBeat()
        {

        }

        public override int Id
        {
            get
            {
                return 1;
            }
        }

        public override void Clear()
        {

        }
    }
}
