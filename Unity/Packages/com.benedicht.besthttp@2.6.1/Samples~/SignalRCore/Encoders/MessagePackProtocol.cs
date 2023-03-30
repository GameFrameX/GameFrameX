#if !BESTHTTP_DISABLE_SIGNALR_CORE && BESTHTTP_SIGNALR_CORE_ENABLE_GAMEDEVWARE_MESSAGEPACK

using System;
using System.Collections.Generic;
using BestHTTP.PlatformSupport.Memory;
using BestHTTP.SignalRCore.Messages;
using GameDevWare.Serialization;
using GameDevWare.Serialization.MessagePack;
using GameDevWare.Serialization.Serializers;

using UnityEngine;

namespace BestHTTP.SignalRCore.Encoders
{
    public sealed class MessagePackProtocolSerializationOptions
    {
        /// <summary>
        /// A function that must return a TypeSerializer for the given Type. To serialize an enum it can return an EnumNumberSerializer (default) to serialize enums as numbers or EnumSerializer to serialize them as strings.
        /// </summary>
        public Func<Type, TypeSerializer> EnumSerializerFactory;
    }

    /// <summary>
    /// IPRotocol implementation using the "Json & MessagePack Serialization" asset store package (https://assetstore.unity.com/packages/tools/network/json-messagepack-serialization-59918).
    /// </summary>
    public sealed class MessagePackProtocol : BestHTTP.SignalRCore.IProtocol
    {
        public string Name { get { return "messagepack"; } }

        public TransferModes Type { get { return TransferModes.Binary; } }

        public IEncoder Encoder { get; private set; }

        public HubConnection Connection { get; set; }

        public MessagePackProtocolSerializationOptions Options { get; set; }

        public MessagePackProtocol()
            : this(new MessagePackProtocolSerializationOptions { EnumSerializerFactory = (enumType) => new EnumNumberSerializer(enumType) })
        {
            
        }

        public MessagePackProtocol(MessagePackProtocolSerializationOptions options)
        {
            this.Options = options;

            GameDevWare.Serialization.Json.DefaultSerializers.Clear();
            GameDevWare.Serialization.Json.DefaultSerializers.AddRange(new TypeSerializer[]
            {
                new BinarySerializer(),
                new DateTimeOffsetSerializer(),
                new DateTimeSerializer(),
                new GuidSerializer(),
                new StreamSerializer(),
                new UriSerializer(),
                new VersionSerializer(),
                new TimeSpanSerializer(),
                new DictionaryEntrySerializer(),

                new BestHTTP.SignalRCore.Encoders.Vector2Serializer(),
                new BestHTTP.SignalRCore.Encoders.Vector3Serializer(),
                new BestHTTP.SignalRCore.Encoders.Vector4Serializer(),

                new PrimitiveSerializer(typeof (bool)),
                new PrimitiveSerializer(typeof (byte)),
                new PrimitiveSerializer(typeof (decimal)),
                new PrimitiveSerializer(typeof (double)),
                new PrimitiveSerializer(typeof (short)),
                new PrimitiveSerializer(typeof (int)),
                new PrimitiveSerializer(typeof (long)),
                new PrimitiveSerializer(typeof (sbyte)),
                new PrimitiveSerializer(typeof (float)),
                new PrimitiveSerializer(typeof (ushort)),
                new PrimitiveSerializer(typeof (uint)),
                new PrimitiveSerializer(typeof (ulong)),
                new PrimitiveSerializer(typeof (string)),
            });
        }

