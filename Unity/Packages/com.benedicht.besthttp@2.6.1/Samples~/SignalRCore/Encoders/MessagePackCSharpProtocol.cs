#if !BESTHTTP_DISABLE_SIGNALR_CORE && BESTHTTP_SIGNALR_CORE_ENABLE_MESSAGEPACK_CSHARP
using System;
using System.Buffers;
using System.Collections.Generic;

using BestHTTP.Extensions;

using BestHTTP.PlatformSupport.Memory;
using BestHTTP.SignalRCore.Messages;

using MessagePack;

namespace BestHTTP.SignalRCore.Encoders
{
    class BufferPoolBufferWriter : IBufferWriter<byte>
    {
        private BufferPoolMemoryStream underlyingStream;
        private BufferSegment last;

        public BufferPoolBufferWriter(BufferPoolMemoryStream stream)
        {
            this.underlyingStream = stream;
            this.last = BufferSegment.Empty;
        }

        public void Advance(int count)
        {
            this.underlyingStream.Write(this.last.Data, this.last.Offset, this.last.Count + count);
            BufferPool.Release(this.last);
            this.last = BufferSegment.Empty;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            var buffer = BufferPool.Get(Math.Max(sizeHint, BufferPool.MinBufferSize), true);
            //Array.Clear(buffer, 0, buffer.Length);

            this.last = new BufferSegment(buffer, 0, 0);
            return new Memory<byte>(buffer, 0, buffer.Length);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            var buffer = BufferPool.Get(Math.Max(sizeHint, BufferPool.MinBufferSize), true);
            //Array.Clear(buffer, 0, buffer.Length);

            this.last = new BufferSegment(buffer, 0, 0);

            return new Span<byte>(buffer, 0, buffer.Length);
        }
    }

    public sealed class MessagePackCSharpProtocol : BestHTTP.SignalRCore.IProtocol
    {
        public string Name { get { return "messagepack"; } }
        public TransferModes Type { get { return TransferModes.Binary; } }
        public IEncoder Encoder { get; private set; }
        public HubConnection Connection { get; set; }

        public BufferSegment EncodeMessage(Message message)
        {
            var memBuffer = BufferPool.Get(256, true);
            var stream = new BufferPoolMemoryStream(memBuffer, 0, memBuffer.Length, true, true, false, true);

            // Write 5 bytes for placeholder for length prefix
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);

            var bufferWriter = new BufferPoolBufferWriter(stream);
            var writer = new MessagePackWriter(bufferWriter);

