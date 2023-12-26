namespace GameFrameX.Network
{
    public class PacketBase : Packet
    {
        public MessageObject MessageObject { get; set; }
        public int MessageId { get; set; }

        public override void Clear()
        {
            MessageId = 0;
        }

        public override string Id
        {
            get { return MessageId.ToString(); }
        }

        public override string ToString()
        {
            return "Packet ID: " + MessageId + ", Message: " + MessageObject;
        }
    }
}