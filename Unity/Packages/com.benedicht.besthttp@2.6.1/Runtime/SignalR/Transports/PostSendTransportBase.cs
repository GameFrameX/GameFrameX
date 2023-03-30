#if !BESTHTTP_DISABLE_SIGNALR

using System.Collections.Generic;

using BestHTTP.SignalR.Messages;

namespace BestHTTP.SignalR.Transports
{
    /// <summary>
    /// A base class for implementations that must send the data in unique requests. These are currently the LongPolling and ServerSentEvents transports.
    /// </summary>
    public abstract class PostSendTransportBase : TransportBase
    {
        /// <summary>
        /// Sent out send requests. Keeping a reference to them for future use.
        /// </summary>
        protected List<HTTPRequest> sendRequestQueue = new List<HTTPRequest>();

        public PostSendTransportBase(string name, Connection con)
            : base(name, con)
        {

        }

        #region Send Implementation

        protected override void SendImpl(string json)
        {
            var request = new HTTPRequest(Connection.BuildUri(RequestTypes.Send, this), HTTPMethods.Post, true, true, OnSendRequestFinished);

            request.FormUsage = Forms.HTTPFormUsage.UrlEncoded;
            request.AddField("data", json);

            Connection.PrepareRequest(request, RequestTypes.Send);

            request.Send();

            sendRequestQueue.Add(request);
        }

        void OnSendRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            sendRequestQueue.Remove(req);

            // error reason if there is any. We will call the manager's Error function if it's not empty.
            string reason = string.Empty;

            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        HTTPManager.Logger.Information("Transport - " + this.Name, "Send - Request Finished Successfully! " + resp.DataAsText);

                        if (!string.IsNullOrEmpty(resp.DataAsText))
                        {
                            IServerMessage msg = TransportBase.Parse(Connection.JsonEncoder, resp.DataAsText);

                            if (msg != null)
                                Connection.OnMessage(msg);
                        }
                    }
                    else
                        reason = string.Format("Send - Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                                                                                                    resp.StatusCode,
                                                                                                    resp.Message,
                                                                                                    resp.DataAsText);
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    reason = "Send - Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception");
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    reason = "Send - Request Aborted!";
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    reason = "Send - Connection Timed Out!";
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    reason = "Send - Processing the request Timed Out!";
                    break;
            }

            if (!string.IsNullOrEmpty(reason))
                Connection.Error(reason);
        }

        #endregion
    }
}

#endif