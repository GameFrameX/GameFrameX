#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Bcpg
{
    public enum RevocationKeyTag
		: byte
    {
		ClassDefault = 0x80,
		ClassSensitive = 0x40
	}
}
#pragma warning restore
#endif
