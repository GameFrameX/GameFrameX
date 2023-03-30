#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Bcpg
{
    public class InputStreamPacket
        : Packet
    {
        private readonly BcpgInputStream bcpgIn;

		public InputStreamPacket(
            BcpgInputStream bcpgIn)
        {
            this.bcpgIn = bcpgIn;
        }

		/// <summary>Note: you can only read from this once...</summary>
		public BcpgInputStream GetInputStream()
        {
            return bcpgIn;
        }
    }
}
#pragma warning restore
#endif
