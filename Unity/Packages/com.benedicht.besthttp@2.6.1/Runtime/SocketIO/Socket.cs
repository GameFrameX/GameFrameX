#if !BESTHTTP_DISABLE_SOCKETIO

using System;
using System.Collections.Generic;

namespace BestHTTP.SocketIO
{
    using BestHTTP;
    using BestHTTP.SocketIO.Events;

    /// <summary>
    /// This class represents a Socket.IO namespace.
    /// </summary>
    public sealed class Socket : ISocket
    {
        #region Public Properties

        /// <summary>
        /// The SocketManager instance that created this socket.
        /// </summary>
        public SocketManager Manager { get; private set; }

        /// <summary>
        /// The namespace that this socket is bound to.
        /// </summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// Unique Id of the socket.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// True if the socket is connected and open to the server. False otherwise.
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// While this property is True, the socket will decode the Packet's Payload data using the parent SocketManager's Encoder. You must set this property before any event subscription! Its default value is True;
        /// </summary>
        public bool AutoDecodePayload { get; set; }

        #endregion

        #region Privates

        /// <summary>
        /// A table to store acknowledgment callbacks associated to the given ids.
        /// </summary>
        private Dictionary<int, SocketIOAckCallback> AckCallbacks;

        /// <summary>
        /// Tha callback table that helps this class to manage event subscription and dispatching events.
        /// </summary>
        private EventTable EventCallbacks;

        /// <summary>
        /// Cached list to spare some GC alloc.
        /// </summary>
        private List<object> arguments = new List<object>();

        #endregion

        /// <summary>
        /// Internal constructor.
        /// </summary>
        internal Socket(string nsp, SocketManager manager)
        {
            this.Namespace = nsp;
            this.Manager = manager;
            this.IsOpen = false;
            this.AutoDecodePayload = true;
            this.EventCallbacks = new EventTable(this);
        }

        #region Socket Handling

        /// <summary>
        /// Internal function to start opening the socket.
        /// </summary>
        void ISocket.Open()
        {
            HTTPManager.Logger.Information("Socket", string.Format("Open - Manager.State = {0}", Manager.State));

            // The transport already established the connection
            if (Manager.State == SocketManager.States.Open)
                OnTransportOpen();
            else if (Manager.Options.AutoConnect && Manager.State == SocketManager.States.Initial)
                    Manager.Open();
        }

        /// <summary>
        /// Disconnects this socket/namespace.
        /// </summary>
        public void Disconnect()
        {
            (this as ISocket).Disconnect(true);
        }

        /// <summary>
        /// Disconnects this socket/namespace.
        /// </summary>
        void ISocket.Disconnect(bool remove)
        {
            // Send a disconnect packet to the server
            if (IsOpen)
            {
                Packet packet = new Packet(TransportEventTypes.Message, SocketIOEventTypes.Disconnect, this.Namespace, string.Empty);
                (Manager as IManager).SendPacket(packet);

                // IsOpen must be false, because in the OnPacket preprocessing the packet would call this function again
                IsOpen = false;
                (this as ISocket).OnPacket(packet);
            }

            if (AckCallbacks != null)
                AckCallbacks.Clear();

            if (remove)
            {
                EventCallbacks.Clear();

                (Manager as IManager).Remove(this);
            }
        }

        #endregion

        #region Emit Implementations

        public Socket Emit(string eventName, params object[] args)
        {
            return Emit(eventName, null, args);
        }

        public Socket Emit(string eventName, SocketIOAckCallback callback, params object[] args)
        {
            bool blackListed = EventNames.IsBlacklisted(eventName);
            if (blackListed)
                throw new ArgumentException("Blacklisted event: " + eventName);

            arguments.Clear();
            arguments.Add(eventName);

            // Find and swap any binary data(byte[]) to a placeholder string.
            // Server side these will be swapped back.
            List<byte[]> attachments = null;
            if (args != null && args.Length > 0)
            {
                int idx = 0;
                for (int i = 0; i < args.Length; ++i)
                {
                    byte[] binData = args[i] as byte[];
                    if (binData != null)
                    {
                        if (attachments == null)
                            attachments = new List<byte[]>();

                        Dictionary<string, object> placeholderObj = new Dictionary<string, object>(2);
                        placeholderObj.Add(Packet.Placeholder, true);
                        placeholderObj.Add("num", idx++);

                        arguments.Add(placeholderObj);

                        attachments.Add(binData);
                    }
                    else
                        arguments.Add(args[i]);
                }
            }

            string payload = null;

            try
            {
                payload = Manager.Encoder.Encode(arguments);
            }
            catch(Exception ex)
            {
                (this as ISocket).EmitError(SocketIOErrors.Internal, "Error while encoding payload: " + ex.Message + " " + ex.StackTrace);

                return this;
            }

            // We don't use it further in this function, so we can clear it to not hold any unwanted reference.
            arguments.Clear();

            if (payload == null)
                throw new ArgumentException("Encoding the arguments to JSON failed!");

            int id = 0;

            if (callback != null)
            {
                id = Manager.NextAckId;

                if (AckCallbacks == null)
                    AckCallbacks = new Dictionary<int, SocketIOAckCallback>();

                AckCallbacks[id] = callback;
            }

            Packet packet = new Packet(TransportEventTypes.Message,
                                       attachments == null ? SocketIOEventTypes.Event : SocketIOEventTypes.BinaryEvent,
                                       this.Namespace,
                                       payload,
                                       0,
                                       id);

            if (attachments != null)
                packet.Attachments = attachments; // This will set the AttachmentCount property too.

            (Manager as IManager).SendPacket(packet);

            return this;
        }

