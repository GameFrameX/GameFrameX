#if !BESTHTTP_DISABLE_SIGNALR_CORE
using System;
using BestHTTP.PlatformSupport.Memory;
using BestHTTP.JSON.LitJson;

namespace BestHTTP.SignalRCore.Encoders
{
    public sealed class LitJsonEncoder : BestHTTP.SignalRCore.IEncoder
    {
        public LitJsonEncoder()
        {
            BestHTTP.JSON.LitJson.JsonMapper.RegisterImporter<int, long>((input) => input);
            BestHTTP.JSON.LitJson.JsonMapper.RegisterImporter<long, int>((input) => (int)input);
            BestHTTP.JSON.LitJson.JsonMapper.RegisterImporter<double, int>((input) => (int)(input + 0.5));
            BestHTTP.JSON.LitJson.JsonMapper.RegisterImporter<string, DateTime>((input) => Convert.ToDateTime((string)input).ToUniversalTime());
            BestHTTP.JSON.LitJson.JsonMapper.RegisterImporter<double, float>((input) => (float)input);
            BestHTTP.JSON.LitJson.JsonMapper.RegisterImporter<string, byte[]>((input) => Convert.FromBase64String(input));
            BestHTTP.JSON.LitJson.JsonMapper.RegisterExporter<float>((f, writer) => writer.Write((double)f));
        }

        public T DecodeAs<T>(BufferSegment buffer)
        {
            using (var reader = new System.IO.StreamReader(new System.IO.MemoryStream(buffer.Data, buffer.Offset, buffer.Count)))
            {
                return JsonMapper.ToObject<T>(reader);
            }
        }

        public PlatformSupport.Memory.BufferSegment Encode<T>(T value)
        {
            var json = JsonMapper.ToJson(value);
            int len = System.Text.Encoding.UTF8.GetByteCount(json);
            byte[] buffer = BufferPool.Get(len + 1, true);
            System.Text.Encoding.UTF8.GetBytes(json, 0, json.Length, buffer, 0);
            buffer[len] = (byte)JsonProtocol.Separator;
            return new BufferSegment(buffer, 0, len + 1);
        }

        public object ConvertTo(Type toType, object obj)
        {
            string json = BestHTTP.JSON.LitJson.JsonMapper.ToJson(obj);
            return BestHTTP.JSON.LitJson.JsonMapper.ToObject(toType, json);
        }
    }
}

#endif
