#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Math.Field
{
    internal class GF2Polynomial
        : IPolynomial
    {
        protected readonly int[] exponents;

        internal GF2Polynomial(int[] exponents)
        {
            this.exponents = Arrays.Clone(exponents);
        }

        public virtual int Degree
        {
            get { return exponents[exponents.Length - 1]; }
        }

        public virtual int[] GetExponentsPresent()
        {
            return Arrays.Clone(exponents);
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            GF2Polynomial other = obj as GF2Polynomial;
            if (null == other)
            {
                return false;
            }
            return Arrays.AreEqual(exponents, other.exponents);
        }

        public override int GetHashCode()
        {
            return Arrays.GetHashCode(exponents);
        }
    }
}
#pragma warning restore
#endif
