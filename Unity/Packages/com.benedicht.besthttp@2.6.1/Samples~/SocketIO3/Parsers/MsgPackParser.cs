#if !BESTHTTP_DISABLE_SOCKETIO && BESTHTTP_SOCKETIO_ENABLE_GAMEDEVWARE_MESSAGEPACK

using System;
using System.Linq;

using BestHTTP.PlatformSupport.Memory;
using BestHTTP.SignalRCore.Encoders;
using BestHTTP.SocketIO3.Events;

using GameDevWare.Serialization;
using GameDevWare.Serialization.MessagePack;
using GameDevWare.Serialization.Serializers;

namespace BestHTTP.SocketIO3.Parsers
{
    public sealed class MsgPackParser : IParser
    {
        /// <summary>
        /// Custom function instead of char.GetNumericValue, as it throws an error under WebGL using the new 4.x runtime.
        /// It will return the value of the char if it's a numeric one, otherwise -1.
        /// </summary>
        private int ToInt(char ch)
        {
            int charValue = Convert.ToInt32(ch);
            int num = charValue - '0';
            if (num < 0 || num > 9)
                return -1;

            return num;
        }

        public IncomingPacket MergeAttachements(SocketManager manager, IncomingPacket packet) { return packet; }

        public IncomingPacket Parse(SocketManager manager, string from)
        {
            int idx = 0;
            var transportEvent = (TransportEventTypes)ToInt(from[idx++]);
            var socketIOEvent = SocketIOEventTypes.Unknown;
            var nsp = string.Empty;
            var id = -1;
            var payload = string.Empty;
            int attachments = 0;

            if (from.Length > idx && ToInt(from[idx]) >= 0)
                socketIOEvent = (SocketIOEventTypes)ToInt(from[idx++]);
            else
                socketIOEvent = SocketIOEventTypes.Unknown;

            // Parse Attachment
            if (socketIOEvent == SocketIOEventTypes.BinaryEvent || socketIOEvent == SocketIOEventTypes.BinaryAck)
            {
                int endIdx = from.IndexOf('-', idx);
                if (endIdx == -1)
                    endIdx = from.Length;

                int.TryParse(from.Substring(idx, endIdx - idx), out attachments);
                idx = endIdx + 1;
            }

            // Parse Namespace
            if (from.Length > idx && from[idx] == '/')
            {
                int endIdx = from.IndexOf(',', idx);
                if (endIdx == -1)
                    endIdx = from.Length;

                nsp = from.Substring(idx, endIdx - idx);
                idx = endIdx + 1;
            }
            else
                nsp = "/";

            // Parse Id
            if (from.Length > idx && ToInt(from[idx]) >= 0)
            {
                int startIdx = idx++;
                while (from.Length > idx && ToInt(from[idx]) >= 0)
                    idx++;

                int.TryParse(from.Substring(startIdx, idx - startIdx), out id);
            }

            // What left is the payload data
            if (from.Length > idx)
                payload = from.Substring(idx);
            else
                payload = string.Empty;

            return new IncomingPacket(transportEvent, socketIOEvent, nsp, id) { DecodedArg = payload, AttachementCount = attachments };
        }

        public IncomingPacket Parse(SocketManager manager, BufferSegment data, TransportEventTypes transportEvent = TransportEventTypes.Unknown)
        {
            using (var stream = new System.IO.MemoryStream(data.Data, data.Offset, data.Count))
            {
                var buff = BufferPool.Get(MsgPackReader.DEFAULT_BUFFER_SIZE, true);
                try
                {
                    var context = new SerializationContext
                    {
                        Options = SerializationOptions.SuppressTypeInformation/*,
                        ExtensionTypeHandler = CustomMessagePackExtensionTypeHandler.Instance*/
                    };
                    IJsonReader reader = new MsgPackReader(stream, context, Endianness.BigEndian, buff);

                    reader.ReadObjectBegin();

                    int type = -1, id = -1;
                    string nsp = null;

                    bool hasData = false, readData = false;

                    IncomingPacket packet = IncomingPacket.Empty;

                    READ:

                    while (reader.Token != JsonToken.EndOfObject)
                    {
                        string key = reader.ReadMember();

                        switch(key)
                        {
                            case "type":
                                type = reader.ReadByte();
                                break;

                            case "nsp":
                                nsp = reader.ReadString();
                                break;

                            case "id":
                                id = reader.ReadInt32();
                                break;

                            case "data":
                                if (!hasData)
                                {
                                    hasData = true;
                                    SkipData(reader, (SocketIOEventTypes)type);
                                }
                                else
                                {
                                    readData = true;

                                    packet = new IncomingPacket(transportEvent != TransportEventTypes.Unknown ? transportEvent : TransportEventTypes.Message, (SocketIOEventTypes)type, nsp, id);
                                    (string eventName, object[] args) = ReadData(manager, packet, reader);

                                    packet.EventName = eventName;
                                    if (args != null)
                                    {
                                        if (args.Length == 1)
                                            packet.DecodedArg = args[0];
                                        else
                                            packet.DecodedArgs = args;
                                    }
                                }
                                break;
                        }
                    }
                    
                    // type, nsp, id and data can come in any order. To read data strongly typed we need to know all the additional fields before processing the data field.
                    // In order to do it, when we first encounter the data field we skip it than we do a reset and an additional turn but reading the data too now.
                    if (hasData && !readData)
                    {
                        reader.Reset();
                        stream.Position = 0;
                        reader.ReadObjectBegin();

                        goto READ;
                    }

                    reader.ReadObjectEnd();

                    return packet.Equals(IncomingPacket.Empty) ? new IncomingPacket(transportEvent != TransportEventTypes.Unknown ? transportEvent : TransportEventTypes.Message, (SocketIOEventTypes)type, nsp, id) : packet;
                }
                finally
                {
                    BufferPool.Release(buff);
                }
            }
        }