            switch (message.type)
            {
                case MessageTypes.StreamItem:
                    // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#streamitem-message-encoding-1
                    // [2, Headers, InvocationId, Item]

                    writer.WriteArrayHeader(4);

                    writer.Write(2);
                    WriteHeaders(ref writer);
                    WriteString(ref writer, message.invocationId);
                    WriteValue(ref writer, bufferWriter, message.item);

                    break;

                case MessageTypes.Completion:
                    // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#completion-message-encoding-1
                    // [3, Headers, InvocationId, ResultKind, Result?]

                    byte resultKind = (byte)(!string.IsNullOrEmpty(message.error) ? /*error*/ 1 : message.result != null ? /*non-void*/ 3 : /*void*/ 2);

                    writer.WriteArrayHeader(resultKind == 2 ? 4 : 5);

                    writer.Write(3);
                    WriteHeaders(ref writer);
                    WriteString(ref writer, message.invocationId);
                    writer.Write(resultKind);

                    if (resultKind == 1) // error
                        WriteString(ref writer, message.error);
                    else if (resultKind == 3) // non-void
                        WriteValue(ref writer, bufferWriter, message.result);

                    break;

                case MessageTypes.Invocation:
                // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#invocation-message-encoding-1
                // [1, Headers, InvocationId, NonBlocking, Target, [Arguments], [StreamIds]]

                case MessageTypes.StreamInvocation:
                    // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#streaminvocation-message-encoding-1
                    // [4, Headers, InvocationId, Target, [Arguments], [StreamIds]]

                    writer.WriteArrayHeader(message.streamIds != null ? 6 : 5);

                    writer.Write((int)message.type);
                    WriteHeaders(ref writer);
                    WriteString(ref writer, message.invocationId);
                    WriteString(ref writer, message.target);
                    writer.WriteArrayHeader(message.arguments != null ? message.arguments.Length : 0);
                    if (message.arguments != null)
                        for (int i = 0; i < message.arguments.Length; ++i)
                            WriteValue(ref writer, bufferWriter, message.arguments[i]);

                    if (message.streamIds != null)
                    {
                        writer.WriteArrayHeader(message.streamIds.Length);

                        for (int i = 0; i < message.streamIds.Length; ++i)
                            WriteValue(ref writer, bufferWriter, message.streamIds[i]);
                    }

                    break;

                case MessageTypes.CancelInvocation:
                    // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#cancelinvocation-message-encoding-1
                    // [5, Headers, InvocationId]

                    writer.WriteArrayHeader(3);

                    writer.Write(5);
                    WriteHeaders(ref writer);
                    WriteString(ref writer, message.invocationId);

                    break;

                case MessageTypes.Ping:
                    // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#ping-message-encoding-1
                    // [6]

                    writer.WriteArrayHeader(1);
                    writer.Write(6);

                    break;

                case MessageTypes.Close:
                    // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#close-message-encoding-1
                    // [7, Error, AllowReconnect?]

                    writer.WriteArrayHeader(string.IsNullOrEmpty(message.error) ? 1 : 2);

                    writer.Write(7);
                    if (!string.IsNullOrEmpty(message.error))
                        WriteString(ref writer, message.error);

                    break;
            }

            writer.Flush();

            // get how much bytes got written to the buffer. This includes the 5 placeholder bytes too.
            int length = (int)stream.Position;

            // this is the length without the 5 placeholder bytes
            int contentLength = length - 5;

            // get the stream's internal buffer. We set the releaseBuffer flag to false, so we can use it safely.
            var buffer = stream.GetBuffer();

            // add varint length prefix
            byte prefixBytes = GetRequiredBytesForLengthPrefix(contentLength);
            WriteLengthAsVarInt(buffer, 5 - prefixBytes, contentLength);

            // return with the final segment
            return new BufferSegment(buffer, 5 - prefixBytes, contentLength + prefixBytes);
        }

        private void WriteValue(ref MessagePackWriter writer, BufferPoolBufferWriter bufferWriter, object item)
        {
            if (item == null)
                writer.WriteNil();
            else
            {
                writer.Flush();
                MessagePackSerializer.Serialize(item.GetType(), bufferWriter, item);
            }
        }

        private void WriteString(ref MessagePackWriter writer, string str)
        {
            if (str == null)
                writer.WriteNil();
            else
            {
                int count = System.Text.Encoding.UTF8.GetByteCount(str);
                var buffer = BufferPool.Get(count, true);
                System.Text.Encoding.UTF8.GetBytes(str, 0, str.Length, buffer, 0);

                writer.WriteString(new ReadOnlySpan<byte>(buffer, 0, count));

                BufferPool.Release(buffer);
            }
        }

        private void WriteHeaders(ref MessagePackWriter writer)
        {
            writer.WriteMapHeader(0);
        }

        public void ParseMessages(BufferSegment segment, ref List<Message> messages)
        {
            int offset = segment.Offset;
            while (offset < segment.Count)
            {
                int length = (int)ReadVarInt(segment.Data, ref offset);

                var reader = new MessagePackReader(new ReadOnlyMemory<byte>(segment.Data, offset, length));

                int arrayLength = reader.ReadArrayHeader();
                int messageType = reader.ReadByte();

                switch ((MessageTypes)messageType)
                {
                    case MessageTypes.Invocation: messages.Add(ReadInvocation(ref reader)); break;
                    case MessageTypes.StreamItem: messages.Add(ReadStreamItem(ref reader)); break;
                    case MessageTypes.Completion: messages.Add(ReadCompletion(ref reader)); break;
                    case MessageTypes.StreamInvocation: messages.Add(ReadStreamInvocation(ref reader)); break;
                    case MessageTypes.CancelInvocation: messages.Add(ReadCancelInvocation(ref reader)); break;
                    case MessageTypes.Ping:

                        // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#ping-message-encoding-1
                        messages.Add(new Message { type = MessageTypes.Ping });
                        break;
                    case MessageTypes.Close: messages.Add(ReadClose(ref reader)); break;
                }

                offset += length;
            }
        }