        /// <summary>
        /// This function must convert all element in the arguments array to the corresponding type from the argTypes array.
        /// </summary>
        public object[] GetRealArguments(Type[] argTypes, object[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
                return null;

            if (argTypes.Length > arguments.Length)
                throw new Exception(string.Format("argType.Length({0}) < arguments.length({1})", argTypes.Length, arguments.Length));

            return arguments;
        }

        /// <summary>
        /// Convert a value to the given type.
        /// </summary>
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

        /// <summary>
        /// This function must return the encoded representation of the given message.
        /// </summary>
        public BufferSegment EncodeMessage(Message message)
        {
            var memBuffer = BufferPool.Get(256, true);
            var stream = new BestHTTP.Extensions.BufferPoolMemoryStream(memBuffer, 0, memBuffer.Length, true, true, false, true);

            // Write 5 bytes for placeholder for length prefix
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);

            var buffer = BufferPool.Get(MsgPackWriter.DEFAULT_BUFFER_SIZE, true);
            
            var context = new SerializationContext {
                Options = SerializationOptions.SuppressTypeInformation,
                EnumSerializerFactory = this.Options.EnumSerializerFactory,
                ExtensionTypeHandler = CustomMessagePackExtensionTypeHandler.Instance
            };

            var writer = new MsgPackWriter(stream, context, buffer);

            switch (message.type)
            {
                case MessageTypes.StreamItem:
                    // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#streamitem-message-encoding-1
                    // [2, Headers, InvocationId, Item]

                    writer.WriteArrayBegin(4);

                    writer.WriteNumber(2);
                    WriteHeaders(writer);
                    writer.WriteString(message.invocationId);
                    WriteValue(writer, message.item);

                    writer.WriteArrayEnd();
                    break;

                case MessageTypes.Completion:
                    // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#completion-message-encoding-1
                    // [3, Headers, InvocationId, ResultKind, Result?]

                    byte resultKind = (byte)(!string.IsNullOrEmpty(message.error) ? /*error*/ 1 : message.result != null ? /*non-void*/ 3 : /*void*/ 2);

                    writer.WriteArrayBegin(resultKind == 2 ? 4 : 5);

                    writer.WriteNumber(3);
                    WriteHeaders(writer);
                    writer.WriteString(message.invocationId);
                    writer.WriteNumber(resultKind);

                    if (resultKind == 1) // error
                        writer.WriteString(message.error);
                    else if (resultKind == 3) // non-void
                        WriteValue(writer, message.result);

                    writer.WriteArrayEnd();
                    break;

                case MessageTypes.Invocation:
                    // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#invocation-message-encoding-1
                    // [1, Headers, InvocationId, NonBlocking, Target, [Arguments], [StreamIds]]

                case MessageTypes.StreamInvocation:
                    // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#streaminvocation-message-encoding-1
                    // [4, Headers, InvocationId, Target, [Arguments], [StreamIds]]

                    writer.WriteArrayBegin(message.streamIds != null ? 6 : 5);

                    writer.WriteNumber((int)message.type);
                    WriteHeaders(writer);
                    writer.WriteString(message.invocationId);
                    writer.WriteString(message.target);
                    writer.WriteArrayBegin(message.arguments != null ? message.arguments.Length : 0);
                    if (message.arguments != null)
                        for (int i = 0; i < message.arguments.Length; ++i)
                            WriteValue(writer, message.arguments[i]);
                    writer.WriteArrayEnd();

                    if (message.streamIds != null)
                    {
                        writer.WriteArrayBegin(message.streamIds.Length);

                        for (int i = 0; i < message.streamIds.Length; ++i)
                            WriteValue(writer, message.streamIds[i]);

                        writer.WriteArrayEnd();
                    }

                    writer.WriteArrayEnd();
                    break;

                case MessageTypes.CancelInvocation:
                    // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#cancelinvocation-message-encoding-1
                    // [5, Headers, InvocationId]

                    writer.WriteArrayBegin(3);

                    writer.WriteNumber(5);
                    WriteHeaders(writer);
                    writer.WriteString(message.invocationId);

                    writer.WriteArrayEnd();
                    break;

                case MessageTypes.Ping:
                    // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#ping-message-encoding-1
                    // [6]

                    writer.WriteArrayBegin(1);

                    writer.WriteNumber(6);

                    writer.WriteArrayEnd();
                    break;

                case MessageTypes.Close:
                    // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#close-message-encoding-1
                    // [7, Error, AllowReconnect?]

                    writer.WriteArrayBegin(string.IsNullOrEmpty(message.error) ? 1 : 2);

                    writer.WriteNumber(7);
                    if (!string.IsNullOrEmpty(message.error))
                        writer.WriteString(message.error);

                    writer.WriteArrayEnd();
                    break;
            }

            writer.Flush();

            // release back the buffer we used for the MsgPackWriter
            BufferPool.Release(buffer);

            // get how much bytes got written to the buffer. This includes the 5 placeholder bytes too.
            int length = (int)stream.Position;

            // this is the length without the 5 placeholder bytes
            int contentLength = length - 5;

            // get the stream's internal buffer. We set the releaseBuffer flag to false, so we can use it safely.
            buffer = stream.GetBuffer();

            // add varint length prefix
            byte prefixBytes = GetRequiredBytesForLengthPrefix(contentLength);
            WriteLengthAsVarInt(buffer, 5 - prefixBytes, contentLength);

            // return with the final segment
            return new BufferSegment(buffer, 5 - prefixBytes, contentLength + prefixBytes);
        }

