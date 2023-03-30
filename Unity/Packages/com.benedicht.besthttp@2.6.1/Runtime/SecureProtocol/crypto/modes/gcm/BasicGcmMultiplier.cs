#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Modes.Gcm
{
    public class BasicGcmMultiplier
        : IGcmMultiplier
    {
        private ulong[] H;

        public void Init(byte[] H)
        {
            this.H = GcmUtilities.AsUlongs(H);
        }

        public void MultiplyH(byte[] x)
        {
            ulong[] t = GcmUtilities.AsUlongs(x);
            GcmUtilities.Multiply(t, H);
            GcmUtilities.AsBytes(t, x);
        }
    }
}
#pragma warning restore
#endif
