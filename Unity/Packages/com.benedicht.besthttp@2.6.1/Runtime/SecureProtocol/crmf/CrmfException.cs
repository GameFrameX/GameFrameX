#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crmf
{
    public class CrmfException
        : Exception
    {
        public CrmfException()
        {
        }

        public CrmfException(string message)
            : base(message)
        {
        }

        public CrmfException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
#pragma warning restore
#endif
