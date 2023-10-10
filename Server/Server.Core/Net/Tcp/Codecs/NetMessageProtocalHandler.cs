/*using System.Buffers;
using System.Text;
using Newtonsoft.Json;
using NLog.Fluent;
using Server.Core.Hotfix;
using Server.Core.Net.Bedrock.Protocols;
using Server.Core.Net.Messages;
using Server.Utility;

namespace Server.Core.Net.Tcp.Codecs
{
    public class NetMessageProtocalHandler : IProtocal<NetMessage>
    {
        private readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed,
            ref SequencePosition examined, out NetMessage message)
        {
            var reader = new SequenceReader<byte>(input);
            //客户端传过来的length包含了长度自身（data: [length:byte[1,2,3,4]] ==> 则length=int 4 个字节+byte数组长度4=8）
            if (!reader.TryReadBigEndian(out int length) || reader.Remaining < length - 4)
            {
                message = default;
                return false;
            }

            var payload = input.Slice(reader.Position, length - 4); //length已经被TryReadBigEndian读取
            message = new NetMessage(payload);

            consumed = payload.End;
            examined = consumed;
            return true;
        }

        public void WriteMessage(NetMessage netMessage, IBufferWriter<byte> output)
        {
            var bytes = netMessage.Serialize();
            // len +timestamp + msgId + bytes.length
            int len = 4 + 4 + 8 + bytes.Length;
            var span = output.GetSpan(len);
            int offset = 0;
            XBuffer.WriteInt(len, span, ref offset);
            XBuffer.WriteLong(TimeHelper.UnixTimeSeconds(), span, ref offset);
            var messageType = netMessage.Msg.GetType();
            var msgId = HotfixMgr.GetMsgType(messageType);
            XBuffer.WriteInt(msgId, span, ref offset);
            XBuffer.WriteBytesWithoutLength(bytes, span, ref offset);
            output.Advance(len);

            Log.Debug($"---发送消息ID:[{msgId}] ==>消息类型:{messageType} 消息内容:{JsonConvert.SerializeObject(netMessage.Msg)}");
        }
    }
}*/