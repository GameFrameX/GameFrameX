#if !BESTHTTP_DISABLE_SIGNALR

using System;

namespace BestHTTP.SignalR.Messages
{
    public interface IServerMessage
    {
        MessageTypes Type { get; }
        void Parse(object data);
    }

    public interface IHubMessage
    {
        UInt64 InvocationId { get; }
    }
}

#endif