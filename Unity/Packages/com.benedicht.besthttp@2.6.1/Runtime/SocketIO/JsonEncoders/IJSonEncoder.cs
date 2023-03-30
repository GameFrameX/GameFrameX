#if !BESTHTTP_DISABLE_SOCKETIO

using System.Collections.Generic;

namespace BestHTTP.SocketIO.JsonEncoders
{
    /// <summary>
    /// Interface to be able to write custom Json encoders/decoders.
    /// </summary>
    public interface IJsonEncoder
    {
        /// <summary>
        /// The Decode function must create a list of objects from the Json formatted string parameter. If the decoding fails, it should return null.
        /// </summary>
        List<object> Decode(string json);

        /// <summary>
        /// The Encode function must create a json formatted string from the parameter. If the encoding fails, it should return null.
        /// </summary>
        string Encode(List<object> obj);
    }
}

#endif