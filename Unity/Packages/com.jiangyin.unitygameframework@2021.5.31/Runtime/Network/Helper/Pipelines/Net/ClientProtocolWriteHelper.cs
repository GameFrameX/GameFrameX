using System.Buffers;
using GameFramework;
using GameFramework.Network;
using MessagePack;
using UnityGameFramework.Network.Pipelines.Protocols;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Network.Pipelines
{
    public class ClientProtocolWriteHelper : IProtoCalWriteHelper<MessageObject>
    {
        private const int Magic = 0x1234;

        int count = 0;
        // private readonly MemoryStream _writeMemoryStream = new MemoryStream();

        public void WriteMessage(MessageObject msg, IBufferWriter<byte> output)
        {
            // _writeMemoryStream.Seek(0, SeekOrigin.Begin);
            //length + timestamp + magic + msgid

            // MessagePackSerializer.Serialize().Serializer.Serialize(_writeMemoryStream, msg); 
            var bytes = MessagePackSerializer.Serialize(msg);
            var messageLength = bytes.Length;
            int len = 4 + 8 + 4 + 4 + messageLength;

            var span = output.GetSpan(len);

            int magic = Magic + ++count;
            magic ^= Magic << 8;
            magic ^= len;

            int offset = 0;
            span.WriteInt(len, ref offset);
            span.WriteLong(GameTimeHelper.UnixTimeSeconds(), ref offset);
            span.WriteInt(magic, ref offset);
            var messageType = msg.GetType();
            var msgId = ProtoMessageIdHandler.GetReqMessageIdByType(messageType);
            span.WriteInt(msgId, ref offset);
            span.WriteBytesWithoutLength(bytes, ref offset);
            output.Advance(len);

#if UNITY_EDITOR
            Log.Debug($"发送消息 ID:[{msgId}] ==>消息类型:{messageType} 消息内容:{Utility.Json.ToJson(msg)}");
#endif
        }
    }
}