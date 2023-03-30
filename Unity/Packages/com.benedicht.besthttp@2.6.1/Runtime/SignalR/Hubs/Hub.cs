#if !BESTHTTP_DISABLE_SIGNALR

using System;
using System.Collections.Generic;

using BestHTTP.SignalR.Messages;
using System.Text;

namespace BestHTTP.SignalR.Hubs
{
    public delegate void OnMethodCallDelegate(Hub hub, string method, params object[] args);
    public delegate void OnMethodCallCallbackDelegate(Hub hub, MethodCallMessage methodCall);

    public delegate void OnMethodResultDelegate(Hub hub, ClientMessage originalMessage, ResultMessage result);
    public delegate void OnMethodFailedDelegate(Hub hub, ClientMessage originalMessage, FailureMessage error);
    public delegate void OnMethodProgressDelegate(Hub hub, ClientMessage originialMessage, ProgressMessage progress);

    /// <summary>
    /// Represents a clientside Hub. This class can be used as a base class to encapsulate proxy functionalities.
    /// </summary>
    public class Hub : IHub
    {

        #region Public Properties

        /// <summary>
        /// Name of this hub.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Server and user set state of the hub.
        /// </summary>
        public Dictionary<string, object> State
        {
            // Create only when we need to.
            get
            {
                if (state == null)
                    state = new Dictionary<string, object>();
                return state;
            }
        }
        private Dictionary<string, object> state;

        /// <summary>
        /// Event called every time when the server sends an order to call a method on the client.
        /// </summary>
        public event OnMethodCallDelegate OnMethodCall;

        #endregion

        #region Privates

        /// <summary>
        /// Table of the sent messages. These messages will be removed from this table when a Result message is received from the server.
        /// </summary>
        private Dictionary<UInt64, ClientMessage> SentMessages = new Dictionary<ulong, ClientMessage>();

        /// <summary>
        /// Methodname -> callback delegate mapping. This table stores the server callable functions.
        /// </summary>
        private Dictionary<string, OnMethodCallCallbackDelegate> MethodTable = new Dictionary<string, OnMethodCallCallbackDelegate>();

        /// <summary>
        /// A reusable StringBuilder to save some GC allocs
        /// </summary>
        private StringBuilder builder = new StringBuilder();

        #endregion

        Connection IHub.Connection { get; set; }

        public Hub(string name)
            :this(name, null)
        {

        }

        public Hub(string name, Connection manager)
        {
            this.Name = name;
            (this as IHub).Connection = manager;
        }

        #region Public Hub Functions

        /// <summary>
        /// Registers a callback function to the given method.
        /// </summary>
        public void On(string method, OnMethodCallCallbackDelegate callback)
        {
            MethodTable[method] = callback;
        }

        /// <summary>
        /// Removes callback from the given method.
        /// </summary>
        /// <param name="method"></param>
        public void Off(string method)
        {
            MethodTable[method] = null;
        }

        /// <summary>
        /// Orders the server to call a method with the given arguments.
        /// </summary>
        /// <returns>True if the plugin was able to send out the message</returns>
        public bool Call(string method, params object[] args)
        {
            return Call(method, null, null, null, args);
        }

        /// <summary>
        /// Orders the server to call a method with the given arguments.
        /// The onResult callback will be called when the server successfully called the function.
        /// </summary>
        /// <returns>True if the plugin was able to send out the message</returns>
        public bool Call(string method, OnMethodResultDelegate onResult, params object[] args)
        {
            return Call(method, onResult, null, null, args);
        }

        /// <summary>
        /// Orders the server to call a method with the given arguments.
        /// The onResult callback will be called when the server successfully called the function.
        /// The onResultError will be called when the server can't call the function, or when the function throws an exception.
        /// </summary>
        /// <returns>True if the plugin was able to send out the message</returns>
        public bool Call(string method, OnMethodResultDelegate onResult, OnMethodFailedDelegate onResultError, params object[] args)
        {
            return Call(method, onResult, onResultError, null, args);
        }

        /// <summary>
        /// Orders the server to call a method with the given arguments.
        /// The onResult callback will be called when the server successfully called the function.
        /// The onProgress callback called multiple times when the method is a long running function and reports back its progress.
        /// </summary>
        /// <returns>True if the plugin was able to send out the message</returns>
        public bool Call(string method, OnMethodResultDelegate onResult, OnMethodProgressDelegate onProgress, params object[] args)
        {
            return Call(method, onResult, null, onProgress, args);
        }

        /// <summary>
        /// Orders the server to call a method with the given arguments.
        /// The onResult callback will be called when the server successfully called the function.
        /// The onResultError will be called when the server can't call the function, or when the function throws an exception.
        /// The onProgress callback called multiple times when the method is a long running function and reports back its progress.
        /// </summary>
        /// <returns>True if the plugin was able to send out the message</returns>
        public bool Call(string method, OnMethodResultDelegate onResult, OnMethodFailedDelegate onResultError, OnMethodProgressDelegate onProgress, params object[] args)
        {
            IHub thisHub = this as IHub;

            // Start over the counter if we are reached the max value if the long type.
            // While we are using this property only here, we don't want to make it static to avoid another thread synchronization, neither we want to make it a Hub-instance field to achieve better debuggability.

            long newValue, originalValue;
            do
            {
                originalValue = thisHub.Connection.ClientMessageCounter;
                newValue = (originalValue % long.MaxValue) + 1;
            } while (System.Threading.Interlocked.CompareExchange(ref thisHub.Connection.ClientMessageCounter, newValue, originalValue) != originalValue);

            // Create and send the client message
            return thisHub.Call(new ClientMessage(this, method, args, (ulong)thisHub.Connection.ClientMessageCounter, onResult, onResultError, onProgress));
        }

        #endregion

