using Microsoft.AspNetCore.Connections;
using Server.Core.Net.Bedrock.Protocols;
using Server.Core.Net.Messages;

namespace Server.Core.Net.Tcp.Codecs
{
    public class NetChannel
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public const string SESSIONID = "SESSIONID";
        public ConnectionContext Context { get; protected set; }
        public ProtocolReader Reader { get; protected set; }
        protected ProtocolWriter Writer { get; set; }

        public IProtocal<NetMessage> Protocol { get; protected set; }

        public NetChannel(ConnectionContext context, IProtocal<NetMessage> protocal)
        {
            Context = context;
            Reader = context.CreateReader();
            Writer = context.CreateWriter();
            Protocol = protocal;
            Context.ConnectionClosed.Register(ConnectionClosed);
        }

        protected virtual void ConnectionClosed()
        {
            Reader = null;
            Writer = null;
        }

        public void RemoveSessionId()
        {
            Context.Items.Remove(SESSIONID);
        }

        public bool IsClose()
        {
            return Reader == null || Writer == null;
        }

        public void SetSessionId(long id)
        {
            Context.Items[SESSIONID] = id;
        }

        public long GetSessionId()
        {
            if (Context.Items.TryGetValue(SESSIONID, out var idObj))
                return (long) idObj;
            return 0;
        }

        public void Abort()
        {
            Context.Abort();
            Reader = null;
            Writer = null;
        }

        public async ValueTask WriteAsync(NetMessage msg)
        {
            if (Writer != null)
            {
                await Writer.WriteAsync(Protocol, msg);
            }
        }

        public void WriteAsync(MessageObject msg)
        {
            _ = WriteAsync(new NetMessage(msg));
        }
    }
}