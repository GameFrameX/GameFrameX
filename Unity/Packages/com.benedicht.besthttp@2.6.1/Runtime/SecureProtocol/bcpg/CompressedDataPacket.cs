#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Bcpg
{
	/// <remarks>Generic compressed data object.</remarks>
    public class CompressedDataPacket
        : InputStreamPacket
    {
        private readonly CompressionAlgorithmTag algorithm;

		internal CompressedDataPacket(
            BcpgInputStream bcpgIn)
			: base(bcpgIn)
        {
            this.algorithm = (CompressionAlgorithmTag) bcpgIn.ReadByte();
        }

		/// <summary>The algorithm tag value.</summary>
        public CompressionAlgorithmTag Algorithm
		{
			get { return algorithm; }
		}
    }
}
#pragma warning restore
#endif
