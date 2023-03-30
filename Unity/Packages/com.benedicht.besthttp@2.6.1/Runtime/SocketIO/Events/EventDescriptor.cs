#if !BESTHTTP_DISABLE_SOCKETIO

using System;
using System.Collections.Generic;

namespace BestHTTP.SocketIO.Events
{
    public delegate void SocketIOCallback(Socket socket, Packet packet, params object[] args);
    public delegate void SocketIOAckCallback(Socket socket, Packet packet, params object[] args);

    /// <summary>
    /// A class to describe an event, and its metadatas.
    /// </summary>
    internal sealed class EventDescriptor
    {
        #region Public Properties

        /// <summary>
        /// List of callback delegates.
        /// </summary>
        public List<SocketIOCallback> Callbacks { get; private set; }

        /// <summary>
        /// If this property is true, callbacks are removed automatically after the event dispatch.
        /// </summary>
        public bool OnlyOnce { get; private set; }

        /// <summary>
        /// If this property is true, the dispatching packet's Payload will be decoded using the Manager's Encoder.
        /// </summary>
        public bool AutoDecodePayload { get; private set; }

        #endregion

        /// <summary>
        /// Cache an array on a hot-path.
        /// </summary>
        private SocketIOCallback[] CallbackArray;

        /// <summary>
        /// Constructor to create an EventDescriptor instance and set the meta-datas.
        /// </summary>
        public EventDescriptor(bool onlyOnce, bool autoDecodePayload, SocketIOCallback callback)
        {
            this.OnlyOnce = onlyOnce;
            this.AutoDecodePayload = autoDecodePayload;
            this.Callbacks = new List<SocketIOCallback>(1);

            if (callback != null)
                Callbacks.Add(callback);
        }

        /// <summary>
        /// Will call the callback delegates with the given parameters and remove the callbacks if this descriptor marked with a true OnlyOnce property.
        /// </summary>
        public void Call(Socket socket, Packet packet, params object[] args)
        {
            int callbackCount = Callbacks.Count;
            if (CallbackArray == null || CallbackArray.Length < callbackCount)
                Array.Resize(ref CallbackArray, callbackCount);

            // Copy the callback delegates to an array, because in one of the callbacks we can modify the list(by calling On/Once/Off in an event handler)
            // This way we can prevent some strange bug
            Callbacks.CopyTo(CallbackArray);

            // Go through the delegates and call them
            for (int i = 0; i < callbackCount; ++i)
            {
                try
                {
                    // Call the delegate.
                    SocketIOCallback callback = CallbackArray[i];
                    if (callback!= null)
                        callback(socket, packet, args);
                }
                catch (Exception ex)
                {
                    // Do not try to emit a new Error when we already tried to deliver an Error, possible causing a
                    //  stack overflow
                    if (args == null || args.Length == 0 || !(args[0] is Error))
                        (socket as ISocket).EmitError(SocketIOErrors.User, ex.Message + " " + ex.StackTrace);

                    HTTPManager.Logger.Exception("EventDescriptor", "Call", ex);
                }

                // If these callbacks has to be called only once, remove them from the main list
                if (this.OnlyOnce)
                    Callbacks.Remove(CallbackArray[i]);

                // Don't keep any reference avoiding memory leaks
                CallbackArray[i] = null;
            }
        }
    }
}

#endif