        private Message ReadClose(ref MessagePackReader reader)
        {
            // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#close-message-encoding-1

            string error = reader.ReadString();
            bool allowReconnect = false;
            try
            {
                allowReconnect = reader.ReadBoolean();
            }
            catch { }

            return new Message
            {
                type = MessageTypes.Close,
                error = error,
                allowReconnect = allowReconnect
            };
        }

        private Message ReadCancelInvocation(ref MessagePackReader reader)
        {
            // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#cancelinvocation-message-encoding-1

            ReadHeaders(ref reader);
            string invocationId = reader.ReadString();

            return new Message
            {
                type = MessageTypes.CancelInvocation,
                invocationId = invocationId
            };
        }

        private Message ReadStreamInvocation(ref MessagePackReader reader)
        {
            // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#streaminvocation-message-encoding-1

            ReadHeaders(ref reader);
            string invocationId = reader.ReadString();
            string target = reader.ReadString();
            object[] arguments = ReadArguments(ref reader, target);
            string[] streamIds = ReadStreamIds(ref reader);

            return new Message
            {
                type = MessageTypes.StreamInvocation,
                invocationId = invocationId,
                target = target,
                arguments = arguments,
                streamIds = streamIds
            };
        }

        private Message ReadCompletion(ref MessagePackReader reader)
        {
            // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#completion-message-encoding-1

            ReadHeaders(ref reader);
            string invocationId = reader.ReadString();
            byte resultKind = reader.ReadByte();

            switch (resultKind)
            {
                // 1 - Error result - Result contains a String with the error message
                case 1:
                    string error = reader.ReadString();
                    return new Message
                    {
                        type = MessageTypes.Completion,
                        invocationId = invocationId,
                        error = error
                    };

                // 2 - Void result - Result is absent
                case 2:
                    return new Message
                    {
                        type = MessageTypes.Completion,
                        invocationId = invocationId
                    };

                // 3 - Non-Void result - Result contains the value returned by the server
                case 3:
                    object item = ReadItem(ref reader, invocationId);
                    return new Message
                    {
                        type = MessageTypes.Completion,
                        invocationId = invocationId,
                        item = item,
                        result = item
                    };

                default:
                    throw new NotImplementedException("Unknown resultKind: " + resultKind);
            }
        }

        private Message ReadStreamItem(ref MessagePackReader reader)
        {
            // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#streamitem-message-encoding-1

            ReadHeaders(ref reader);
            string invocationId = reader.ReadString();
            object item = ReadItem(ref reader, invocationId);

            return new Message
            {
                type = MessageTypes.StreamItem,
                invocationId = invocationId,
                item = item
            };
        }

        private Message ReadInvocation(ref MessagePackReader reader)
        {
            // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#invocation-message-encoding-1

            ReadHeaders(ref reader);
            string invocationId = reader.ReadString();
            string target = reader.ReadString();
            object[] arguments = ReadArguments(ref reader, target);
            string[] streamIds = ReadStreamIds(ref reader);

            return new Message
            {
                type = MessageTypes.Invocation,
                invocationId = invocationId,
                target = target,
                arguments = arguments,
                streamIds = streamIds
            };
        }

        private object ReadItem(ref MessagePackReader reader, string invocationId)
        {
            long longId = 0;
            if (long.TryParse(invocationId, out longId))
            {
                Type itemType = this.Connection.GetItemType(longId);

                return MessagePackSerializer.Deserialize(itemType, reader.ReadRaw());
            }
            else
            {
                reader.Skip();
                return null;
            }
        }

