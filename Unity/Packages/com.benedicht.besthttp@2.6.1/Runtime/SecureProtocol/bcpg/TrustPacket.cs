#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Bcpg
{
	/// <summary>Basic type for a trust packet.</summary>
    public class TrustPacket
        : ContainedPacket
    {
        private readonly byte[] levelAndTrustAmount;

		public TrustPacket(
            BcpgInputStream bcpgIn)
        {
            MemoryStream bOut = new MemoryStream();

			int ch;
            while ((ch = bcpgIn.ReadByte()) >= 0)
            {
                bOut.WriteByte((byte) ch);
            }

			levelAndTrustAmount = bOut.ToArray();
        }

		public TrustPacket(
            int trustCode)
        {
			this.levelAndTrustAmount = new byte[]{ (byte) trustCode };
        }

		public byte[] GetLevelAndTrustAmount()
		{
			return (byte[]) levelAndTrustAmount.Clone();
		}

		public override void Encode(
            BcpgOutputStream bcpgOut)
        {
            bcpgOut.WritePacket(PacketTag.Trust, levelAndTrustAmount, true);
        }
    }
}
#pragma warning restore
#endif
