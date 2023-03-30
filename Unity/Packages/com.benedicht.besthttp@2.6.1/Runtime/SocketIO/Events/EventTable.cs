#if !BESTHTTP_DISABLE_SOCKETIO

using System.Collections.Generic;

namespace BestHTTP.SocketIO.Events
{
    /// <summary>
    /// This class helps keep track and maintain EventDescriptor instances and dispatching packets to the right delegates.
    /// </summary>
    internal sealed class EventTable
    {
        #region Privates

        /// <summary>
        /// The Socket that this EventTable is bound to.
        /// </summary>
        private Socket Socket { get; set; }

        /// <summary>
        /// The 'EventName -> List of events' mapping.
        /// </summary>
        private Dictionary<string, List<EventDescriptor>> Table = new Dictionary<string, List<EventDescriptor>>();

        #endregion

        /// <summary>
        /// Constructor to create an instance and bind it to a socket.
        /// </summary>
        public EventTable(Socket socket)
        {
            this.Socket = socket;
        }

        /// <summary>
        /// Register a callback to a name with the given metadata.
        /// </summary>
        public void Register(string eventName, SocketIOCallback callback, bool onlyOnce, bool autoDecodePayload)
        {
            List<EventDescriptor> events;
            if (!Table.TryGetValue(eventName, out events))
                Table.Add(eventName, events = new List<EventDescriptor>(1));

            // Find a matching descriptor
            var desc = events.Find((d) => d.OnlyOnce == onlyOnce && d.AutoDecodePayload == autoDecodePayload);

            // If not found, create one
            if (desc == null)
                events.Add(new EventDescriptor(onlyOnce, autoDecodePayload, callback));
            else // if found, add the new callback
                desc.Callbacks.Add(callback);
        }

        /// <summary>
        /// Removes all events that registered for the given name.
        /// </summary>
        public void Unregister(string eventName)
        {
            Table.Remove(eventName);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Unregister(string eventName, SocketIOCallback callback)
        {
            List<EventDescriptor> events;
            if (Table.TryGetValue(eventName, out events))
                for (int i = 0; i < events.Count; ++i)
                    events[i].Callbacks.Remove(callback);
        }

        /// <summary>
        /// Will call the delegates that associated to the given eventName.
        /// </summary>
        public void Call(string eventName, Packet packet, params object[] args)
        {
            List<EventDescriptor> events;

            if (Table.TryGetValue(eventName, out events))
            {
                if (HTTPManager.Logger.Level <= BestHTTP.Logger.Loglevels.All)
                    HTTPManager.Logger.Verbose("EventTable", string.Format("Call - {0} ({1})", eventName, events.Count));

                for (int i = 0; i < events.Count; ++i)
                    events[i].Call(Socket, packet, args);
            }
            else
            {
                if (HTTPManager.Logger.Level <= BestHTTP.Logger.Loglevels.All)
                    HTTPManager.Logger.Verbose("EventTable", string.Format("Call - {0} (0)", eventName));
            }
        }

        /// <summary>
        /// This function will get the eventName from the packet's Payload, and optionally will decode it from Json.
        /// </summary>
        public void Call(Packet packet)
        {
            string eventName = packet.DecodeEventName();
            string typeName = packet.SocketIOEvent != SocketIOEventTypes.Unknown ? EventNames.GetNameFor(packet.SocketIOEvent) : EventNames.GetNameFor(packet.TransportEvent);
            object[] args = null;

            if (!HasSubsciber(eventName) && !HasSubsciber(typeName))
                return;

            // If this is an Event or BinaryEvent message, or we have a subscriber with AutoDecodePayload, then 
            //  we have to decode the packet's Payload.
            if (packet.TransportEvent == TransportEventTypes.Message && (packet.SocketIOEvent == SocketIOEventTypes.Event || packet.SocketIOEvent == SocketIOEventTypes.BinaryEvent) && ShouldDecodePayload(eventName))
                args = packet.Decode(Socket.Manager.Encoder);

            // call event callbacks registered for 'eventName'
            if (!string.IsNullOrEmpty(eventName))
                Call(eventName, packet, args);

            if (!packet.IsDecoded && ShouldDecodePayload(typeName))
                args = packet.Decode(Socket.Manager.Encoder);

            // call event callbacks registered for 'typeName'
            if (!string.IsNullOrEmpty(typeName))
                Call(typeName, packet, args);
        }

        /// <summary>
        /// Remove all event -> delegate association.
        /// </summary>
        public void Clear()
        {
            Table.Clear();
        }

        #region Private Helpers

        /// <summary>
        /// Returns true, if for the given event name there are at least one event that needs a decoded 
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        private bool ShouldDecodePayload(string eventName)
        {
            List<EventDescriptor> events;

            // If we find at least one EventDescriptor with AutoDecodePayload == true, we have to 
            //  decode the whole payload
            if (Table.TryGetValue(eventName, out events))
                for (int i = 0; i < events.Count; ++i)
                    if (events[i].AutoDecodePayload && events[i].Callbacks.Count > 0)
                        return true;

            return false;
        }

        private bool HasSubsciber(string eventName)
        {
            return Table.ContainsKey(eventName);
        }

        #endregion
    }
}

#endif
