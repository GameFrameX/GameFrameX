#if !BESTHTTP_DISABLE_SIGNALR_CORE
using System;
using System.Collections.Generic;
using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP.SignalRCore
{
    public enum TransportTypes
    {
#if !BESTHTTP_DISABLE_WEBSOCKET
        WebSocket,
#endif
        LongPolling
    }

    public enum TransferModes
    {
        Binary,
        Text
    }

    public enum TransportStates
    {
        Initial,
        Connecting,
        Connected,
        Closing,
        Failed,
        Closed
    }

    /// <summary>
    /// Possible states of a HubConnection
    /// </summary>
    public enum ConnectionStates
    {
        Initial,
        Authenticating,
        Negotiating,
        Redirected,
        Reconnecting,
        Connected,
        CloseInitiated,
        Closed
    }

    /// <summary>
    /// States that a transport can goes trough as seen from 'outside'.
    /// </summary>
    public enum TransportEvents
    {
        /// <summary>
        /// Transport is selected to try to connect to the server
        /// </summary>
        SelectedToConnect,

        /// <summary>
        /// Transport failed to connect to the server. This event can occur after SelectedToConnect, when already connected and an error occurs it will be a ClosedWithError one.
        /// </summary>
        FailedToConnect,

        /// <summary>
        /// The transport successfully connected to the server.
        /// </summary>
        Connected,

        /// <summary>
        /// Transport gracefully terminated.
        /// </summary>
        Closed,

        /// <summary>
        /// Unexpected error occured and the transport can't recover from it.
        /// </summary>
        ClosedWithError
    }

    public interface ITransport
    {
        TransferModes TransferMode { get; }
        TransportTypes TransportType { get; }
        TransportStates State { get; }

        string ErrorReason { get; }

        event Action<TransportStates, TransportStates> OnStateChanged;

        void StartConnect();
        void StartClose();

        void Send(BufferSegment bufferSegment);
    }

    public interface IEncoder
    {
        BufferSegment Encode<T>(T value);

        T DecodeAs<T>(BufferSegment buffer);

        object ConvertTo(Type toType, object obj);
    }

    public sealed class StreamItemContainer<T>
    {
        public readonly long id;

        public List<T> Items { get; private set; }
        public T LastAdded { get; private set; }

        public bool IsCanceled;

        public StreamItemContainer(long _id)
        {
            this.id = _id;
            this.Items = new List<T>();
        }

        public void AddItem(T item)
        {
            if (this.Items == null)
                this.Items = new List<T>();

            this.Items.Add(item);
            this.LastAdded = item;
        }
    }

    struct CallbackDescriptor
    {
        public readonly Type[] ParamTypes;
        public readonly Action<object[]> Callback;
        public CallbackDescriptor(Type[] paramTypes, Action<object[]> callback)
        {
            this.ParamTypes = paramTypes;
            this.Callback = callback;
        }
    }

    internal struct InvocationDefinition
    {
        public Action<Messages.Message> callback;
        public Type returnType;
    }

    internal sealed class Subscription
    {
        public List<CallbackDescriptor> callbacks = new List<CallbackDescriptor>(1);

        public void Add(Type[] paramTypes, Action<object[]> callback)
        {
            this.callbacks.Add(new CallbackDescriptor(paramTypes, callback));
        }

        public void Remove(Action<object[]> callback)
        {
            int idx = -1;
            for (int i = 0; i < this.callbacks.Count && idx == -1; ++i)
                if (this.callbacks[i].Callback == callback)
                    idx = i;

            if (idx != -1)
                this.callbacks.RemoveAt(idx);
        }
    }

    public sealed class HubOptions
    {
        /// <summary>
        /// When this is set to true, the plugin will skip the negotiation request if the PreferedTransport is WebSocket. Its default value is false.
        /// </summary>
        public bool SkipNegotiation { get; set; }

        /// <summary>
        /// The preferred transport to choose when more than one available. Its default value is TransportTypes.WebSocket.
        /// </summary>
        public TransportTypes PreferedTransport { get; set; }

        /// <summary>
        /// A ping message is only sent if the interval has elapsed without a message being sent. Its default value is 15 seconds.
        /// </summary>
        public TimeSpan PingInterval { get; set; }

        /// <summary>
        /// If the client doesn't see any message in this interval, considers the connection broken. Its default value is 30 seconds.
        /// </summary>
        public TimeSpan PingTimeoutInterval { get; set; }

        /// <summary>
        /// The maximum count of redirect negotiation result that the plugin will follow. Its default value is 100.
        /// </summary>
        public int MaxRedirects { get; set; }

        /// <summary>
        /// The maximum time that the plugin allowed to spend trying to connect. Its default value is 1 minute.
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; }

        public HubOptions()
        {
            this.SkipNegotiation = false;
#if !BESTHTTP_DISABLE_WEBSOCKET
            this.PreferedTransport = TransportTypes.WebSocket;
#else
            this.PreferedTransport = TransportTypes.LongPolling;
#endif
            this.PingInterval = TimeSpan.FromSeconds(15);
            this.PingTimeoutInterval = TimeSpan.FromSeconds(30);
            this.MaxRedirects = 100;
            this.ConnectTimeout = TimeSpan.FromSeconds(60);
        }
    }

    public interface IRetryPolicy
    {
        /// <summary>
        /// This function must return with a delay time to wait until a new connection attempt, or null to do not do another one.
        /// </summary>
        TimeSpan? GetNextRetryDelay(RetryContext context);
    }

    public struct RetryContext
    {
        /// <summary>
        /// Previous reconnect attempts. A successful connection sets it back to zero.
        /// </summary>
        public uint PreviousRetryCount;

        /// <summary>
        /// Elapsed time since the original connection error.
        /// </summary>
        public TimeSpan ElapsedTime;

        /// <summary>
        /// String representation of the connection error.
        /// </summary>
        public string RetryReason;
    }

    public sealed class DefaultRetryPolicy : IRetryPolicy
    {
        private static TimeSpan?[] DefaultBackoffTimes = new TimeSpan?[]
        {
            TimeSpan.Zero,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(30),
            null
        };

        TimeSpan?[] backoffTimes;

        public DefaultRetryPolicy()
        {
            this.backoffTimes = DefaultBackoffTimes;
        }

        public DefaultRetryPolicy(TimeSpan?[] customBackoffTimes)
        {
            this.backoffTimes = customBackoffTimes;
        }

        public TimeSpan? GetNextRetryDelay(RetryContext context)
        {
            if (context.PreviousRetryCount >= this.backoffTimes.Length)
                return null;

            return this.backoffTimes[context.PreviousRetryCount];
        }
    }
}
#endif