        public Socket EmitAck(Packet originalPacket, params object[] args)
        {
            if (originalPacket == null)
                throw new ArgumentNullException("originalPacket == null!");

            if (/*originalPacket.Id == 0 ||*/
                (originalPacket.SocketIOEvent != SocketIOEventTypes.Event && originalPacket.SocketIOEvent != SocketIOEventTypes.BinaryEvent))
                throw new ArgumentException("Wrong packet - you can't send an Ack for a packet with id == 0 and SocketIOEvent != Event or SocketIOEvent != BinaryEvent!");

            arguments.Clear();
            if (args != null && args.Length > 0)
                arguments.AddRange(args);

            string payload = null;
            try
            {
                payload = Manager.Encoder.Encode(arguments);
            }
            catch (Exception ex)
            {
                (this as ISocket).EmitError(SocketIOErrors.Internal, "Error while encoding payload: " + ex.Message + " " + ex.StackTrace);

                return this;
            }

            if (payload == null)
                throw new ArgumentException("Encoding the arguments to JSON failed!");

            Packet packet = new Packet(TransportEventTypes.Message,
                                       originalPacket.SocketIOEvent == SocketIOEventTypes.Event ? SocketIOEventTypes.Ack : SocketIOEventTypes.BinaryAck,
                                       this.Namespace,
                                       payload,
                                       0,
                                       originalPacket.Id);

            (Manager as IManager).SendPacket(packet);

            return this;
        }

        #endregion

        #region On Implementations

        /// <summary>
        /// Register a callback for a given name
        /// </summary>
        public void On(string eventName, SocketIOCallback callback)
        {
            EventCallbacks.Register(eventName, callback, false, this.AutoDecodePayload);
        }

        public void On(SocketIOEventTypes type, SocketIOCallback callback)
        {
            string eventName = EventNames.GetNameFor(type);

            EventCallbacks.Register(eventName, callback, false, this.AutoDecodePayload);
        }

        public void On(string eventName, SocketIOCallback callback, bool autoDecodePayload)
        {
            EventCallbacks.Register(eventName, callback, false, autoDecodePayload);
        }

        public void On(SocketIOEventTypes type, SocketIOCallback callback, bool autoDecodePayload)
        {
            string eventName = EventNames.GetNameFor(type);

            EventCallbacks.Register(eventName, callback, false, autoDecodePayload);
        }

        #endregion

        #region Once Implementations

        public void Once(string eventName, SocketIOCallback callback)
        {
            EventCallbacks.Register(eventName, callback, true, this.AutoDecodePayload);
        }

        public void Once(SocketIOEventTypes type, SocketIOCallback callback)
        {
            EventCallbacks.Register(EventNames.GetNameFor(type), callback, true, this.AutoDecodePayload);
        }

        public void Once(string eventName, SocketIOCallback callback, bool autoDecodePayload)
        {
            EventCallbacks.Register(eventName, callback, true, autoDecodePayload);
        }

        public void Once(SocketIOEventTypes type, SocketIOCallback callback, bool autoDecodePayload)
        {
            EventCallbacks.Register(EventNames.GetNameFor(type), callback, true, autoDecodePayload);
        }

        #endregion

        #region Off Implementations

        /// <summary>
        /// Remove all callbacks for all events.
        /// </summary>
        public void Off()
        {
            EventCallbacks.Clear();
        }

        /// <summary>
        /// Removes all callbacks to the given event.
        /// </summary>
        public void Off(string eventName)
        {
            EventCallbacks.Unregister(eventName);
        }

        /// <summary>
        /// Removes all callbacks to the given event.
        /// </summary>
        public void Off(SocketIOEventTypes type)
        {
            Off(EventNames.GetNameFor(type));
        }

        /// <summary>
        /// Remove the specified callback.
        /// </summary>
        public void Off(string eventName, SocketIOCallback callback)
        {
            EventCallbacks.Unregister(eventName, callback);
        }

        /// <summary>
        /// Remove the specified callback.
        /// </summary>
        public void Off(SocketIOEventTypes type, SocketIOCallback callback)
        {
            EventCallbacks.Unregister(EventNames.GetNameFor(type), callback);
        }

