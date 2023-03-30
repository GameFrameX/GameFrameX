#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC.Multiplier;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC.Endo
{
    public class EndoPreCompInfo
        : PreCompInfo
    {
        protected ECEndomorphism m_endomorphism;

        protected ECPoint m_mappedPoint;

        public virtual ECEndomorphism Endomorphism
        {
            get { return m_endomorphism; }
            set { this.m_endomorphism = value; }
        }

        public virtual ECPoint MappedPoint
        {
            get { return m_mappedPoint; }
            set { this.m_mappedPoint = value; }
        }
    }
}
#pragma warning restore
#endif
