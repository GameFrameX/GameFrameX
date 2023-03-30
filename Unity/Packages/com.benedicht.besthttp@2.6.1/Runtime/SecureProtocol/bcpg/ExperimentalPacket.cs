#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Bcpg
{
	/// <remarks>Basic packet for an experimental packet.</remarks>
    public class ExperimentalPacket
        : ContainedPacket //, PublicKeyAlgorithmTag
    {
        private readonly PacketTag	tag;
        private readonly byte[]		contents;

		internal ExperimentalPacket(
            PacketTag		tag,
            BcpgInputStream	bcpgIn)
        {
            this.tag = tag;

			this.contents = bcpgIn.ReadAll();
        }

		public PacketTag Tag
        {
			get { return tag; }
        }

		public byte[] GetContents()
        {
			return (byte[]) contents.Clone();
        }

		public override void Encode(
            BcpgOutputStream bcpgOut)
        {
            bcpgOut.WritePacket(tag, contents, true);
        }
    }
}
#pragma warning restore
#endif
