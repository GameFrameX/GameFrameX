#if !BESTHTTP_DISABLE_SIGNALR

using System.Collections.Generic;

using BestHTTP.JSON.LitJson;

namespace BestHTTP.SignalR.JsonEncoders
{
    public sealed class LitJsonEncoder : IJsonEncoder
    {
        public string Encode(object obj)
        {
            JsonWriter writer = new JsonWriter();
            JsonMapper.ToJson(obj, writer);

            return writer.ToString();
        }

        public IDictionary<string, object> DecodeMessage(string json)
        {
            JsonReader reader = new JsonReader(json);

            return JsonMapper.ToObject<Dictionary<string, object>>(reader);
        }
    }
}

#endif
