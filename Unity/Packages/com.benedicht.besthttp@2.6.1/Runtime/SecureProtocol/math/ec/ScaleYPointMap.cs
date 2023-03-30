#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC
{
    public class ScaleYPointMap
        : ECPointMap
    {
        protected readonly ECFieldElement scale;

        public ScaleYPointMap(ECFieldElement scale)
        {
            this.scale = scale;
        }

        public virtual ECPoint Map(ECPoint p)
        {
            return p.ScaleY(scale);
        }
    }
}
#pragma warning restore
#endif
