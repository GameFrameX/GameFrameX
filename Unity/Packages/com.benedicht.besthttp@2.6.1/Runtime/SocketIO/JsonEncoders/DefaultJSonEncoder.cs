#if !BESTHTTP_DISABLE_SOCKETIO

using System.Collections.Generic;
using BestHTTP.JSON;

namespace BestHTTP.SocketIO.JsonEncoders
{
    /// <summary>
    /// The default IJsonEncoder implementation. It's uses the Json class from the BestHTTP.JSON namespace to encode and decode.
    /// </summary>
    public sealed class DefaultJSonEncoder : IJsonEncoder
    {
        public List<object> Decode(string json)
        {
            return Json.Decode(json) as List<object>;
        }

        public string Encode(List<object> obj)
        {
            return Json.Encode(obj);
        }
    }
}

#endif