#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC.Multiplier
{

    public class ZSignedDigitL2RMultiplier 
        : AbstractECMultiplier
    {
        /**
         * 'Zeroless' Signed Digit Left-to-Right.
         */
        protected override ECPoint MultiplyPositive(ECPoint p, BigInteger k)
        {
            ECPoint addP = p.Normalize(), subP = addP.Negate();

            ECPoint R0 = addP;

            int n = k.BitLength;
            int s = k.GetLowestSetBit();

            int i = n;
            while (--i > s)
            {
                R0 = R0.TwicePlus(k.TestBit(i) ? addP : subP);
            }

            R0 = R0.TimesPow2(s);

            return R0;
        }
    }
}
#pragma warning restore
#endif
