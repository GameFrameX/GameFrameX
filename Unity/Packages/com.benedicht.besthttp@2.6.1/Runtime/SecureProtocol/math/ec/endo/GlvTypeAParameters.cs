#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC.Endo
{
    public class GlvTypeAParameters
    {
        protected readonly BigInteger m_i, m_lambda;
        protected readonly ScalarSplitParameters m_splitParams;

        public GlvTypeAParameters(BigInteger i, BigInteger lambda, ScalarSplitParameters splitParams)
        {
            this.m_i = i;
            this.m_lambda = lambda;
            this.m_splitParams = splitParams;
        }

        public virtual BigInteger I
        {
            get { return m_i; }
        }

        public virtual BigInteger Lambda
        {
            get { return m_lambda; }
        }

        public virtual ScalarSplitParameters SplitParams
        {
            get { return m_splitParams; }
        }
    }
}
#pragma warning restore
#endif