        private void SkipData(IJsonReader reader, SocketIOEventTypes type)
        {
            switch (type)
            {
                case SocketIOEventTypes.Unknown:
                    // TODO: Error?
                    break;

                case SocketIOEventTypes.Connect:
                    // No Data | Object
                    SkipObject(reader);
                    break;

                case SocketIOEventTypes.Disconnect:
                    // No Data
                    break;

                case SocketIOEventTypes.Error:
                    // String | Object
                    switch(reader.Token)
                    {
                        case JsonToken.StringLiteral:
                            reader.ReadString();
                            break;

                        case JsonToken.BeginObject:
                            SkipObject(reader);
                            break;
                    }
                    break;

                default:
                    // Array
                    SkipArray(reader, false);
                    break;
            }
        }

        private object[] ReadParameters(Socket socket, Subscription subscription, IJsonReader reader)
        {
            var desc = subscription != null ? subscription.callbacks.FirstOrDefault() : default(CallbackDescriptor);
            int paramCount = desc.ParamTypes != null ? desc.ParamTypes.Length : 0;
            object[] args = null;

            if (paramCount > 0)
            {
                args = new object[paramCount];

                for (int i = 0; i < desc.ParamTypes.Length; ++i)
                {
                    Type type = desc.ParamTypes[i];

                    if (type == typeof(Socket))
                        args[i] = socket;
                    else if (type == typeof(SocketManager))
                        args[i] = socket.Manager;
                    else
                        args[i] = reader.ReadValue(desc.ParamTypes[i]);
                }
            }

            return args;
        }

        private (string, object[]) ReadData(SocketManager manager, IncomingPacket packet, IJsonReader reader)
        {
            Socket socket = manager.GetSocket(packet.Namespace);

            string eventName = packet.EventName;
            Subscription subscription = socket.GetSubscription(eventName);

            object[] args = null;

            switch (packet.SocketIOEvent)
            {
                case SocketIOEventTypes.Unknown:
                    // TODO: Error?
                    break;

                case SocketIOEventTypes.Connect:
                    // No Data | Object
                    args = ReadParameters(socket, subscription, reader);
                    //SkipObject(reader);
                    break;

                case SocketIOEventTypes.Disconnect:
                    // No Data
                    break;

                case SocketIOEventTypes.Error:
                    // String | Object
                    switch(reader.Token)
                    {
                        case JsonToken.StringLiteral:
                            args = new object[] { new Error(reader.ReadString()) };
                            break;

                        case JsonToken.BeginObject:
                            args = ReadParameters(socket, subscription, reader);
                            if (subscription == null && args == null)
                                SkipObject(reader);
                            break;
                    }
                    break;

                case SocketIOEventTypes.Ack:
                    eventName = IncomingPacket.GenerateAcknowledgementNameFromId(packet.Id);
                    subscription = socket.GetSubscription(eventName);

                    reader.ReadArrayBegin();

                    args = ReadParameters(socket, subscription, reader);

                    reader.ReadArrayEnd();
                    break;

                default:
                    // Array
                    reader.ReadArrayBegin();
                    eventName = reader.ReadString();

                    subscription = socket.GetSubscription(eventName);

                    args = ReadParameters(socket, subscription, reader);

                    reader.ReadArrayEnd();
                    break;
            }

            return (eventName, args);
        }

