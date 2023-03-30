#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC.Multiplier
{
    public abstract class AbstractECMultiplier
        : ECMultiplier
    {
        public virtual ECPoint Multiply(ECPoint p, BigInteger k)
        {
            int sign = k.SignValue;
            if (sign == 0 || p.IsInfinity)
                return p.Curve.Infinity;

            ECPoint positive = MultiplyPositive(p, k.Abs());
            ECPoint result = sign > 0 ? positive : positive.Negate();

            /*
             * Although the various multipliers ought not to produce invalid output under normal
             * circumstances, a final check here is advised to guard against fault attacks.
             */
            return CheckResult(result);
        }

        protected abstract ECPoint MultiplyPositive(ECPoint p, BigInteger k);

        protected virtual ECPoint CheckResult(ECPoint p)
        {
            return ECAlgorithms.ImplCheckResult(p);
        }
    }
}
#pragma warning restore
#endif
