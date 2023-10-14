using System;
using System.Buffers;
using GameFramework;
using GameFramework.Network;
using MessagePack;
using UnityGameFramework.Network.Pipelines.Protocols;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Network.Pipelines
{
    public class ClientProtocolReadHelper : IProtoCalReadHelper<MessageObject>
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

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out MessageObject messageObject)
        {
            messageObject = default;

            var reader = new MessagePack.SequenceReader<byte>(input);
            // Total Length
            int msgLen = (int)reader.Length; //4
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
            // Log.Debug($"{length} {timestamp} {msgId}");
            var payload = input.Slice(reader.Position);
            if (payload.Length < 4)
            {
                throw new Exception("消息长度不够");
                return false;
            }

            consumed = payload.End;
            examined = consumed;

            var messageType = ProtoMessageIdHandler.GetRespTypeById(msgId);
            if (messageType == null)
            {
                Log.Error($"消息ID:{msgId} 找不到对应的Msg.");
                throw new Exception("不能发现消息对应类型");
            }
            else
            {
                messageObject = MessagePackSerializer.Deserialize<MessageObject>(payload);
#if UNITY_EDITOR
                Log.Debug($"收到消息 ID:[{msgId}] ==>消息类型:{messageType} 消息内容:{Utility.Json.ToJson(messageObject)}");
#endif
            }

            return true;
        }
    }
}