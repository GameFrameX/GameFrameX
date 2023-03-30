#if !BESTHTTP_DISABLE_SOCKETIO

using System.Text;

namespace BestHTTP.SocketIO
{
    using System;
    using System.Collections.Generic;
    using BestHTTP.JSON;

    public sealed class Packet
    {
        private enum PayloadTypes : byte
        {
            Textual = 0,
            Binary = 1
        }

        public const string Placeholder = "_placeholder";

        #region Public properties

        /// <summary>
        /// Event type of this packet on the transport layer.
        /// </summary>
        public TransportEventTypes TransportEvent { get; private set; }

        /// <summary>
        /// The packet's type in the Socket.IO protocol.
        /// </summary>
        public SocketIOEventTypes SocketIOEvent { get; private set; }

        /// <summary>
        /// How many attachment should have this packet.
        /// </summary>
        public int AttachmentCount { get; private set; }

        /// <summary>
        /// The internal ack-id of this packet.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// The sender namespace's name.
        /// </summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// The payload as a Json string.
        /// </summary>
        public string Payload { get; private set; }

        /// <summary>
        /// The decoded event name from the payload string.
        /// </summary>
        public string EventName { get; private set; }

        /// <summary>
        /// All binary data attached to this event.
        /// </summary>
        public List<byte[]> Attachments { get { return attachments; } set { attachments = value; AttachmentCount = attachments != null ? attachments.Count : 0; } }
        private List<byte[]> attachments;

        /// <summary>
        /// Property to check whether all attachments are received to this packet.
        /// </summary>
        public bool HasAllAttachment { get { return Attachments != null && Attachments.Count == AttachmentCount; } }

        /// <summary>
        /// True if it's already decoded. The DecodedArgs still can be null after the Decode call.
        /// </summary>
        public bool IsDecoded { get; private set; }

        /// <summary>
        /// The decoded arguments from the result of a Json string -> c# object convert.
        /// </summary>
        public object[] DecodedArgs { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Internal constructor. Don't use it directly!
        /// </summary>
        internal Packet()
        {
            this.TransportEvent = TransportEventTypes.Unknown;
            this.SocketIOEvent = SocketIOEventTypes.Unknown;
            this.Payload = string.Empty;
        }

        /// <summary>
        /// Internal constructor. Don't use it directly!
        /// </summary>
        internal Packet(string from)
        {
            this.Parse(from);
        }

        /// <summary>
        /// Internal constructor. Don't use it directly!
        /// </summary>
        public Packet(TransportEventTypes transportEvent, SocketIOEventTypes packetType, string nsp, string payload, int attachment = 0, int id = 0)
        {
            this.TransportEvent = transportEvent;
            this.SocketIOEvent = packetType;
            this.Namespace = nsp;
            this.Payload = payload;
            this.AttachmentCount = attachment;
            this.Id = id;
        }

        #endregion

        #region Public Functions

        public object[] Decode(BestHTTP.SocketIO.JsonEncoders.IJsonEncoder encoder)
        {
            if (IsDecoded || encoder == null)
                return DecodedArgs;

            IsDecoded = true;

            if (string.IsNullOrEmpty(Payload))
                return DecodedArgs;

            List<object> decoded = encoder.Decode(Payload);

            if (decoded != null && decoded.Count > 0)
            {
                if (this.SocketIOEvent == SocketIOEventTypes.Ack || this.SocketIOEvent == SocketIOEventTypes.BinaryAck)
                    DecodedArgs = decoded.ToArray();
                else
                {
                    decoded.RemoveAt(0);

                    DecodedArgs = decoded.ToArray();
                }
            }

            return DecodedArgs;
        }

        /// <summary>
        /// Will set and return with the EventName from the packet's Payload string.
        /// </summary>
        public string DecodeEventName()
        {
            // Already decoded
            if (!string.IsNullOrEmpty(EventName))
                return EventName;

            // No Payload to decode
            if (string.IsNullOrEmpty(Payload))
                return string.Empty;

            // Not array encoded, we can't decode
            if (Payload[0] != '[')
                return string.Empty;

            int idx = 1;

            // Search for the string-begin mark( ' or " chars)
            while (Payload.Length > idx && Payload[idx] != '"' && Payload[idx] != '\'')
                idx++;

            // Reached the end of the string
            if (Payload.Length <= idx)
                return string.Empty;

            int startIdx = ++idx;

            // Search for the trailing mark of the string
            while (Payload.Length > idx && Payload[idx] != '"' && Payload[idx] != '\'')
                idx++;

            // Reached the end of the string
            if (Payload.Length <= idx)
                return string.Empty;

            return EventName = Payload.Substring(startIdx, idx - startIdx);
        }

