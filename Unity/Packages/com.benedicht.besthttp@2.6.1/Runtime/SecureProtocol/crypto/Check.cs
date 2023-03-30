#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto
{
    internal class Check
    {
        internal static void DataLength(bool condition, string msg)
        {
            if (condition)
                throw new DataLengthException(msg);
        }

        internal static void DataLength(byte[] buf, int off, int len, string msg)
        {
            if (off > (buf.Length - len))
                throw new DataLengthException(msg);
        }

        internal static void OutputLength(byte[] buf, int off, int len, string msg)
        {
            if (off > (buf.Length - len))
                throw new OutputLengthException(msg);
        }
    }
}
#pragma warning restore
#endif
