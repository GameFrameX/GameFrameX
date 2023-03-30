#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC.Rfc7748;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters
{
    public sealed class X448PrivateKeyParameters
        : AsymmetricKeyParameter
    {
        public static readonly int KeySize = X448.ScalarSize;
        public static readonly int SecretSize = X448.PointSize;

        private readonly byte[] data = new byte[KeySize];

        public X448PrivateKeyParameters(SecureRandom random)
            : base(true)
        {
            X448.GeneratePrivateKey(random, data);
        }

        public X448PrivateKeyParameters(byte[] buf)
            : this(Validate(buf), 0)
        {
        }

        public X448PrivateKeyParameters(byte[] buf, int off)
            : base(true)
        {
            Array.Copy(buf, off, data, 0, KeySize);
        }

        public X448PrivateKeyParameters(Stream input)
            : base(true)
        {
            if (KeySize != Streams.ReadFully(input, data))
                throw new EndOfStreamException("EOF encountered in middle of X448 private key");
        }

        public void Encode(byte[] buf, int off)
        {
            Array.Copy(data, 0, buf, off, KeySize);
        }

        public byte[] GetEncoded()
        {
            return Arrays.Clone(data);
        }

        public X448PublicKeyParameters GeneratePublicKey()
        {
            byte[] publicKey = new byte[X448.PointSize];
            X448.GeneratePublicKey(data, 0, publicKey, 0);
            return new X448PublicKeyParameters(publicKey, 0);
        }

        public void GenerateSecret(X448PublicKeyParameters publicKey, byte[] buf, int off)
        {
            byte[] encoded = new byte[X448.PointSize];
            publicKey.Encode(encoded, 0);
            if (!X448.CalculateAgreement(data, 0, encoded, 0, buf, off))
                throw new InvalidOperationException("X448 agreement failed");
        }

        private static byte[] Validate(byte[] buf)
        {
            if (buf.Length != KeySize)
                throw new ArgumentException("must have length " + KeySize, "buf");

            return buf;
        }
    }
}
#pragma warning restore
#endif
