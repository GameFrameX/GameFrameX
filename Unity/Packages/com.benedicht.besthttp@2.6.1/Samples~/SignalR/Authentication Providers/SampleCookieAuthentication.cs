#if !BESTHTTP_DISABLE_SIGNALR
#if !BESTHTTP_DISABLE_COOKIES && (!UNITY_WEBGL || UNITY_EDITOR)

using System;

using BestHTTP.Cookies;
using BestHTTP.SignalR.Transports;

namespace BestHTTP.SignalR.Authentication
{
    public sealed class SampleCookieAuthentication : IAuthenticationProvider
    {
        #region Public Properties

        public Uri AuthUri { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public string UserRoles { get; private set; }

        #endregion

        #region IAuthenticationProvider properties

        public bool IsPreAuthRequired { get; private set; }

        public event OnAuthenticationSuccededDelegate OnAuthenticationSucceded;
        public event OnAuthenticationFailedDelegate OnAuthenticationFailed;

        #endregion

        #region Privates

        private HTTPRequest AuthRequest;
        private Cookie Cookie;

        #endregion

        public SampleCookieAuthentication(Uri authUri, string user, string passwd, string roles)
        {
            this.AuthUri = authUri;
            this.UserName = user;
            this.Password = passwd;
            this.UserRoles = roles;
            this.IsPreAuthRequired = true;
        }

        #region IAuthenticationProvider Implementation

        public void StartAuthentication()
        {
            AuthRequest = new HTTPRequest(AuthUri, HTTPMethods.Post, OnAuthRequestFinished);

            // Setup the form
            AuthRequest.AddField("userName", UserName);
            AuthRequest.AddField("Password", Password); // not used in the sample
            AuthRequest.AddField("roles", UserRoles);

            AuthRequest.Send();
        }

        public void PrepareRequest(HTTPRequest request, RequestTypes type)
        {
            // Adding the cookie to the request is not required, as it's managed by the plugin automatically,
            // but for now, we want to be really sure that it's added
            request.Cookies.Add(Cookie);
        }

        #endregion

        #region Request Handler

        void OnAuthRequestFinished(HTTPRequest req, HTTPResponse resp)
        {
            AuthRequest = null;
            string failReason = string.Empty;

            switch (req.State)
            {
                // The request finished without any problem.
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        Cookie = resp.Cookies != null ? resp.Cookies.Find(c => c.Name.Equals(".ASPXAUTH")) : null;

                        if (Cookie != null)
                        {
                            HTTPManager.Logger.Information("CookieAuthentication", "Auth. Cookie found!");

                            if (OnAuthenticationSucceded != null)
                                OnAuthenticationSucceded(this);

                            // return now, all other paths are authentication failures
                            return;
                        }
                        else
                            HTTPManager.Logger.Warning("CookieAuthentication", failReason = "Auth. Cookie NOT found!");
                    }
                    else
                        HTTPManager.Logger.Warning("CookieAuthentication", failReason = string.Format("Request Finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                                                        resp.StatusCode,
                                                        resp.Message,
                                                        resp.DataAsText));
                    break;

                // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
                case HTTPRequestStates.Error:
                    HTTPManager.Logger.Warning("CookieAuthentication", failReason = "Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception"));
                    break;

                // The request aborted, initiated by the user.
                case HTTPRequestStates.Aborted:
                    HTTPManager.Logger.Warning("CookieAuthentication", failReason = "Request Aborted!");
                    break;

                // Connecting to the server is timed out.
                case HTTPRequestStates.ConnectionTimedOut:
                    HTTPManager.Logger.Error("CookieAuthentication", failReason = "Connection Timed Out!");
                    break;

                // The request didn't finished in the given time.
                case HTTPRequestStates.TimedOut:
                    HTTPManager.Logger.Error("CookieAuthentication", failReason = "Processing the request Timed Out!");
                    break;
            }

            if (OnAuthenticationFailed != null)
                OnAuthenticationFailed(this, failReason);
        }

        #endregion
    }
}

#endif
#endif