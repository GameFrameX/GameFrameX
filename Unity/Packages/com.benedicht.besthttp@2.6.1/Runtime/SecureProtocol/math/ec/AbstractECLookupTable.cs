#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC
{
    public abstract class AbstractECLookupTable
        : ECLookupTable
    {
        public abstract ECPoint Lookup(int index);
        public abstract int Size { get; }

        public virtual ECPoint LookupVar(int index)
        {
            return Lookup(index);
        }
    }
}
#pragma warning restore
#endif