        private void WriteValue(MsgPackWriter writer, object value)
        {
            if (value == null)
                writer.WriteNull();
            else
                writer.WriteValue(value, value.GetType());
        }

        private void WriteHeaders(MsgPackWriter writer)
        {
            writer.WriteObjectBegin(0);
            writer.WriteObjectEnd();
        }

        /// <summary>
        /// This function must parse binary representation of the messages into the list of Messages.
        /// </summary>
        public void ParseMessages(BufferSegment segment, ref List<Message> messages)
        {
            messages.Clear();

            int offset = segment.Offset;
            while (offset < segment.Count)
            {
                int length = (int)ReadVarInt(segment.Data, ref offset);

                using (var stream = new System.IO.MemoryStream(segment.Data, offset, length))
                {
                    var buff = BufferPool.Get(MsgPackReader.DEFAULT_BUFFER_SIZE, true);
                    try
                    {
                        var context = new SerializationContext {
                            Options = SerializationOptions.SuppressTypeInformation,
                            ExtensionTypeHandler = CustomMessagePackExtensionTypeHandler.Instance
                        };
                        var reader = new MsgPackReader(stream, context, Endianness.BigEndian, buff);
                        
                        reader.NextToken();
                        reader.NextToken();

                        int messageType = reader.ReadByte();
                        switch ((MessageTypes)messageType)
                        {
                            case MessageTypes.Invocation: messages.Add(ReadInvocation(reader)); break;
                            case MessageTypes.StreamItem: messages.Add(ReadStreamItem(reader)); break;
                            case MessageTypes.Completion: messages.Add(ReadCompletion(reader)); break;
                            case MessageTypes.StreamInvocation: messages.Add(ReadStreamInvocation(reader)); break;
                            case MessageTypes.CancelInvocation: messages.Add(ReadCancelInvocation(reader)); break;
                            case MessageTypes.Ping:

                                // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#ping-message-encoding-1
                                messages.Add(new Message { type = MessageTypes.Ping });
                                break;
                            case MessageTypes.Close: messages.Add(ReadClose(reader)); break;
                        }

                        reader.NextToken();
                    }
                    finally
                    {
                        BufferPool.Release(buff);
                    }
                }

                offset += length;
            }
        }

        private Message ReadClose(MsgPackReader reader)
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

        private Message ReadCancelInvocation(MsgPackReader reader)
        {
            // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#cancelinvocation-message-encoding-1

            ReadHeaders(reader);
            string invocationId = reader.ReadString();

            return new Message
            {
                type = MessageTypes.CancelInvocation,
                invocationId = invocationId
            };
        }

        private Message ReadStreamInvocation(MsgPackReader reader)
        {
            // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#streaminvocation-message-encoding-1

            ReadHeaders(reader);
            string invocationId = reader.ReadString();
            string target = reader.ReadString();
            object[] arguments = ReadArguments(reader, target);
            string[] streamIds = ReadStreamIds(reader);

            return new Message
            {
                type = MessageTypes.StreamInvocation,
                invocationId = invocationId,
                target = target,
                arguments = arguments,
                streamIds = streamIds
            };
        }

