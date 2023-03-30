#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.IO;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Cms
{
#if !(NETCF_1_0 || NETCF_2_0 || SILVERLIGHT || PORTABLE || NETFX_CORE)
    [Serializable]
#endif
    public class CmsStreamException
        : IOException
    {
		public CmsStreamException()
		{
		}

		public CmsStreamException(
			string name)
			: base(name)
        {
        }

		public CmsStreamException(
			string		name,
			Exception	e)
			: base(name, e)
        {
        }
    }
}
#pragma warning restore
#endif
