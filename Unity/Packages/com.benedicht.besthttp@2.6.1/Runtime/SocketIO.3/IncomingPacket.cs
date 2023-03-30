#if !BESTHTTP_DISABLE_SOCKETIO

namespace BestHTTP.SocketIO3
{
    using System.Collections.Generic;

    using BestHTTP.PlatformSupport.Memory;
    using BestHTTP.SocketIO3.Events;

    public struct OutgoingPacket
    {
        public bool IsBinary { get { return string.IsNullOrEmpty(this.Payload); } }

        public string Payload { get; set; }
        public List<byte[]> Attachements { get; set; }

        public BufferSegment PayloadData { get; set; }

        public bool IsVolatile { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Payload))
                return this.Payload;
            else
                return this.PayloadData.ToString();
        }
    }

    public struct IncomingPacket
    {
        public static readonly IncomingPacket Empty = new IncomingPacket(TransportEventTypes.Unknown, SocketIOEventTypes.Unknown, null, -1);

        /// <summary>
        /// Event type of this packet on the transport layer.
        /// </summary>
        public TransportEventTypes TransportEvent { get; private set; }

        /// <summary>
        /// The packet's type in the Socket.IO protocol.
        /// </summary>
        public SocketIOEventTypes SocketIOEvent { get; private set; }

        /// <summary>
        /// The internal ack-id of this packet.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// The sender namespace's name.
        /// </summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// Count of binary data expected after the current packet.
        /// </summary>
        public int AttachementCount { get; set; }

        /// <summary>
        /// list of binary data received.
        /// </summary>
        public List<BufferSegment> Attachements { get; set; }

        /// <summary>
        /// The decoded event name from the payload string.
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// The decoded arguments by the parser.
        /// </summary>
        public object[] DecodedArgs { get; set; }

        public object DecodedArg { get; set; }

        public IncomingPacket(TransportEventTypes transportEvent, SocketIOEventTypes packetType, string nsp, int id)
        {
            this.TransportEvent = transportEvent;
            this.SocketIOEvent = packetType;
            this.Namespace = nsp;
            this.Id = id;

            this.AttachementCount = 0;
            //this.ReceivedAttachements = 0;
            this.Attachements = null;

            if (this.SocketIOEvent != SocketIOEventTypes.Unknown)
                this.EventName = EventNames.GetNameFor(this.SocketIOEvent);
            else
                this.EventName = EventNames.GetNameFor(this.TransportEvent);

            this.DecodedArg = this.DecodedArgs = null;
        }

        /// <summary>
        /// Returns with the Payload of this packet.
        /// </summary>
        public override string ToString()
        {
            return string.Format("[Packet {0}{1}/{2},{3}[{4}]]", this.TransportEvent, this.SocketIOEvent, this.Namespace, this.Id, this.EventName);
        }

        public override bool Equals(object obj)
        {
            if (obj is IncomingPacket)
                return Equals((IncomingPacket)obj);

            return false;
        }

        public bool Equals(IncomingPacket packet)
        {
            return this.TransportEvent == packet.TransportEvent &&
                this.SocketIOEvent == packet.SocketIOEvent &&
                this.Id == packet.Id &&
                this.Namespace == packet.Namespace &&
                this.EventName == packet.EventName &&
                this.DecodedArg == packet.DecodedArg &&
                this.DecodedArgs == packet.DecodedArgs;
        }

        public override int GetHashCode()
        {
            int hashCode = -1860921451;
            hashCode = hashCode * -1521134295 + TransportEvent.GetHashCode();
            hashCode = hashCode * -1521134295 + SocketIOEvent.GetHashCode();
            hashCode = hashCode * -1521134295 + Id.GetHashCode();

            if (Namespace != null)
                hashCode = hashCode * -1521134295 + Namespace.GetHashCode();

            if (EventName != null)
                hashCode = hashCode * -1521134295 + EventName.GetHashCode();

            if (DecodedArgs != null)
                hashCode = hashCode * -1521134295 + DecodedArgs.GetHashCode();

            if (DecodedArg != null)
                hashCode = hashCode * -1521134295 + DecodedArg.GetHashCode();

            return hashCode;
        }

        public static string GenerateAcknowledgementNameFromId(int id)
        {
            return string.Concat("Generated Callback Name for Id: ##", id.ToString(), "##");
        }
    }
}

#endif
