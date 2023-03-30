#if !BESTHTTP_DISABLE_SIGNALR_CORE && BESTHTTP_SIGNALR_CORE_ENABLE_NEWTONSOFT_JSON_DOTNET_ENCODER
using System;
using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP.SignalRCore.Encoders
{
    public sealed class JsonDotNetEncoder : BestHTTP.SignalRCore.IEncoder
    {
        public object ConvertTo(Type toType, object obj)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

            return Newtonsoft.Json.JsonConvert.DeserializeObject(json, toType);
        }

        public T DecodeAs<T>(BufferSegment buffer)
        {
            using (var reader = new System.IO.StreamReader(new System.IO.MemoryStream(buffer.Data, buffer.Offset, buffer.Count)))
            using (var jsonReader = new Newtonsoft.Json.JsonTextReader(reader))
                return new Newtonsoft.Json.JsonSerializer().Deserialize<T>(jsonReader);
        }

        public BufferSegment Encode<T>(T value)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(value);

            int len = System.Text.Encoding.UTF8.GetByteCount(json);
            byte[] buffer = BufferPool.Get(len + 1, true);
            System.Text.Encoding.UTF8.GetBytes(json, 0, json.Length, buffer, 0);

            buffer[len] = 0x1e;

            return new BufferSegment(buffer, 0, len + 1);
        }
    }
}
#endif
