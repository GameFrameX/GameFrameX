#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;
namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Bcpg
{
	/// <remarks>Basic packet for a PGP public subkey</remarks>
    public class PublicSubkeyPacket
        : PublicKeyPacket
    {
        internal PublicSubkeyPacket(
            BcpgInputStream bcpgIn)
			: base(bcpgIn)
        {
        }

		/// <summary>Construct a version 4 public subkey packet.</summary>
        public PublicSubkeyPacket(
            PublicKeyAlgorithmTag	algorithm,
            DateTime				time,
            IBcpgKey				key)
            : base(algorithm, time, key)
        {
        }

		public override void Encode(
            BcpgOutputStream bcpgOut)
        {
            bcpgOut.WritePacket(PacketTag.PublicSubkey, GetEncodedContents(), true);
        }
    }
}
#pragma warning restore
#endif
