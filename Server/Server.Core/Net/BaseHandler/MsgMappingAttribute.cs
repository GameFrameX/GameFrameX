namespace Server.Core.Net.BaseHandler
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