        public string RemoveEventName(bool removeArrayMarks)
        {
            // No Payload to decode
            if (string.IsNullOrEmpty(Payload))
                return string.Empty;

            // Not array encoded, we can't decode
            if (Payload[0] != '[')
                return string.Empty;

            int idx = 1;

            // Search for the string-begin mark( ' or " chars)
            while (Payload.Length > idx && Payload[idx] != '"' && Payload[idx] != '\'')
                idx++;

            // Reached the end of the string
            if (Payload.Length <= idx)
                return string.Empty;

            int startIdx = idx;

            // Search for end of first element, or end of the array marks
            while (Payload.Length > idx && Payload[idx] != ',' && Payload[idx] != ']')
                idx++;

            // Reached the end of the string
            if (Payload.Length <= ++idx)
                return string.Empty;

            string payload = Payload.Remove(startIdx, idx - startIdx);

            if (removeArrayMarks)
                payload = payload.Substring(1, payload.Length - 2);

            return payload;
        }

        /// <summary>
        /// Will switch the "{'_placeholder':true,'num':X}" to a the index num X.
        /// </summary>
        /// <returns>True if successfully reconstructed, false otherwise.</returns>
        public bool ReconstructAttachmentAsIndex()
        {
            //"452-["multiImage",{"image":true,"buffer1":{"_placeholder":true,"num":0},"buffer2":{"_placeholder":true,"num":1}}]"

            return PlaceholderReplacer((json, obj) =>
            {
                int idx = Convert.ToInt32(obj["num"]);
                this.Payload = this.Payload.Replace(json, idx.ToString());
                this.IsDecoded = false;
            });
        }

        /// <summary>
        /// Will switch the "{'_placeholder':true,'num':X}" to a the data as a base64 encoded string.
        /// </summary>
        /// <returns>True if successfully reconstructed, false otherwise.</returns>
        public bool ReconstructAttachmentAsBase64()
        {
            //"452-["multiImage",{"image":true,"buffer1":{"_placeholder":true,"num":0},"buffer2":{"_placeholder":true,"num":1}}]"

            if (!HasAllAttachment)
                return false;

            return PlaceholderReplacer((json, obj) =>
            {
                int idx = Convert.ToInt32(obj["num"]);
                this.Payload = this.Payload.Replace(json, string.Format("\"{0}\"", Convert.ToBase64String(this.Attachments[idx])));
                this.IsDecoded = false;
            });
        }

        #endregion

        #region Internal Functions

        /// <summary>
        /// Parse the packet from a server sent textual data. The Payload will be the raw json string.
        /// </summary>
        internal void Parse(string from)
        {
            int idx = 0;
            this.TransportEvent = (TransportEventTypes)ToInt(from[idx++]);

            if (from.Length > idx && ToInt(from[idx]) >= 0)
                this.SocketIOEvent = (SocketIOEventTypes)ToInt(from[idx++]);
            else
                this.SocketIOEvent = SocketIOEventTypes.Unknown;

            // Parse Attachment
            if (this.SocketIOEvent == SocketIOEventTypes.BinaryEvent || this.SocketIOEvent == SocketIOEventTypes.BinaryAck)
            {
                int endIdx = from.IndexOf('-', idx);
                if (endIdx == -1)
                    endIdx = from.Length;

                int attachment = 0;
                int.TryParse(from.Substring(idx, endIdx - idx), out attachment);
                this.AttachmentCount = attachment;
                idx = endIdx + 1;
            }

            // Parse Namespace
            if (from.Length > idx && from[idx] == '/')
            {
                int endIdx = from.IndexOf(',', idx);
                if (endIdx == -1)
                    endIdx = from.Length;

                this.Namespace = from.Substring(idx, endIdx - idx);
                idx = endIdx + 1;
            }
            else
                this.Namespace = "/";

            // Parse Id
            if (from.Length > idx && ToInt(from[idx]) >= 0)
            {
                int startIdx = idx++;
                while (from.Length > idx && ToInt(from[idx]) >= 0)
                    idx++;

                int id = 0;
                int.TryParse(from.Substring(startIdx, idx - startIdx), out id);
                this.Id = id;
            }

            // What left is the payload data
            if (from.Length > idx)
                this.Payload = from.Substring(idx);
            else
                this.Payload = string.Empty;
        }

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

