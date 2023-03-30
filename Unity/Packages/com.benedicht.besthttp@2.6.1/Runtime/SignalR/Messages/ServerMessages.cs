#if !BESTHTTP_DISABLE_SIGNALR

#if BESTHTTP_SIGNALR_WITH_JSONDOTNET
using Newtonsoft.Json.Linq;
#endif
using System;
using System.Collections;
using System.Collections.Generic;

namespace BestHTTP.SignalR.Messages
{
    /// <summary>
    /// Keep-alive message sent by the server. No data sent with it.
    /// </summary>
    public sealed class KeepAliveMessage : IServerMessage
    {
        MessageTypes IServerMessage.Type { get { return MessageTypes.KeepAlive; } }
        void IServerMessage.Parse(object data) { }
    }

    /// <summary>
    /// A message that may contains multiple sub-messages and additional informations.
    /// </summary>
    public sealed class MultiMessage : IServerMessage
    {
        MessageTypes IServerMessage.Type { get { return MessageTypes.Multiple; } }

        /// <summary>
        /// Id of the sent message
        /// </summary>
        public string MessageId { get; private set; }

        /// <summary>
        /// True if it's an initialization message, false otherwise.
        /// </summary>
        public bool IsInitialization { get; private set; }

        /// <summary>
        /// Group token may be sent, if the group changed that the client belongs to.
        /// </summary>
        public string GroupsToken { get; private set; }

        /// <summary>
        /// The server suggests that the client should do a reconnect turn.
        /// </summary>
        public bool ShouldReconnect { get; private set; }

        /// <summary>
        /// Additional poll delay sent by the server.
        /// </summary>
        public TimeSpan? PollDelay { get; private set; }

        /// <summary>
        /// List of server messages sent inside this message.
        /// </summary>
        public List<IServerMessage> Data { get; private set; }

        void IServerMessage.Parse(object data)
        {
            IDictionary<string, object> dic = data as IDictionary<string, object>;
            object value;

            this.MessageId = dic["C"].ToString();

            if (dic.TryGetValue("S", out value))
                IsInitialization = int.Parse(value.ToString()) == 1 ? true : false;
            else
                IsInitialization = false;

            if (dic.TryGetValue("G", out value))
                GroupsToken = value.ToString();

            if (dic.TryGetValue("T", out value))
                ShouldReconnect = int.Parse(value.ToString()) == 1 ? true : false;
            else
                ShouldReconnect = false;

            if (dic.TryGetValue("L", out value))
                PollDelay = TimeSpan.FromMilliseconds(double.Parse(value.ToString()));

            IEnumerable enumerable = dic["M"] as IEnumerable;

            if (enumerable != null)
            {
                Data = new List<IServerMessage>();

                foreach (object subData in enumerable)
                {
#if BESTHTTP_SIGNALR_WITH_JSONDOTNET
                    IDictionary<string, JToken> subObj = subData as IDictionary<string, JToken>;
#else
                    IDictionary<string, object> subObj = subData as IDictionary<string, object>;
#endif

                    IServerMessage subMsg = null;

                    if (subObj != null)
                    {
                        if (subObj.ContainsKey("H"))
                            subMsg = new MethodCallMessage();
                        else if (subObj.ContainsKey("I"))
                            subMsg = new ProgressMessage();
                        else
                            subMsg = new DataMessage();
                    }
                    else
                        subMsg = new DataMessage();

                    subMsg.Parse(subData);

                    Data.Add(subMsg);
                }
            }
        }
    }

    /// <summary>
    /// A simple non-hub data message. It holds only one Data property.
    /// </summary>
    public sealed class DataMessage : IServerMessage
    {
        MessageTypes IServerMessage.Type { get { return MessageTypes.Data; } }

        public object Data { get; private set; }

        void IServerMessage.Parse(object data)
        {
            this.Data = data;
        }
    }

    /// <summary>
    /// A Hub message that orders the client to call a method.
    /// </summary>
    public sealed class MethodCallMessage : IServerMessage
    {
        MessageTypes IServerMessage.Type { get { return MessageTypes.MethodCall; } }

        /// <summary>
        /// The name of the Hub that the method is called on.
        /// </summary>
        public string Hub { get; private set; }

        /// <summary>
        /// Name of the Method.
        /// </summary>
        public string Method { get; private set; }

        /// <summary>
        /// Arguments of the method call.
        /// </summary>
        public object[] Arguments { get; private set; }

        /// <summary>
        /// State changes of the hub. It's handled automatically by the Hub.
        /// </summary>
#if BESTHTTP_SIGNALR_WITH_JSONDOTNET
        public IDictionary<string, JToken> State { get; private set; }
#else
        public IDictionary<string, object> State { get; private set; }
#endif

        void IServerMessage.Parse(object data)
        {
            #if BESTHTTP_SIGNALR_WITH_JSONDOTNET
                IDictionary<string, JToken> dic = data as IDictionary<string, JToken>;
            #else
                IDictionary<string, object> dic = data as IDictionary<string, object>;
            #endif

            Hub = dic["H"].ToString();
            Method = dic["M"].ToString();

            List<object> args = new List<object>();
            foreach (object arg in dic["A"] as IEnumerable)
                args.Add(arg);
            Arguments = args.ToArray();

#if BESTHTTP_SIGNALR_WITH_JSONDOTNET
            JToken value;
            if (dic.TryGetValue("S", out value))
                State = value as IDictionary<string, JToken>;
#else
            object value;
            if (dic.TryGetValue("S", out value))
                State = value as IDictionary<string, object>;
#endif
        }
    }

