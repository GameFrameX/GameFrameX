#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Cms
{
#if !(NETCF_1_0 || NETCF_2_0 || SILVERLIGHT || PORTABLE || NETFX_CORE)
    [Serializable]
#endif
    public class CmsException
		: Exception
	{
		public CmsException()
		{
		}

		public CmsException(
			string msg)
			: base(msg)
		{
		}

		public CmsException(
			string		msg,
			Exception	e)
			: base(msg, e)
		{
		}
	}
}
#pragma warning restore
#endif
