#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1
{
    public class LazyAsn1InputStream
        : Asn1InputStream
    {
        public LazyAsn1InputStream(
            byte[] input)
            : base(input)
        {
        }

        public LazyAsn1InputStream(
            Stream inputStream)
            : base(inputStream)
        {
        }

        internal LazyAsn1InputStream(Stream input, int limit, byte[][] tmpBuffers)
            : base(input, limit, tmpBuffers)
        {
        }

        internal override DerSequence CreateDerSequence(
            DefiniteLengthInputStream dIn)
        {
            return new LazyDerSequence(dIn.ToArray());
        }

        internal override DerSet CreateDerSet(
            DefiniteLengthInputStream dIn)
        {
            return new LazyDerSet(dIn.ToArray());
        }

        internal override Asn1EncodableVector ReadVector(DefiniteLengthInputStream defIn)
        {
            int remaining = defIn.Remaining;
            if (remaining < 1)
                return new Asn1EncodableVector(0);

            return new LazyAsn1InputStream(defIn, remaining, tmpBuffers).ReadVector();
        }
    }
}
#pragma warning restore
#endif
