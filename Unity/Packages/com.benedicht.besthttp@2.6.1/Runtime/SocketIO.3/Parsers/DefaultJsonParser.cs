#if !BESTHTTP_DISABLE_SOCKETIO
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BestHTTP.PlatformSupport.Memory;
using BestHTTP.SocketIO3.Events;

namespace BestHTTP.SocketIO3.Parsers
{
    public sealed class Placeholder
    {
        public bool _placeholder;
        public int num;
    }

    [PlatformSupport.IL2CPP.Il2CppEagerStaticClassConstructionAttribute]
    public sealed class DefaultJsonParser : IParser
    {
        static DefaultJsonParser()
        {
            BestHTTP.JSON.LitJson.JsonMapper.RegisterImporter<string, byte[]>(str => Convert.FromBase64String(str));
        }

        private IncomingPacket PacketWithAttachment = IncomingPacket.Empty;

        private int ToInt(char ch)
        {
            int charValue = Convert.ToInt32(ch);
            int num = charValue - '0';
            if (num < 0 || num > 9)
                return -1;

            return num;
        }

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

            var packet = new IncomingPacket(transportEvent, socketIOEvent, nsp, id);
            packet.AttachementCount = attachments;

            string eventName = packet.EventName;
            object[] args = null;

            switch (socketIOEvent)
            {
                case SocketIOEventTypes.Unknown:
                    packet.DecodedArg = payload;
                    break;

                case SocketIOEventTypes.Connect:
                    // No Data | Object
                    if (!string.IsNullOrEmpty(payload))
                        (eventName, args) = ReadData(manager, packet, payload);
                    break;

                case SocketIOEventTypes.Disconnect:
                    // No Data
                    break;

                case SocketIOEventTypes.Error:
                    // String | Object
                    (eventName, args) = ReadData(manager, packet, payload);
                    break;

                default:
                    // Array
                    (eventName, args) = ReadData(manager, packet, payload);
                    // Save payload until all attachments arrive
                    if (packet.AttachementCount > 0)
                        packet.DecodedArg = payload;
                    break;
            }

            packet.EventName = eventName;
            
            if (args != null)
            {
                if (args.Length == 1)
                    packet.DecodedArg = args[0];
                else
                    packet.DecodedArgs = args;
            }

            if (packet.AttachementCount > 0)
            {
                PacketWithAttachment = packet;
                return IncomingPacket.Empty;
            }

            return packet;
        }

        public IncomingPacket MergeAttachements(SocketManager manager, IncomingPacket packet)
        {
            string payload = packet.DecodedArg as string;
            packet.DecodedArg = null;

            string placeholderFormat = "{{\"_placeholder\":true,\"num\":{0}}}";

            for (int i = 0; i < packet.Attachements.Count; ++i)
            {
                string placeholder = string.Format(placeholderFormat, i);
                BufferSegment data = packet.Attachements[i];

                payload = payload.Replace(placeholder, "\"" + Convert.ToBase64String(data.Data, data.Offset, data.Count) + "\"");
            }

            (string eventName, object[] args) = ReadData(manager, packet, payload);

            packet.EventName = eventName;
            
            if (args != null)
            {
                if (args.Length == 1)
                    packet.DecodedArg = args[0];
                else
                    packet.DecodedArgs = args;
            }

            return packet;
        }

        private (string, object[]) ReadData(SocketManager manager, IncomingPacket packet, string payload)
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
                    using (var strReader = new System.IO.StringReader(payload))
                        args = ReadParameters(socket, subscription, strReader);
                    break;

                case SocketIOEventTypes.Disconnect:
                    // No Data
                    break;

                case SocketIOEventTypes.Error:
                    // String | Object
                    switch (payload[0])
                    {
                        case '{':
                            using (var strReader = new System.IO.StringReader(payload))
                                args = ReadParameters(socket, subscription, strReader);
                            break;

                        default:
                            args = new object[] { new Error(payload) };
                            break;

                    }
                    break;

