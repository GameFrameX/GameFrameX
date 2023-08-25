using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using Bedrock.Framework;
using Bedrock.Framework.Protocols;
using GameFramework;
using MessagePack;
using Net;
using Protocol;
using UnityGameFramework.Runtime;

namespace Base.Net
{
    public class ClientProtocol : IProtoCal<Message>
    {
        const int MAX_RECV_SIZE = 1024 * 1024;

        /// <summary>
        /// 检查消息长度是否合法
        /// </summary>
        /// <param name="msgLen"></param>
        /// <returns></returns>
        private static bool CheckMsgLen(int msgLen)
        {
            //消息长度+时间戳+magic+消息id+数据
            //4 + 8 + 4 + 4 + data
            if (msgLen <= 16) //(消息长度已经被读取)
            {
                // LOGGER.Error("从客户端接收的包大小异常:" + msgLen + ":至少16个字节");
                return false;
            }
            else if (msgLen > MAX_RECV_SIZE)
            {
                // LOGGER.Error("从客户端接收的包大小超过限制：" + msgLen + "字节，最大值：" + MAX_RECV_SIZE / 1024 + "字节");
                return false;
            }

            return true;
        }


        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out Message message)
        {
            message = default;

            var reader = new SequenceReader<byte>(input);
            // Total Length
            int msgLen = (int) reader.Length; //4
            if (!CheckMsgLen(msgLen))
            {
                consumed = input.End;
                return false;
            }

            // Length
            reader.TryReadBigEndian(out int length); //4
            // timestamp
            reader.TryReadBigEndian(out long timestamp); //8
            // MsgId
            reader.TryReadBigEndian(out int msgId); //4
            Log.Debug($"{length} {timestamp} {msgId}");
            var payload = input.Slice(reader.Position);
            if (payload.Length < 4)
            {
                throw new ProtocalParseErrorException("消息长度不够");
                return false;
            }

            consumed = payload.End;
            examined = consumed;

            var messageType = ProtoMessageIdHandler.GetRespTypeById(msgId);
            if (messageType == null)
            {
                Log.Error($"消息ID:{msgId} 找不到对应的Msg.");
                throw new ProtocalParseErrorException("不能发现消息对应类型");
            }
            else
            {
                message = MessagePackSerializer.Deserialize<Message>(payload);
#if UNITY_EDITOR
                Log.Debug($"收到消息 ID:[{msgId}] ==>消息类型:{messageType} 消息内容:{Utility.Json.ToJson(message)}");
#endif
            }

            return true;
        }

        private const int Magic = 0x1234;

        int count = 0;
        // private readonly MemoryStream _writeMemoryStream = new MemoryStream();

        public void WriteMessage(Message msg, IBufferWriter<byte> output)
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