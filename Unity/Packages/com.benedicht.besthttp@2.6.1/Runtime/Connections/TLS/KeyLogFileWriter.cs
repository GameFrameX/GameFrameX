#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using System.Diagnostics;
using System.IO;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Tls;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Tls.Crypto.Impl;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Encoders;

// https://www.m00nie.com/2015/05/decrypt-https-ssltls-with-wireshark/
// https://developer.mozilla.org/en-US/docs/Mozilla/Projects/NSS/Key_Log_Format
// https://github.com/bcgit/bc-csharp/issues/343

namespace BestHTTP.Connections.TLS
{
    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Mozilla/Projects/NSS/Key_Log_Format
    /// </summary>
    internal enum Labels
    {
        CLIENT_RANDOM,
        CLIENT_EARLY_TRAFFIC_SECRET,
        CLIENT_HANDSHAKE_TRAFFIC_SECRET,
        SERVER_HANDSHAKE_TRAFFIC_SECRET,
        CLIENT_TRAFFIC_SECRET_0,
        SERVER_TRAFFIC_SECRET_0,
        EARLY_EXPORTER_SECRET,
        EXPORTER_SECRET
    }

    internal static class KeyLogFileWriter
    {
        private static string GetKeylogFileName() => Environment.GetEnvironmentVariable("SSLKEYLOGFILE", EnvironmentVariableTarget.User);

        [Conditional("UNITY_EDITOR")]
        public static void WriteLabel(Labels label, byte[] clientRandom, TlsSecret secret)
        {
            if (clientRandom != null && secret != null)
            {
                string SSLKEYLOGFILE = GetKeylogFileName();
                if (!string.IsNullOrEmpty(SSLKEYLOGFILE))
                    using (var writer = new StreamWriter(System.IO.File.Open(SSLKEYLOGFILE, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
                        writer.WriteLine($"{label} {Hex.ToHexString(clientRandom)} {Hex.ToHexString((secret as AbstractTlsSecret).CopyData())}");
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void WriteLabel(Labels label, SecurityParameters securityParameters)
        {
            try
            {
                TlsSecret secret = null;
                switch (label)
                {
                    case Labels.CLIENT_RANDOM: secret = securityParameters.MasterSecret; break;
                    case Labels.CLIENT_HANDSHAKE_TRAFFIC_SECRET: secret = securityParameters.TrafficSecretClient; break;
                    case Labels.SERVER_HANDSHAKE_TRAFFIC_SECRET: secret = securityParameters.TrafficSecretServer; break;
                    case Labels.CLIENT_TRAFFIC_SECRET_0: secret = securityParameters.TrafficSecretClient; break;
                    case Labels.SERVER_TRAFFIC_SECRET_0: secret = securityParameters.TrafficSecretServer; break;
                    case Labels.EXPORTER_SECRET: secret = securityParameters.ExporterMasterSecret; break;

                    case Labels.CLIENT_EARLY_TRAFFIC_SECRET: break;
                    case Labels.EARLY_EXPORTER_SECRET: break;
                }

                if (secret != null)
                    WriteLabel(label, securityParameters.ClientRandom, secret);
            }
            catch
            { }
        }
    }
}
#endif
