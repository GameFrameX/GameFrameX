#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.X509.Store
{
	public interface IX509Selector
#if !(SILVERLIGHT || PORTABLE || NETFX_CORE)
		: ICloneable
#endif
	{
#if SILVERLIGHT || PORTABLE || NETFX_CORE
        object Clone();
#endif
        bool Match(object obj);
	}
}
#pragma warning restore
#endif