        private Message ReadCompletion(MsgPackReader reader)
        {
            // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#completion-message-encoding-1

            ReadHeaders(reader);
            string invocationId = reader.ReadString();
            byte resultKind = reader.ReadByte();

            switch(resultKind)
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
                    object item = ReadItem(reader, invocationId);
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

        private Message ReadStreamItem(MsgPackReader reader)
        {
            // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#streamitem-message-encoding-1

            ReadHeaders(reader);
            string invocationId = reader.ReadString();
            object item = ReadItem(reader, invocationId);

            return new Message
            {
                type = MessageTypes.StreamItem,
                invocationId = invocationId,
                item = item
            };
        }

        private Message ReadInvocation(MsgPackReader reader)
        {
            // https://github.com/aspnet/AspNetCore/blob/master/src/SignalR/docs/specs/HubProtocol.md#invocation-message-encoding-1

            ReadHeaders(reader);
            string invocationId = reader.ReadString();
            string target = reader.ReadString();
            object[] arguments = ReadArguments(reader, target);
            string[] streamIds = ReadStreamIds(reader);

            return new Message
            {
                type = MessageTypes.Invocation,
                invocationId = invocationId,
                target = target,
                arguments = arguments,
                streamIds = streamIds
            };
        }

        private object ReadItem(MsgPackReader reader, string invocationId)
        {
            long longId = 0;
            if (long.TryParse(invocationId, out longId))
            {
                Type itemType = this.Connection.GetItemType(longId);
                return reader.ReadValue(itemType);
            }
            else
                return reader.ReadValue(typeof(object));
        }

        private string[] ReadStreamIds(MsgPackReader reader)
        {
            return reader.ReadValue(typeof(string[])) as string[];
        }

        private object[] ReadArguments(MsgPackReader reader, string target)
        {
            var subscription = this.Connection.GetSubscription(target);

            object[] args;
            if (subscription == null || subscription.callbacks == null || subscription.callbacks.Count == 0)
            {
                args = reader.ReadValue(typeof(object[])) as object[];
            }
            else
            {
                reader.NextToken();

                if (subscription.callbacks[0].ParamTypes != null)
                {
                    args = new object[subscription.callbacks[0].ParamTypes.Length];
                    for (int i = 0; i < subscription.callbacks[0].ParamTypes.Length; ++i)
                        args[i] = reader.ReadValue(subscription.callbacks[0].ParamTypes[i]);
                }
                else
                    args = null;

                reader.NextToken();
            }

            return args;
        }

        private Dictionary<string, string> ReadHeaders(MsgPackReader reader)
        {
            return reader.ReadValue(typeof(Dictionary<string, string>)) as Dictionary<string, string>;
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
    }

    public sealed class CustomMessagePackExtensionTypeHandler : MessagePackExtensionTypeHandler
    {
        public const int EXTENSION_TYPE_DATE_TIME = -1;
        public const int DATE_TIME_SIZE = 8;

        public const long BclSecondsAtUnixEpoch = 62135596800;
        public const int NanosecondsPerTick = 100;
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly Type[] DefaultExtensionTypes = new[] { typeof(DateTime) };
        public static CustomMessagePackExtensionTypeHandler Instance = new CustomMessagePackExtensionTypeHandler();

        public override IEnumerable<Type> ExtensionTypes
        {
            get { return DefaultExtensionTypes; }
        }

        
        public override bool TryRead(sbyte type, ArraySegment<byte> data, out object value)
        {
            if (data.Array == null) throw new ArgumentNullException("data");

            value = default(object);
            switch (type)
            {
                case EXTENSION_TYPE_DATE_TIME:
                    switch (data.Count)
                    {
                        case 4:
                            {
                                var intValue = unchecked((int)(FromBytes(data.Array, data.Offset, 4)));
                                value = CustomMessagePackExtensionTypeHandler.UnixEpoch.AddSeconds(unchecked((uint)intValue));
                                return true;
                            }
                        case 8:
                            {
                                long longValue = FromBytes(data.Array, data.Offset, 8);
                                ulong ulongValue = unchecked((ulong)longValue);
                                long nanoseconds = (long)(ulongValue >> 34);
                                ulong seconds = ulongValue & 0x00000003ffffffffL;
                                value = CustomMessagePackExtensionTypeHandler.UnixEpoch.AddSeconds(seconds).AddTicks(nanoseconds / CustomMessagePackExtensionTypeHandler.NanosecondsPerTick);
                                return true;
                            }
                        case 12:
                            {
                                var intValue = unchecked((int)(FromBytes(data.Array, data.Offset, 4)));
                                long longValue = FromBytes(data.Array, data.Offset, 8);

                                var nanoseconds = unchecked((uint)intValue);
                                value = CustomMessagePackExtensionTypeHandler.UnixEpoch.AddSeconds(longValue).AddTicks(nanoseconds / CustomMessagePackExtensionTypeHandler.NanosecondsPerTick);
                                return true;
                            }
                        default:
                            throw new Exception($"Length of extension was {data.Count}. Either 4, 8 or 12 were expected.");
                    }
                default:
                    return false;
            }
        }
        
        public override bool TryWrite(object value, out sbyte type, ref ArraySegment<byte> data)
        {
            if (value == null)
            {
                type = 0;
                return false;
            }
            else if (value is DateTime)
            {
                type = EXTENSION_TYPE_DATE_TIME;

                var dateTime = (DateTime)(object)value;

                // The spec requires UTC. Convert to UTC if we're sure the value was expressed as Local time.
                // If it's Unspecified, we want to leave it alone since .NET will change the value when we convert
                // and we simply don't know, so we should leave it as-is.
                if (dateTime.Kind == DateTimeKind.Local)
                {
                    dateTime = dateTime.ToUniversalTime();
                }

                var secondsSinceBclEpoch = dateTime.Ticks / TimeSpan.TicksPerSecond;
                var seconds = secondsSinceBclEpoch - CustomMessagePackExtensionTypeHandler.BclSecondsAtUnixEpoch;
                var nanoseconds = (dateTime.Ticks % TimeSpan.TicksPerSecond) * CustomMessagePackExtensionTypeHandler.NanosecondsPerTick;

                if ((seconds >> 34) == 0)
                {
                    var data64 = unchecked((ulong)((nanoseconds << 34) | seconds));
                    if ((data64 & 0xffffffff00000000L) == 0)
                    {
                        // timestamp 32(seconds in 32-bit unsigned int)
                        var data32 = (UInt32)data64;

                        const int TIMESTAMP_SIZE = 4;

                        if (data.Array == null || data.Count < TIMESTAMP_SIZE)
                            data = new ArraySegment<byte>(new byte[TIMESTAMP_SIZE]);

                        CopyBytesImpl(data32, 4, data.Array, data.Offset);

                        if (data.Count != DATE_TIME_SIZE)
                            data = new ArraySegment<byte>(data.Array, data.Offset, DATE_TIME_SIZE);
                    }
                    else
                    {
                        // timestamp 64(nanoseconds in 30-bit unsigned int | seconds in 34-bit unsigned int)
                        const int TIMESTAMP_SIZE = 8;
                        if (data.Array == null || data.Count < TIMESTAMP_SIZE)
                            data = new ArraySegment<byte>(new byte[TIMESTAMP_SIZE]);

                        CopyBytesImpl(unchecked((long)data64), 8, data.Array, data.Offset);

                        if (data.Count != DATE_TIME_SIZE)
                            data = new ArraySegment<byte>(data.Array, data.Offset, DATE_TIME_SIZE);
                    }
                }
                else
                {
                    // timestamp 96( nanoseconds in 32-bit unsigned int | seconds in 64-bit signed int )

                    const int TIMESTAMP_SIZE = 12;

                    if (data.Array == null || data.Count < TIMESTAMP_SIZE)
                        data = new ArraySegment<byte>(new byte[TIMESTAMP_SIZE]);

                    CopyBytesImpl((uint)nanoseconds, 4, data.Array, data.Offset);
                    CopyBytesImpl(seconds, 8, data.Array, data.Offset + 4);

                    if (data.Count != DATE_TIME_SIZE)
                        data = new ArraySegment<byte>(data.Array, data.Offset, DATE_TIME_SIZE);
                }

                return true;
            }

            type = default(sbyte);
            return false;
        }

        private void CopyBytesImpl(long value, int bytes, byte[] buffer, int index)
        {
            var endOffset = index + bytes - 1;
            for (var i = 0; i < bytes; i++)
            {
                buffer[endOffset - i] = unchecked((byte)(value & 0xff));
                value = value >> 8;
            }
        }

        private long FromBytes(byte[] buffer, int startIndex, int bytesToConvert)
        {
            long ret = 0;
            for (var i = 0; i < bytesToConvert; i++)
            {
                ret = unchecked((ret << 8) | buffer[startIndex + i]);
            }
            return ret;
        }
    }

    // https://github.com/neuecc/MessagePack-CSharp/blob/13c299a5172c60154bae53395612af194c02d286/src/MessagePack.UnityClient/Assets/Scripts/MessagePack/Unity/Formatters.cs#L15
    public sealed class Vector2Serializer : TypeSerializer
    {
        public override Type SerializedType { get { return typeof(Vector2); } }

        public override object Deserialize(IJsonReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            if (reader.Token == JsonToken.Null)
                return null;

            var value = new Vector2();
            reader.ReadArrayBegin();

            int idx = 0;
            while (reader.Token != JsonToken.EndOfArray)
                value[idx++] = reader.ReadSingle();

            reader.ReadArrayEnd(nextToken: false);

            return value;
        }

        public override void Serialize(IJsonWriter writer, object value)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            if (value == null) throw new ArgumentNullException("value");

            var vector2 = (Vector2)value;
            writer.WriteArrayBegin(2);
            writer.Write(vector2.x);
            writer.Write(vector2.y);
            writer.WriteArrayEnd();
        }
    }

