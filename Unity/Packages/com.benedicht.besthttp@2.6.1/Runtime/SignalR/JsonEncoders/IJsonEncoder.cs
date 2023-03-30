#if !BESTHTTP_DISABLE_SIGNALR

using System.Collections.Generic;

namespace BestHTTP.SignalR.JsonEncoders
{
    /// <summary>
    /// Interface to be able to write custom Json encoders/decoders.
    /// </summary>
    public interface IJsonEncoder
    {
        /// <summary>
        /// This function must create a json formatted string from the given object. If the encoding fails, it should return null.
        /// </summary>
        string Encode(object obj);

        /// <summary>
        /// This function must create a dictionary the Json formatted string parameter. If the decoding fails, it should return null.
        /// </summary>
        IDictionary<string, object> DecodeMessage(string json);
    }
}

#endif