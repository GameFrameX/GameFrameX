#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Bcpg
{
	public class SymmetricEncIntegrityPacket
		: InputStreamPacket
	{
		internal readonly int version;

		internal SymmetricEncIntegrityPacket(
			BcpgInputStream bcpgIn)
			: base(bcpgIn)
        {
			version = bcpgIn.ReadByte();
        }
    }
}
#pragma warning restore
#endif
