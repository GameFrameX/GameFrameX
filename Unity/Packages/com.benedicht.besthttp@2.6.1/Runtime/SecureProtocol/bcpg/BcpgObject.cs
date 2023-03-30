#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Bcpg
{
    /// <remarks>Base class for a PGP object.</remarks>
    public abstract class BcpgObject
    {
        public virtual byte[] GetEncoded()
        {
            MemoryStream bOut = new MemoryStream();
            BcpgOutputStream pOut = new BcpgOutputStream(bOut);

            pOut.WriteObject(this);

            return bOut.ToArray();
        }

        public abstract void Encode(BcpgOutputStream bcpgOut);
    }
}

#pragma warning restore
#endif
