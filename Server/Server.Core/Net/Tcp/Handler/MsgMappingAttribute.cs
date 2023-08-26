namespace Server.Core.Net.Tcp.Handler
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MsgMapping : Attribute
    {
        public Type MsgType { get; }

        public MsgMapping(Type msgType)
        {
            MsgType = msgType;
        }
    }
}