    // https://github.com/neuecc/MessagePack-CSharp/blob/13c299a5172c60154bae53395612af194c02d286/src/MessagePack.UnityClient/Assets/Scripts/MessagePack/Unity/Formatters.cs#L56
    public sealed class Vector3Serializer : TypeSerializer
    {
        public override Type SerializedType { get { return typeof(Vector3); } }

        public override object Deserialize(IJsonReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            if (reader.Token == JsonToken.Null)
                return null;

            var value = new Vector3();
            reader.ReadArrayBegin();

            int idx = 0;
            while (reader.Token != JsonToken.EndOfArray)
                value[idx++] = reader.ReadSingle();

            reader.ReadArrayEnd(nextToken: false);

            return value;
        }

        public override void Serialize(IJsonWriter writer, object value)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            if (value == null) throw new ArgumentNullException("value");

            var vector3 = (Vector3)value;
            writer.WriteArrayBegin(3);
            writer.Write(vector3.x);
            writer.Write(vector3.y);
            writer.Write(vector3.z);
            writer.WriteArrayEnd();
        }
    }

    // https://github.com/neuecc/MessagePack-CSharp/blob/13c299a5172c60154bae53395612af194c02d286/src/MessagePack.UnityClient/Assets/Scripts/MessagePack/Unity/Formatters.cs#L102
    public sealed class Vector4Serializer : TypeSerializer
    {
        public override Type SerializedType { get { return typeof(Vector4); } }

        public override object Deserialize(IJsonReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            if (reader.Token == JsonToken.Null)
                return null;

            var value = new Vector4();
            reader.ReadArrayBegin();

            int idx = 0;
            while (reader.Token != JsonToken.EndOfArray)
                value[idx++] = reader.ReadSingle();

            reader.ReadArrayEnd(nextToken: false);

            return value;
        }
        public override void Serialize(IJsonWriter writer, object value)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            if (value == null) throw new ArgumentNullException("value");

            var vector4 = (Vector4)value;
            writer.WriteArrayBegin(4);
            writer.Write(vector4.x);
            writer.Write(vector4.y);
            writer.Write(vector4.z);
            writer.Write(vector4.z);
            writer.WriteArrayEnd();
        }
    }
}

#endif