        /// <summary>
        /// Encodes this packet to a Socket.IO formatted string.
        /// </summary>
        internal string Encode()
        {
            StringBuilder builder = new StringBuilder();

            // Set to Message if not set, and we are sending attachments
            if (this.TransportEvent == TransportEventTypes.Unknown && this.AttachmentCount > 0)
                this.TransportEvent = TransportEventTypes.Message;

            if (this.TransportEvent != TransportEventTypes.Unknown)
                builder.Append(((int)this.TransportEvent).ToString());

            // Set to BinaryEvent if not set, and we are sending attachments
            if (this.SocketIOEvent == SocketIOEventTypes.Unknown && this.AttachmentCount > 0)
                this.SocketIOEvent = SocketIOEventTypes.BinaryEvent;

            if (this.SocketIOEvent != SocketIOEventTypes.Unknown)
                builder.Append(((int)this.SocketIOEvent).ToString());

            if (this.SocketIOEvent == SocketIOEventTypes.BinaryEvent || this.SocketIOEvent == SocketIOEventTypes.BinaryAck)
            {
                builder.Append(this.AttachmentCount.ToString());
                builder.Append("-");
            }

            // Add the namespace. If there is any other then the root nsp ("/")
            // then we have to add a trailing "," if we have more data.
            bool nspAdded = false;
            if (this.Namespace != "/")
            {
                builder.Append(this.Namespace);
                nspAdded = true;
            }

            // ack id, if any
            if (this.Id != 0)
            {
                if (nspAdded)
                {
                    builder.Append(",");
                    nspAdded = false;
                }

                builder.Append(this.Id.ToString());
            }

            // payload
            if (!string.IsNullOrEmpty(this.Payload))
            {
                if (nspAdded)
                {
                    builder.Append(",");
                    nspAdded = false;
                }

                builder.Append(this.Payload);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Encodes this packet to a Socket.IO formatted byte array.
        /// </summary>
        internal byte[] EncodeBinary()
        {
            if (AttachmentCount != 0 || (Attachments != null && Attachments.Count != 0))
            {
                if (Attachments == null)
                    throw new ArgumentException("packet.Attachments are null!");

                if (AttachmentCount != Attachments.Count)
                    throw new ArgumentException("packet.AttachmentCount != packet.Attachments.Count. Use the packet.AddAttachment function to add data to a packet!");
            }

            // Encode it as usual
            string encoded = Encode();

            // Convert it to a byte[]
            byte[] payload = Encoding.UTF8.GetBytes(encoded);

            // Encode it to a message
            byte[] buffer = EncodeData(payload, PayloadTypes.Textual, null);

            // If there is any attachment, convert them too, and append them after each other
            if (AttachmentCount != 0)
            {
                int idx = buffer.Length;

                // List to temporarily hold the converted attachments
                List<byte[]> attachmentDatas = new List<byte[]>(AttachmentCount);

                // The sum size of the converted attachments to be able to resize our buffer only once. This way we can avoid some GC garbage
                int attachmentDataSize = 0;

                // Encode our attachments, and store them in our list
                for (int i = 0; i < AttachmentCount; i++)
                {
                    byte[] tmpBuff = EncodeData(Attachments[i], PayloadTypes.Binary, new byte[] { 4 });
                    attachmentDatas.Add(tmpBuff);

                    attachmentDataSize += tmpBuff.Length;
                }

                // Resize our buffer once
                Array.Resize(ref buffer, buffer.Length + attachmentDataSize);

                // And copy all data into it
                for (int i = 0; i < AttachmentCount; ++i)
                {
                    byte[] data = attachmentDatas[i];
                    Array.Copy(data, 0, buffer, idx, data.Length);

                    idx += data.Length;
                }
            }

            // Return the buffer
            return buffer;
        }

        /// <summary>
        /// Will add the byte[] that the server sent to the attachments list.
        /// </summary>
        internal void AddAttachmentFromServer(byte[] data, bool copyFull)
        {
            if (data == null || data.Length == 0)
                return;

            if (this.attachments == null)
                this.attachments = new List<byte[]>(this.AttachmentCount);

            if (copyFull)
                this.Attachments.Add(data);
            else
            {
                byte[] buff = new byte[data.Length - 1];
                Array.Copy(data, 1, buff, 0, data.Length - 1);

                this.Attachments.Add(buff);
            }
        }

        #endregion

        #region Private Helper Functions

        /// <summary>
        /// Encodes a byte array to a Socket.IO binary encoded message
        /// </summary>
        private byte[] EncodeData(byte[] data, PayloadTypes type, byte[] afterHeaderData)
        {
            // Packet binary encoding:
            // [          0|1         ][            length of data           ][    FF    ][data]
            // <1 = binary, 0 = string><number from 0-9><number from 0-9>[...]<number 255><data>

            // Get the length of the payload. Socket.IO uses a wasteful encoding to send the length of the data.
            // If the data is 16 bytes we have to send the length as two bytes: byte value of the character '1' and byte value of the character '6'.
            // Instead of just one byte: 0xF. If the payload is 123 bytes, we can't send as 0x7B...
            int afterHeaderLength = (afterHeaderData != null ? afterHeaderData.Length : 0);
            string lenStr = (data.Length + afterHeaderLength).ToString();
            byte[] len = new byte[lenStr.Length];
            for (int cv = 0; cv < lenStr.Length; ++cv)
                len[cv] = (byte)char.GetNumericValue(lenStr[cv]);

            // We need another buffer to store the final data
            byte[] buffer = new byte[data.Length + len.Length + 2 + afterHeaderLength];

            // The payload is textual -> 0
            buffer[0] = (byte)type;

            // Copy the length of the data
            for (int cv = 0; cv < len.Length; ++cv)
                buffer[1 + cv] = len[cv];

            int idx = 1 + len.Length;

            // End of the header data
            buffer[idx++] = 0xFF;

            if (afterHeaderData != null && afterHeaderData.Length > 0)
            {
                Array.Copy(afterHeaderData, 0, buffer, idx, afterHeaderData.Length);
                idx += afterHeaderData.Length;
            }

            // Copy our payload data to the buffer
            Array.Copy(data, 0, buffer, idx, data.Length);

            return buffer;
        }

        /// <summary>
        /// Searches for the "{'_placeholder':true,'num':X}" string, and will call the given action to modify the PayLoad
        /// </summary>
        private bool PlaceholderReplacer(Action<string, Dictionary<string, object>> onFound)
        {
            if (string.IsNullOrEmpty(this.Payload))
                return false;

            // Find the first index of the "_placeholder" str
            int placeholderIdx = this.Payload.IndexOf(Placeholder);

            while (placeholderIdx >= 0)
            {
                // Find the object-start token
                int startIdx = placeholderIdx;
                while (this.Payload[startIdx] != '{')
                    startIdx--;

                // Find the object-end token
                int endIdx = placeholderIdx;
                while (this.Payload.Length > endIdx && this.Payload[endIdx] != '}')
                    endIdx++;

                // We reached the end
                if (this.Payload.Length <= endIdx)
                    return false;

                // Get the object, and decode it
                string placeholderJson = this.Payload.Substring(startIdx, endIdx - startIdx + 1);
                bool success = false;
                Dictionary<string, object> obj = Json.Decode(placeholderJson, ref success) as Dictionary<string, object>;
                if (!success)
                    return false;

                // Check for presence and value of _placeholder
                object value;
                if (!obj.TryGetValue(Placeholder, out value) ||
                    !(bool)value)
                    return false;

                // Check for presence of num
                if (!obj.TryGetValue("num", out value))
                    return false;

                // Let do, what we have to do
                onFound(placeholderJson, obj);

                // Find the next attachment if there is any
                placeholderIdx = this.Payload.IndexOf(Placeholder);
            }

            return true;
        }

        #endregion

        #region Overrides and Interface Implementations

        /// <summary>
        /// Returns with the Payload of this packet.
        /// </summary>
        public override string ToString()
        {
            return this.Payload;
        }

        /// <summary>
        /// Will clone this packet to an identical packet instance.
        /// </summary>
        internal Packet Clone()
        {
            Packet packet = new Packet(this.TransportEvent, this.SocketIOEvent, this.Namespace, this.Payload, 0, this.Id);
            packet.EventName = this.EventName;
            packet.AttachmentCount = this.AttachmentCount;
            packet.attachments = this.attachments;

            return packet;
        }

        #endregion
    }
}

#endif
