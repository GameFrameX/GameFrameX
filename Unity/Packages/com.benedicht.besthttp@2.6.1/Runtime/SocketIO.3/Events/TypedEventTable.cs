#if !BESTHTTP_DISABLE_SOCKETIO
using System;
using System.Collections.Generic;

namespace BestHTTP.SocketIO3.Events
{
    [PlatformSupport.IL2CPP.Preserve]
    public sealed class ConnectResponse
    {
        [PlatformSupport.IL2CPP.Preserve] public string sid;
    }

    public struct CallbackDescriptor
    {
        public readonly Type[] ParamTypes;
        public readonly Action<object[]> Callback;
        public readonly bool Once;

        public CallbackDescriptor(Type[] paramTypes, Action<object[]> callback, bool once)
        {
            this.ParamTypes = paramTypes;
            this.Callback = callback;
            this.Once = once;
        }
    }

    public sealed class Subscription
    {
        public List<CallbackDescriptor> callbacks = new List<CallbackDescriptor>(1);

        public void Add(Type[] paramTypes, Action<object[]> callback, bool once)
        {
            this.callbacks.Add(new CallbackDescriptor(paramTypes, callback, once));
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

    public sealed class TypedEventTable
    {
        /// <summary>
        /// The Socket that this EventTable is bound to.
        /// </summary>
        private Socket Socket { get; set; }

        /// <summary>
        /// This is where we store the methodname => callback mapping.
        /// </summary>
        private Dictionary<string, Subscription> subscriptions = new Dictionary<string, Subscription>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Constructor to create an instance and bind it to a socket.
        /// </summary>
        public TypedEventTable(Socket socket)
        {
            this.Socket = socket;
        }

        public Subscription GetSubscription(string name)
        {
            Subscription subscription = null;
            this.subscriptions.TryGetValue(name, out subscription);
            return subscription;
        }

        public void Register(string methodName, Type[] paramTypes, Action<object[]> callback, bool once = false)
        {
            Subscription subscription = null;
            if (!this.subscriptions.TryGetValue(methodName, out subscription))
                this.subscriptions.Add(methodName, subscription = new Subscription());

            subscription.Add(paramTypes, callback, once);
        }

        public void Call(string eventName, object[] args)
        {
            Subscription subscription = null;
            if (this.subscriptions.TryGetValue(eventName, out subscription))
            {
                for (int i = 0; i < subscription.callbacks.Count; ++i)
                {
                    var callbackDesc = subscription.callbacks[i];

                    try
                    {
                        callbackDesc.Callback.Invoke(args);                        
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception("TypedEventTable", String.Format("Call('{0}', {1}) - Callback.Invoke", eventName, args != null ? args.Length : 0), ex, this.Socket.Context);
                    }

                    if (callbackDesc.Once)
                        subscription.callbacks.RemoveAt(i--);
                }
            }
        }

        public void Call(IncomingPacket packet)
        {
            if (packet.Equals(IncomingPacket.Empty))
                return;

            string name = packet.EventName;
            object[] args = packet.DecodedArg != null ? new object[] { packet.DecodedArg } : packet.DecodedArgs;

            Call(name, args);
        }

        public void Unregister(string name)
        {
            this.subscriptions.Remove(name);
        }

        public void Clear()
        {
            this.subscriptions.Clear();
        }
    }
}
#endif
