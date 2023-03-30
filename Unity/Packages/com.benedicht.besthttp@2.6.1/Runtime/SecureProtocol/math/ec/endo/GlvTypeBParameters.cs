#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC.Endo
{
    public class GlvTypeBParameters
    {
        protected readonly BigInteger m_beta, m_lambda;
        protected readonly ScalarSplitParameters m_splitParams;


        public GlvTypeBParameters(BigInteger beta, BigInteger lambda, BigInteger[] v1, BigInteger[] v2,
            BigInteger g1, BigInteger g2, int bits)
        {
            this.m_beta = beta;
            this.m_lambda = lambda;
            this.m_splitParams = new ScalarSplitParameters(v1, v2, g1, g2, bits);
        }

        public GlvTypeBParameters(BigInteger beta, BigInteger lambda, ScalarSplitParameters splitParams)
        {
            this.m_beta = beta;
            this.m_lambda = lambda;
            this.m_splitParams = splitParams;
        }

        public virtual BigInteger Beta
        {
            get { return m_beta; }
        }

        public virtual BigInteger Lambda
        {
            get { return m_lambda; }
        }

        public virtual ScalarSplitParameters SplitParams
        {
            get { return m_splitParams; }
        }


        public virtual BigInteger[] V1
        {
            get { return new BigInteger[] { m_splitParams.V1A, m_splitParams.V1B }; }
        }


        public virtual BigInteger[] V2
        {
            get { return new BigInteger[] { m_splitParams.V2A, m_splitParams.V2B }; }
        }


        public virtual BigInteger G1
        {
            get { return m_splitParams.G1; }
        }


        public virtual BigInteger G2
        {
            get { return m_splitParams.G2; }
        }


        public virtual int Bits
        {
            get { return m_splitParams.Bits; }
        }
    }
}
#pragma warning restore
#endif
