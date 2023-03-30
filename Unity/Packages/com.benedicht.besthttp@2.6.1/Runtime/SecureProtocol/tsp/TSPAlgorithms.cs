#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System.Collections;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.CryptoPro;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.GM;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Nist;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Oiw;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Pkcs;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Rosstandart;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.TeleTrust;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tsp
{
	/**
	 * Recognised hash algorithms for the time stamp protocol.
	 */
	public abstract class TspAlgorithms
	{
		public static readonly string MD5 = PkcsObjectIdentifiers.MD5.Id;

		public static readonly string Sha1 = OiwObjectIdentifiers.IdSha1.Id;

		public static readonly string Sha224 = NistObjectIdentifiers.IdSha224.Id;
		public static readonly string Sha256 = NistObjectIdentifiers.IdSha256.Id;
		public static readonly string Sha384 = NistObjectIdentifiers.IdSha384.Id;
		public static readonly string Sha512 = NistObjectIdentifiers.IdSha512.Id;

		public static readonly string RipeMD128 = TeleTrusTObjectIdentifiers.RipeMD128.Id;
		public static readonly string RipeMD160 = TeleTrusTObjectIdentifiers.RipeMD160.Id;
		public static readonly string RipeMD256 = TeleTrusTObjectIdentifiers.RipeMD256.Id;

		public static readonly string Gost3411 = CryptoProObjectIdentifiers.GostR3411.Id;
        public static readonly string Gost3411_2012_256 = RosstandartObjectIdentifiers.id_tc26_gost_3411_12_256.Id;
        public static readonly string Gost3411_2012_512 = RosstandartObjectIdentifiers.id_tc26_gost_3411_12_512.Id;

        public static readonly string SM3 = GMObjectIdentifiers.sm3.Id;

        public static readonly IList Allowed;

		static TspAlgorithms()
		{
			string[] algs = new string[]
			{
				Gost3411, Gost3411_2012_256, Gost3411_2012_512, MD5, RipeMD128, RipeMD160, RipeMD256, Sha1, Sha224, Sha256, Sha384, Sha512, SM3
			};

			Allowed = BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.CreateArrayList();
			foreach (string alg in algs)
			{
				Allowed.Add(alg);
			}
		}
	}
}
#pragma warning restore
#endif