        private string[] ReadStreamIds(ref MessagePackReader reader)
        {
            var count = reader.ReadArrayHeader();
            string[] result = null;

            if (count > 0)
            {
                result = new string[count];
                for (int i = 0; i < count; i++)
                    result[i] = reader.ReadString();
            }

            return result;
        }

        private object[] ReadArguments(ref MessagePackReader reader, string target)
        {
            var subscription = this.Connection.GetSubscription(target);

            object[] args = null;
            if (subscription == null || subscription.callbacks == null || subscription.callbacks.Count == 0)
            {
                reader.Skip();
            }
            else
            {
                int count = reader.ReadArrayHeader();

                if (subscription.callbacks[0].ParamTypes != null)
                {
                    args = new object[subscription.callbacks[0].ParamTypes.Length];
                    for (int i = 0; i < subscription.callbacks[0].ParamTypes.Length; ++i)
                        args[i] = MessagePackSerializer.Deserialize(subscription.callbacks[0].ParamTypes[i], reader.ReadRaw());
                }
                else
                    args = null;
            }

            return args;
        }

        private Dictionary<string, string> ReadHeaders(ref MessagePackReader reader)
        {
            int count = reader.ReadMapHeader();

            Dictionary<string, string> result = null;
            if (count > 0)
            {
                result = new Dictionary<string, string>(count);

                for (int i = 0; i < count; i++)
                {
                    string key = reader.ReadString();
                    string value = reader.ReadString();

                    result.Add(key, value);
                }
            }

            return result;
        }

        public static byte GetRequiredBytesForLengthPrefix(int length)
        {
            byte bytes = 0;
            do
            {
                length >>= 7;
                bytes++;
            }
            while (length > 0);

            return bytes;
        }

        public static int WriteLengthAsVarInt(byte[] data, int offset, int length)
        {
            do
            {
                var current = data[offset];
                current = (byte)(length & 0x7f);
                length >>= 7;
                if (length > 0)
                {
                    current |= 0x80;
                }

                data[offset++] = current;
            }
            while (length > 0);

            return offset;
        }

        public static uint ReadVarInt(byte[] data, ref int offset)
        {
            var length = 0U;
            var numBytes = 0;

            byte byteRead;
            do
            {
                byteRead = data[offset + numBytes];
                length = length | (((uint)(byteRead & 0x7f)) << (numBytes * 7));
                numBytes++;
            }
            while (offset + numBytes < data.Length && ((byteRead & 0x80) != 0));

            offset += numBytes;

            return length;
        }

        public object ConvertTo(Type toType, object obj)
        {
            if (obj == null)
                return null;

#if NETFX_CORE
            TypeInfo typeInfo = toType.GetTypeInfo();
#endif

#if NETFX_CORE
            if (typeInfo.IsEnum)
#else
            if (toType.IsEnum)
#endif
                return Enum.Parse(toType, obj.ToString(), true);

#if NETFX_CORE
            if (typeInfo.IsPrimitive)
#else
            if (toType.IsPrimitive)
#endif
                return Convert.ChangeType(obj, toType);

            if (toType == typeof(string))
                return obj.ToString();

#if NETFX_CORE
            if (typeInfo.IsGenericType && toType.Name == "Nullable`1")
                return Convert.ChangeType(obj, toType.GenericTypeArguments[0]);
#else
            if (toType.IsGenericType && toType.Name == "Nullable`1")
                return Convert.ChangeType(obj, toType.GetGenericArguments()[0]);
#endif

            return obj;
        }

        public object[] GetRealArguments(Type[] argTypes, object[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
                return null;

            if (argTypes.Length > arguments.Length)
                throw new Exception(string.Format("argType.Length({0}) < arguments.length({1})", argTypes.Length, arguments.Length));

            return arguments;
        }
    }
}
#endif