                case SocketIOEventTypes.Ack:
                    eventName = IncomingPacket.GenerateAcknowledgementNameFromId(packet.Id);
                    subscription = socket.GetSubscription(eventName);

                    args = ReadParameters(socket, subscription, JSON.LitJson.JsonMapper.ToObject<List<object>>(payload), 0);
                    
                    break;

                default:
                    // Array

                    List<object> array = null;
                    using (var reader = new System.IO.StringReader(payload))
                        array = JSON.LitJson.JsonMapper.ToObject<List<object>>(new JSON.LitJson.JsonReader(reader));

                    if (array.Count > 0)
                    {
                        eventName = array[0].ToString();
                        subscription = socket.GetSubscription(eventName);
                    }

                    if (packet.AttachementCount == 0 || packet.Attachements != null)
                    {
                        try
                        {
                            args = ReadParameters(socket, subscription, array, 1);
                        }
                        catch(Exception ex)
                        {
                            HTTPManager.Logger.Exception("DefaultJsonParser", string.Format("ReadParameters with eventName: {0}", eventName), ex);
                        }
                    }

                    break;
            }

            return (eventName, args);
        }

        private object[] ReadParameters(Socket socket, Subscription subscription, List<object> array, int startIdx)
        {
            object[] args = null;

            if (array.Count > startIdx)
            {
                var desc = subscription != null ? subscription.callbacks.FirstOrDefault() : default(CallbackDescriptor);
                int paramCount = desc.ParamTypes != null ? desc.ParamTypes.Length : 0;

                int arrayIdx = startIdx;
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
                        else if (type == typeof(Placeholder))
                            args[i] = new Placeholder();
                        else
                            args[i] = ConvertTo(desc.ParamTypes[i], array[arrayIdx++]);
                    }
                }
            }

            return args;
        }

        public object ConvertTo(Type toType, object obj)
        {
            if (obj == null)
                return null;

#if NETFX_CORE
            TypeInfo objType = obj.GetType().GetTypeInfo();
#else
            Type objType = obj.GetType();
#endif

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

#if NETFX_CORE
            if (objType.Equals(typeInfo))
#else
            if (objType.Equals(toType))
#endif
                return obj;

            if (toType == typeof(byte[]) && objType == typeof(string))
                return Convert.FromBase64String(obj.ToString());

            return JSON.LitJson.JsonMapper.ToObject(toType, JSON.LitJson.JsonMapper.ToJson(obj));
        }

        private object[] ReadParameters(Socket socket, Subscription subscription, System.IO.TextReader reader)
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
                    else {
                        BestHTTP.JSON.LitJson.JsonReader jr = new JSON.LitJson.JsonReader(reader);
                        args[i] = JSON.LitJson.JsonMapper.ToObject(desc.ParamTypes[i], jr);
                        reader.Read();
                    }
                }
            }

            return args;
        }

        public IncomingPacket Parse(SocketManager manager, BufferSegment data, TransportEventTypes transportEvent = TransportEventTypes.Unknown)
        {
            IncomingPacket packet = IncomingPacket.Empty;

            if (PacketWithAttachment.Attachements == null)
                PacketWithAttachment.Attachements = new List<BufferSegment>(PacketWithAttachment.AttachementCount);
            PacketWithAttachment.Attachements.Add(data);
            
            if (PacketWithAttachment.Attachements.Count == PacketWithAttachment.AttachementCount)
            {
                packet = manager.Parser.MergeAttachements(manager, PacketWithAttachment);
                PacketWithAttachment = IncomingPacket.Empty;
            }

            return packet;
        }

        public OutgoingPacket CreateOutgoing(TransportEventTypes transportEvent, string payload)
        {
            return new OutgoingPacket { Payload = "" + (char)('0' + (byte)transportEvent) + payload };
        }

        private StringBuilder builder = new StringBuilder();
        public OutgoingPacket CreateOutgoing(Socket socket, SocketIOEventTypes socketIOEvent, int id, string name, object arg)
        {
            return CreateOutgoing(socket, socketIOEvent, id, name, arg != null ? new object[] { arg } : null);
        }

        private int GetBinaryCount(object[] args)
        {
            if (args == null || args.Length == 0)
                return 0;

            int count = 0;
            for (int i = 0; i < args.Length; ++i)
                if (args[i] is byte[])
                    count++;

            return count;
        }
        public OutgoingPacket CreateOutgoing(Socket socket, SocketIOEventTypes socketIOEvent, int id, string name, object[] args)
        {
            builder.Length = 0;
            List<byte[]> attachements = null;

            switch(socketIOEvent)
            {
                case SocketIOEventTypes.Ack:
                    if (GetBinaryCount(args) > 0)
                    {
                        attachements = CreatePlaceholders(args);
                        socketIOEvent = SocketIOEventTypes.BinaryAck;
                    }
                    break;

                case SocketIOEventTypes.Event:
                    if (GetBinaryCount(args) > 0)
                    {
                        attachements = CreatePlaceholders(args);
                        socketIOEvent = SocketIOEventTypes.BinaryEvent;
                    }
                    break;
            }

            builder.Append(((int)TransportEventTypes.Message).ToString());
            builder.Append(((int)socketIOEvent).ToString());

            if (socketIOEvent == SocketIOEventTypes.BinaryEvent || socketIOEvent == SocketIOEventTypes.BinaryAck)
            {
                builder.Append(attachements.Count.ToString());
                builder.Append('-');
            }

            // Add the namespace. If there is any other then the root nsp ("/")
            // then we have to add a trailing "," if we have more data.
            bool nspAdded = false;
            if (socket.Namespace != "/")
            {
                builder.Append(socket.Namespace);
                nspAdded = true;
            }

            // ack id, if any
            if (id >= 0)
            {
                if (nspAdded)
                {
                    builder.Append(',');
                    nspAdded = false;
                }

                builder.Append(id.ToString());
            }

            // payload
            switch (socketIOEvent)
            {
                case SocketIOEventTypes.Connect:
                    // No Data | Object
                    if (args != null && args.Length > 0)
                    {
                        if (nspAdded) builder.Append(',');

                        builder.Append(BestHTTP.JSON.LitJson.JsonMapper.ToJson(args[0]));
                    }
                    break;

                case SocketIOEventTypes.Disconnect:
                    // No Data
                    break;

                case SocketIOEventTypes.Error:
                    // String | Object
                    if (args != null && args.Length > 0)
                    {
                        if (nspAdded) builder.Append(',');

                        builder.Append(BestHTTP.JSON.LitJson.JsonMapper.ToJson(args[0]));
                    }
                    break;

                case SocketIOEventTypes.Ack:
                case SocketIOEventTypes.BinaryAck:
                    if (nspAdded) builder.Append(',');

                    if (args != null && args.Length > 0)
                    {
                        var argsJson = JSON.LitJson.JsonMapper.ToJson(args);
                        builder.Append(argsJson);
                    }
                    else
                        builder.Append("[]");
                    break;

                default:
                    if (nspAdded) builder.Append(',');

                    // Array
                    builder.Append('[');
                    if (!string.IsNullOrEmpty(name))
                    {
                        builder.Append('\"');
                        builder.Append(name);
                        builder.Append('\"');
                    }

                    if (args != null && args.Length > 0)
                    {
                        builder.Append(',');
                        var argsJson = JSON.LitJson.JsonMapper.ToJson(args);
                        builder.Append(argsJson, 1, argsJson.Length - 2);                        
                    }

                    builder.Append(']');
                    break;
            }

            return new OutgoingPacket { Payload = builder.ToString(), Attachements = attachements };
        }

        private List<byte[]> CreatePlaceholders(object[] args)
        {
            List<byte[]> attachements = null;

            for (int i = 0; i < args.Length; ++i)
            {
                var binary = args[i] as byte[];
                if (binary != null)
                {
                    if (attachements == null)
                        attachements = new List<byte[]>();
                    attachements.Add(binary);

                    args[i] = new Placeholder { _placeholder = true, num = attachements.Count - 1 };
                }
            }

            return attachements;
        }
    }
}
#endif