    /// <summary>
    /// Message of a server side method invocation result.
    /// </summary>
    public sealed class ResultMessage : IServerMessage, IHubMessage
    {
        MessageTypes IServerMessage.Type { get { return MessageTypes.Result; } }

        /// <summary>
        /// The unique id that the client set when called the server side method. Used by the plugin to deliver this message to the good Hub.
        /// </summary>
        public UInt64 InvocationId { get; private set; }

        /// <summary>
        /// The return value of the server side method call, or null if the method's return type is void.
        /// </summary>
        public object ReturnValue { get; private set; }

        /// <summary>
        /// State changes of the hub. It's handled automatically by the Hub.
        /// </summary>
#if BESTHTTP_SIGNALR_WITH_JSONDOTNET
        public IDictionary<string, JToken> State { get; private set; }
#else
        public IDictionary<string, object> State { get; private set; }
#endif

        void IServerMessage.Parse(object data)
        {
            IDictionary<string, object> dic = data as IDictionary<string, object>;

            InvocationId = UInt64.Parse(dic["I"].ToString());

            object value;
            if (dic.TryGetValue("R", out value))
                ReturnValue = value;

            if (dic.TryGetValue("S", out value))
            {
#if BESTHTTP_SIGNALR_WITH_JSONDOTNET
                    State = value as IDictionary<string, JToken>;
#else
                    State = value as IDictionary<string, object>;
#endif
            }
        }
    }

    public sealed class FailureMessage : IServerMessage, IHubMessage
    {
        MessageTypes IServerMessage.Type { get { return MessageTypes.Failure; } }

        /// <summary>
        /// The unique id that the client set when called the server side method. Used by the plugin to deliver this message to the good Hub.
        /// </summary>
        public UInt64 InvocationId { get; private set; }

        /// <summary>
        /// True if it's a hub error.
        /// </summary>
        public bool IsHubError { get; private set; }

        /// <summary>
        /// If the method call failed, it contains the error message to detail what happened.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// A dictionary that may contain additional error data (can only be present for hub errors). It can be null.
        /// </summary>
#if BESTHTTP_SIGNALR_WITH_JSONDOTNET
        public IDictionary<string, JToken> AdditionalData { get; private set; }
#else
        public IDictionary<string, object> AdditionalData { get; private set; }
#endif

        /// <summary>
        /// Stack trace of the error. It present only if detailed error reporting is turned on on the server (https://msdn.microsoft.com/en-us/library/microsoft.aspnet.signalr.hubconfiguration.enabledetailederrors%28v=vs.118%29.aspx).
        /// </summary>
        public string StackTrace { get; private set; }

        /// <summary>
        /// State changes of the hub. It's handled automatically by the Hub.
        /// </summary>
#if BESTHTTP_SIGNALR_WITH_JSONDOTNET
        public IDictionary<string, JToken> State { get; private set; }
#else
        public IDictionary<string, object> State { get; private set; }
#endif

        void IServerMessage.Parse(object data)
        {
            IDictionary<string, object> dic = data as IDictionary<string, object>;

            InvocationId = UInt64.Parse(dic["I"].ToString());

            object value;

            if (dic.TryGetValue("E", out value))
                ErrorMessage = value.ToString();

            if (dic.TryGetValue("H", out value))
                IsHubError = int.Parse(value.ToString()) == 1 ? true : false;

            if (dic.TryGetValue("D", out value))
            {
#if BESTHTTP_SIGNALR_WITH_JSONDOTNET
                    AdditionalData = value as IDictionary<string, JToken>;
#else
                    AdditionalData = value as IDictionary<string, object>;
#endif
            }

            if (dic.TryGetValue("T", out value))
                StackTrace = value.ToString();

            if (dic.TryGetValue("S", out value))
            {
#if BESTHTTP_SIGNALR_WITH_JSONDOTNET
                    State = value as IDictionary<string, JToken>;
#else
                    State = value as IDictionary<string, object>;
#endif
            }
        }
    }

    /// <summary>
    /// When a server method is a long running method the server can send the information about the progress of execution of the method to the client.
    /// </summary>
    public sealed class ProgressMessage : IServerMessage, IHubMessage
    {
        MessageTypes IServerMessage.Type { get { return MessageTypes.Progress; } }

        /// <summary>
        /// The unique id that the client set when called the server side method. Used by the plugin to deliver this message to the good Hub.
        /// </summary>
        public UInt64 InvocationId { get; private set; }

        /// <summary>
        /// Current progress of the long running method.
        /// </summary>
        public double Progress { get; private set; }

        void IServerMessage.Parse(object data)
        {
#if BESTHTTP_SIGNALR_WITH_JSONDOTNET
                IDictionary<string, JToken> dic = data as IDictionary<string, JToken>;
#else
                IDictionary<string, object> dic = data as IDictionary<string, object>;
#endif

#if BESTHTTP_SIGNALR_WITH_JSONDOTNET
                IDictionary<string, JToken> P = dic["P"] as IDictionary<string, JToken>;
#else
                IDictionary<string, object> P = dic["P"] as IDictionary<string, object>;
#endif

            InvocationId = UInt64.Parse(P["I"].ToString());
            Progress = double.Parse(P["D"].ToString());
        }
    }
}

#endif