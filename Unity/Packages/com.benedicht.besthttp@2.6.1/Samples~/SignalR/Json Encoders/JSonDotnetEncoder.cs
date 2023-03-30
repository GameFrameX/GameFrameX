#if !BESTHTTP_DISABLE_SIGNALR && BESTHTTP_SIGNALR_WITH_JSONDOTNET

using System.Collections.Generic;

using Newtonsoft.Json;

namespace BestHTTP.SignalR.JsonEncoders
{
    public sealed class JSonDotnetEncoder : IJsonEncoder
    {
        public string Encode(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public IDictionary<string, object> DecodeMessage(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
    }
}

#endif