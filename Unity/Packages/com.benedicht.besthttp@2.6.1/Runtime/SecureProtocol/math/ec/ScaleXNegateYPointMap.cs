#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC
{
    public class ScaleXNegateYPointMap
        : ECPointMap
    {
        protected readonly ECFieldElement scale;

        public ScaleXNegateYPointMap(ECFieldElement scale)
        {
            this.scale = scale;
        }

        public virtual ECPoint Map(ECPoint p)
        {
            return p.ScaleXNegateY(scale);
        }
    }
}
#pragma warning restore
#endif
