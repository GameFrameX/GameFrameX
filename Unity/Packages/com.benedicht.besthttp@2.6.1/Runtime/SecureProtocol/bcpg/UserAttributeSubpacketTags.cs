#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Bcpg
{
    /**
    * Basic PGP user attribute sub-packet tag types.
    */
    public enum UserAttributeSubpacketTag
    {
        ImageAttribute = 1
    }
}
#pragma warning restore
#endif