        private void SkipArray(IJsonReader reader, bool alreadyStarted)
        {
            if (!alreadyStarted)
                reader.ReadArrayBegin();

            int arrayBegins = 1;

            while (arrayBegins > 0)
            {
                switch (reader.Token)
                {
                    case JsonToken.BeginArray: arrayBegins++; break;
                    case JsonToken.EndOfArray: arrayBegins--; break;
                }
                reader.NextToken();
            }
        }

        private void SkipObject(IJsonReader reader)
        {
            reader.ReadObjectBegin();
            int objectBegins = 1;

            while (objectBegins > 0)
            {
                switch (reader.Token)
                {
                    case JsonToken.BeginObject: objectBegins++; break;
                    case JsonToken.EndOfObject: objectBegins--; break;
                }
                reader.NextToken();
            }
        }

        public OutgoingPacket CreateOutgoing(TransportEventTypes transportEvent, string payload)
        {
            return new OutgoingPacket { Payload = "" + (char)('0' + (byte)transportEvent) + payload };
        }

        public OutgoingPacket CreateOutgoing(Socket socket, SocketIOEventTypes socketIOEvent, int id, string name, object arg)
        {
            return CreateOutgoing(socket, socketIOEvent, id, name, arg != null ? new object[] { arg } : null);
        }

        public OutgoingPacket CreateOutgoing(Socket socket, SocketIOEventTypes socketIOEvent, int id, string name, object[] args)
        {
            var memBuffer = BufferPool.Get(256, true);
            var stream = new BestHTTP.Extensions.BufferPoolMemoryStream(memBuffer, 0, memBuffer.Length, true, true, false, true);

            var buffer = BufferPool.Get(MsgPackWriter.DEFAULT_BUFFER_SIZE, true);

            var context = new SerializationContext
            {
                Options = SerializationOptions.SuppressTypeInformation,
                EnumSerializerFactory = (enumType) => new EnumNumberSerializer(enumType)/*,
                ExtensionTypeHandler = CustomMessagePackExtensionTypeHandler.Instance*/
            };

            var writer = new MsgPackWriter(stream, context, buffer);

            switch (socketIOEvent)
            {
                case SocketIOEventTypes.Connect:
                    // No Data | Object
                    if (args != null && args.Length > 0 && args[0] != null)
                        writer.WriteObjectBegin(id >= 0 ? 4 : 3);
                    else
                        writer.WriteObjectBegin(id >= 0 ? 3 : 2);
                    break;

                case SocketIOEventTypes.Disconnect: writer.WriteObjectBegin(id > 0 ? 3 : 2); break;
                case SocketIOEventTypes.Error: writer.WriteObjectBegin(id > 0 ? 4 : 3); break;
                default: writer.WriteObjectBegin(id >= 0 ? 4 : 3); break;
            }

            writer.WriteMember("type");
            writer.Write((int)socketIOEvent);

            writer.WriteMember("nsp");
            writer.Write(socket.Namespace);

            if (id >= 0)
            {
                writer.WriteMember("id");
                writer.Write(id);
            }

            switch (socketIOEvent)
            {
                case SocketIOEventTypes.Connect:
                    // No Data | Object
                    if (args != null && args.Length > 0 && args[0] != null)
                    {
                        writer.WriteMember("data");
                        writer.WriteValue(args[0], args[0].GetType());
                    }
                    break;

                case SocketIOEventTypes.Disconnect:
                    // No Data
                    break;

                case SocketIOEventTypes.Error:
                    writer.WriteMember("data");

                    // String | Object
                    if (args != null && args.Length > 0)
                        writer.WriteValue(args[0], args[0].GetType());
                    else
                    {
                        writer.WriteObjectBegin(0);
                        writer.WriteObjectEnd();
                    }
                    break;

                default:
                    writer.WriteMember("data");

                    // Array

                    int argCount = (args != null ? args.Length : 0);
                    writer.WriteArrayBegin(!string.IsNullOrEmpty(name) ? 1 + argCount : argCount);

                    if (!string.IsNullOrEmpty(name))
                        writer.Write(name);
                    
                    foreach (var arg in args)
                        writer.WriteValue(arg, arg.GetType());

                    writer.WriteArrayEnd();
                    break;
            }

            writer.WriteObjectEnd();

            writer.Flush();

            BufferPool.Release(buffer);

            // get how much bytes got written to the buffer
            int length = (int)stream.Position;

            buffer = stream.GetBuffer();
            return new OutgoingPacket { PayloadData = new BufferSegment(buffer, 0, length) };
        }
    }
}

#endif