        #endregion

        #region Packet Handling

        /// <summary>
        /// Last call of the OnPacket chain(Transport -> Manager -> Socket), we will dispatch the event if there is any callback
        /// </summary>
        void ISocket.OnPacket(Packet packet)
        {
            // Some preprocessing of the packet
            switch(packet.SocketIOEvent)
            {
                case SocketIOEventTypes.Connect:
                    if (this.Manager.Options.ServerVersion != SupportedSocketIOVersions.v3)
                    {
                        this.Id = this.Namespace != "/" ? this.Namespace + "#" + this.Manager.Handshake.Sid : this.Manager.Handshake.Sid;
                    }
                    else
                    {
                        var data = JSON.Json.Decode(packet.Payload) as Dictionary<string, object>;
                        this.Id = data["sid"].ToString();
                    }
                    this.IsOpen = true;
                    break;

                case SocketIOEventTypes.Disconnect:
                    if (IsOpen)
                    {
                        IsOpen = false;
                        EventCallbacks.Call(EventNames.GetNameFor(SocketIOEventTypes.Disconnect), packet);
                        Disconnect();
                    }
                    break;

                // Create an Error object from the server-sent json string
                case SocketIOEventTypes.Error:
                    bool success = false;
                    object result = JSON.Json.Decode(packet.Payload, ref success);
                    if (success)
                    {
                        var errDict = result as Dictionary<string, object>;
                        Error err = null;

                        if (errDict != null)
                        {
                            object tmpObject = null;
                            string code = null;
                            if (errDict.TryGetValue("code", out tmpObject))
                                code = tmpObject.ToString();

                            int errorCode;
                            if (code != null && int.TryParse(code, out errorCode) && errorCode >= 0 && errorCode <= 7)
                            {
                                errDict.TryGetValue("message", out tmpObject);
                                err = new Error((SocketIOErrors)errorCode, tmpObject != null ? tmpObject.ToString() : string.Empty);
                            }
                        }

                        if (err == null)
                            err = new Error(SocketIOErrors.Custom, packet.Payload);

                        EventCallbacks.Call(EventNames.GetNameFor(SocketIOEventTypes.Error), packet, err);

                        return;
                    }
                    break;
            }

            // Dispatch the event to all subscriber
            EventCallbacks.Call(packet);

            // call Ack callbacks
            if ((packet.SocketIOEvent == SocketIOEventTypes.Ack || packet.SocketIOEvent == SocketIOEventTypes.BinaryAck) && AckCallbacks != null)
            {
                SocketIOAckCallback ackCallback = null;
                if (AckCallbacks.TryGetValue(packet.Id, out ackCallback) &&
                    ackCallback != null)
                {
                    try
                    {
                        ackCallback(this, packet, this.AutoDecodePayload ? packet.Decode(Manager.Encoder) : null);
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception("Socket", "ackCallback", ex);
                    }
                }

                AckCallbacks.Remove(packet.Id);
            }
        }

        #endregion

        /// <summary>
        /// Emits an internal packet-less event to the user level.
        /// </summary>
        void ISocket.EmitEvent(SocketIOEventTypes type, params object[] args)
        {
            (this as ISocket).EmitEvent(EventNames.GetNameFor(type), args);
        }

        /// <summary>
        /// Emits an internal packet-less event to the user level.
        /// </summary>
        void ISocket.EmitEvent(string eventName, params object[] args)
        {
            if (!string.IsNullOrEmpty(eventName))
                EventCallbacks.Call(eventName, null, args);
        }

        void ISocket.EmitError(SocketIOErrors errCode, string msg)
        {
            (this as ISocket).EmitEvent(SocketIOEventTypes.Error, new Error(errCode, msg));
        }

        #region Private Helper Functions

        /// <summary>
        /// Called when the underlying transport is connected
        /// </summary>
        internal void OnTransportOpen()
        {
            HTTPManager.Logger.Information("Socket", "OnTransportOpen - IsOpen: " + this.IsOpen);

            if (this.IsOpen)
                return;

            if (this.Namespace != "/" || this.Manager.Options.ServerVersion == SupportedSocketIOVersions.v3)
            {
                try
                {
                    string authData = null;
                    if (this.Manager.Options.ServerVersion == SupportedSocketIOVersions.v3)
                        authData = this.Manager.Options.Auth != null ? this.Manager.Options.Auth(this.Manager, this) : "{}";

                    (Manager as IManager).SendPacket(new Packet(TransportEventTypes.Message, SocketIOEventTypes.Connect, this.Namespace, authData));
                }
                catch (Exception ex)
                {
                    HTTPManager.Logger.Exception("Socket", "OnTransportOpen", ex);
                }
            }
            else
                this.IsOpen = true;
        }

        #endregion
    }
}

#endif