        #region IHub Implementation

        bool IHub.Call(ClientMessage msg)
        {
            IHub thisHub = this as IHub;

            if (!thisHub.Connection.SendJson(BuildMessage(msg)))
                return false;

            SentMessages.Add(msg.CallIdx, msg);

            return true;
        }

        /// <summary>
        /// Return true if this hub sent the message with the given id.
        /// </summary>
        bool IHub.HasSentMessageId(UInt64 id)
        {
            return SentMessages.ContainsKey(id);
        }

        /// <summary>
        /// Called on the manager's close.
        /// </summary>
        void IHub.Close()
        {
            SentMessages.Clear();
        }

        /// <summary>
        /// Called when the client receives an order to call a hub-function.
        /// </summary>
        void IHub.OnMethod(MethodCallMessage msg)
        {
            // Merge the newly received states with the old one
            MergeState(msg.State);

            if (OnMethodCall != null)
            {
                try
                {
                    OnMethodCall(this, msg.Method, msg.Arguments);
                }
                catch(Exception ex)
                {
                    HTTPManager.Logger.Exception("Hub - " + this.Name, "IHub.OnMethod - OnMethodCall", ex);
                }
            }

            OnMethodCallCallbackDelegate callback;
            if (MethodTable.TryGetValue(msg.Method, out callback) && callback != null)
            {
                try
                {
                    callback(this, msg);
                }
                catch(Exception ex)
                {
                    HTTPManager.Logger.Exception("Hub - " + this.Name, "IHub.OnMethod - callback", ex);
                }
            }
            else if (OnMethodCall == null)
                HTTPManager.Logger.Warning("Hub - " + this.Name, string.Format("[Client] {0}.{1} (args: {2})", this.Name, msg.Method, msg.Arguments.Length));
        }

        /// <summary>
        /// Called when the client receives back messages as a result of a server method call.
        /// </summary>
        void IHub.OnMessage(IServerMessage msg)
        {
            ClientMessage originalMsg;

            UInt64 id = (msg as IHubMessage).InvocationId;
            if (!SentMessages.TryGetValue(id, out originalMsg))
            {
                // This can happen when a result message removes the ClientMessage from the SentMessages dictionary,
                //  then a late come progress message tries to access it
                HTTPManager.Logger.Warning("Hub - " + this.Name, "OnMessage - Sent message not found with id: " + id.ToString());
                return;
            }

            switch(msg.Type)
            {
                case MessageTypes.Result:
                    ResultMessage result = msg as ResultMessage;

                    // Merge the incoming State before firing the events
                    MergeState(result.State);

                    if (originalMsg.ResultCallback != null)
                    {
                        try
                        {
                            originalMsg.ResultCallback(this, originalMsg, result);
                        }
                        catch(Exception ex)
                        {
                            HTTPManager.Logger.Exception("Hub " + this.Name, "IHub.OnMessage - ResultCallback", ex);
                        }
                    }

                    SentMessages.Remove(id);

                    break;

                case MessageTypes.Failure:
                    FailureMessage error = msg as FailureMessage;

                    // Merge the incoming State before firing the events
                    MergeState(error.State);

                    if (originalMsg.ResultErrorCallback != null)
                    {
                        try
                        {
                            originalMsg.ResultErrorCallback(this, originalMsg, error);
                        }
                        catch(Exception ex)
                        {
                            HTTPManager.Logger.Exception("Hub " + this.Name, "IHub.OnMessage - ResultErrorCallback", ex);
                        }
                    }

                    SentMessages.Remove(id);
                    break;

                case MessageTypes.Progress:
                    if (originalMsg.ProgressCallback != null)
                    {
                        try
                        {
                            originalMsg.ProgressCallback(this, originalMsg, msg as ProgressMessage);
                        }
                        catch(Exception ex)
                        {
                            HTTPManager.Logger.Exception("Hub " + this.Name, "IHub.OnMessage - ProgressCallback", ex);
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Merges the current and the new states.
        /// </summary>
#if BESTHTTP_SIGNALR_WITH_JSONDOTNET
        private void MergeState(IDictionary<string, Newtonsoft.Json.Linq.JToken> state)
#else
        private void MergeState(IDictionary<string, object> state)
#endif
        {
            if (state != null && state.Count > 0)
                foreach (var kvp in state)
                    this.State[kvp.Key] = kvp.Value;
        }

        /// <summary>
        /// Builds a JSon string from the given message.
        /// </summary>
        private string BuildMessage(ClientMessage msg)
        {
            try
            {
                builder.Append("{\"H\":\"");
                builder.Append(this.Name);
                builder.Append("\",\"M\":\"");
                builder.Append(msg.Method);
                builder.Append("\",\"A\":");

                string jsonEncoded = string.Empty;

                // Arguments
                if (msg.Args != null && msg.Args.Length > 0)
                    jsonEncoded = (this as IHub).Connection.JsonEncoder.Encode(msg.Args);
                else
                    jsonEncoded = "[]";

                builder.Append(jsonEncoded);

                builder.Append(",\"I\":\"");
                builder.Append(msg.CallIdx.ToString());
                builder.Append("\"");

                // State, if any
                if (msg.Hub.state != null && msg.Hub.state.Count > 0)
                {
                    builder.Append(",\"S\":");

                    jsonEncoded = (this as IHub).Connection.JsonEncoder.Encode(msg.Hub.state);
                    builder.Append(jsonEncoded);
                }

                builder.Append("}");

                return builder.ToString();
            }
            catch (Exception ex)
            {
                HTTPManager.Logger.Exception("Hub - " + this.Name, "Send", ex);

                return null;
            }
            finally
            {
                // reset the StringBuilder instance, to reuse next time
                builder.Length = 0;
            }
        }

        #endregion
    }
}

#endif