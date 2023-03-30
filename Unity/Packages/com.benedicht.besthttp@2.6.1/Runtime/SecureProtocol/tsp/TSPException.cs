#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Tsp
{
#if !(NETCF_1_0 || NETCF_2_0 || SILVERLIGHT || PORTABLE || NETFX_CORE)
    [Serializable]
#endif
    public class TspException
		: Exception
	{
		public TspException()
		{
		}

		public TspException(
			string message)
			: base(message)
		{
		}

		public TspException(
			string		message,
			Exception	e)
			: base(message, e)
		{
		}
	}
}
#pragma warning restore
#endif
