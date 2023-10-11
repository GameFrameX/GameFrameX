using System;
using System.Threading.Tasks;
using GameFramework.Network;
using UnityEngine;
using UnityGameFramework.Network.Pipelines.Protocols;

namespace UnityGameFramework.Network.Pipelines
{
    public class NetChannel
    {
        public const string SESSIONID = "SESSIONID";
        public ConnectionContext Context { get; protected set; }
        public ProtocolReader Reader { get; protected set; }
        protected ProtocolWriter Writer { get; set; }
        public IProtoCalWriteHelper<MessageObject> ProtocolWriteHelper { get; protected set; }
        public IProtoCalReadHelper<MessageObject> ProtocolReadHelper { get; protected set; }
        Action<MessageObject> onMessageAct;
        Action<NetChannel> onConnectCloseAct;
        bool triggerCloseEvt = true;

        public NetChannel(ConnectionContext context, IProtoCalWriteHelper<MessageObject> protoCalWriteHelper,IProtoCalReadHelper<MessageObject> protoCalReadHelper, Action<MessageObject> onMessage = null, Action<NetChannel> onConnectClose = null)
        {
            Context = context;
            Reader = context.CreateReader();
            Writer = context.CreateWriter();
            ProtocolWriteHelper = protoCalWriteHelper;
            ProtocolReadHelper = protoCalReadHelper;
            onMessageAct = onMessage;
            onConnectCloseAct = onConnectClose;
            Context.ConnectionClosed.Register(ConnectionClosed);
        }

        public async Task StartReadMsgAsync()
        {
            while (Reader != null && Writer != null)
            {
                try
                {
                    var result = await Reader.ReadAsync(ProtocolReadHelper);

                    var message = result.Message;

                    onMessageAct(message);

                    if (result.IsCompleted)
                        break;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    break;
                }
            }
        }

        public void Write(MessageObject msg)
        {
            _ = Writer?.WriteAsync(ProtocolWriteHelper, msg);
        }


        public bool IsClose()
        {
            return Reader == null || Writer == null;
        }

        protected void ConnectionClosed()
        {
            if (triggerCloseEvt)
            {
                onConnectCloseAct(this);
            }

            Reader = null;
            Writer = null;
        }

        public void Abort(bool triggerCloseEvt)
        {
            this.triggerCloseEvt = triggerCloseEvt;
            Reader = null;
            Writer = null;
            try
            {
                Context.Abort();
            }
            catch (Exception)
            {
            }
        }

        public void RemoveSessionId()
        {
            Context.Items.Remove(SESSIONID);
        }


        public void SetSessionId(long id)
        {
            Context.Items[SESSIONID] = id;
        }

        public long GetSessionId()
        {
            if (Context.Items.TryGetValue(SESSIONID, out object idObj))
            {
                return (long) idObj;
            }

            return 0;
        }
    }
}