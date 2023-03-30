#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC.Multiplier
{

    public class MontgomeryLadderMultiplier 
        : AbstractECMultiplier
    {
        /**
         * Montgomery ladder.
         */
        protected override ECPoint MultiplyPositive(ECPoint p, BigInteger k)
        {
            ECPoint[] R = new ECPoint[]{ p.Curve.Infinity, p };

            int n = k.BitLength;
            int i = n;
            while (--i >= 0)
            {
                int b = k.TestBit(i) ? 1 : 0;
                int bp = 1 - b;
                R[bp] = R[bp].Add(R[b]);
                R[b] = R[b].Twice();
            }
            return R[0];
        }
    }
}
#pragma warning restore
#endif
