#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Encoders
{
    /// <summary>
    /// Translator interface.
    /// </summary>
    public interface ITranslator
    {
        int GetEncodedBlockSize();

        int Encode(byte[] input, int inOff, int length, byte[] outBytes, int outOff);

        int GetDecodedBlockSize();

        int Decode(byte[] input, int inOff, int length, byte[] outBytes, int outOff);
    }

}
#pragma warning restore
#endif
