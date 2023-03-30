#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Bcpg
{
    /**
    * Basic type for a symmetric encrypted session key packet
    */
    public class SymmetricKeyEncSessionPacket
        : ContainedPacket
    {
        private int version;
        private SymmetricKeyAlgorithmTag encAlgorithm;
        private S2k s2k;
        private readonly byte[] secKeyData;

        public SymmetricKeyEncSessionPacket(
            BcpgInputStream bcpgIn)
        {
            version = bcpgIn.ReadByte();
            encAlgorithm = (SymmetricKeyAlgorithmTag) bcpgIn.ReadByte();

            s2k = new S2k(bcpgIn);

            secKeyData = bcpgIn.ReadAll();
        }

		public SymmetricKeyEncSessionPacket(
            SymmetricKeyAlgorithmTag	encAlgorithm,
            S2k							s2k,
            byte[]						secKeyData)
        {
            this.version = 4;
            this.encAlgorithm = encAlgorithm;
            this.s2k = s2k;
            this.secKeyData = secKeyData;
        }

        /**
        * @return int
        */
        public SymmetricKeyAlgorithmTag EncAlgorithm
        {
			get { return encAlgorithm; }
        }

        /**
        * @return S2k
        */
        public S2k S2k
        {
			get { return s2k; }
        }

        /**
        * @return byte[]
        */
        public byte[] GetSecKeyData()
        {
            return secKeyData;
        }

        /**
        * @return int
        */
        public int Version
        {
			get { return version; }
        }

        public override void Encode(
            BcpgOutputStream bcpgOut)
        {
            MemoryStream bOut = new MemoryStream();
            BcpgOutputStream pOut = new BcpgOutputStream(bOut);

            pOut.Write(
				(byte) version,
				(byte) encAlgorithm);

			pOut.WriteObject(s2k);

			if (secKeyData != null && secKeyData.Length > 0)
			{
                pOut.Write(secKeyData);
            }

			bcpgOut.WritePacket(PacketTag.SymmetricKeyEncryptedSessionKey, bOut.ToArray(), true);
        }
    }
}
#pragma warning restore
#endif
