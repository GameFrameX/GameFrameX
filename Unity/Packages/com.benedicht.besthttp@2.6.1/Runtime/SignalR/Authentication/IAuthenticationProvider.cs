#if !BESTHTTP_DISABLE_SIGNALR

namespace BestHTTP.SignalR.Authentication
{
    public delegate void OnAuthenticationSuccededDelegate(IAuthenticationProvider provider);
    public delegate void OnAuthenticationFailedDelegate(IAuthenticationProvider provider, string reason);

    public interface IAuthenticationProvider
    {
        /// <summary>
        /// The authentication must be run before any request made to build up the SignalR protocol
        /// </summary>
        bool IsPreAuthRequired { get; }

        /// <summary>
        /// This event must be called when the pre-authentication succeded. When IsPreAuthRequired is false, no-one will subscribe to this event.
        /// </summary>
        event OnAuthenticationSuccededDelegate OnAuthenticationSucceded;

        /// <summary>
        /// This event must be called when the pre-authentication failed. When IsPreAuthRequired is false, no-one will subscribe to this event.
        /// </summary>
        event OnAuthenticationFailedDelegate OnAuthenticationFailed;
        
        /// <summary>
        /// This function called once, when the before the SignalR negotiation begins. If IsPreAuthRequired is false, then this step will be skipped.
        /// </summary>
        void StartAuthentication();

        /// <summary>
        /// This function will be called for every request before sending it.
        /// </summary>
        void PrepareRequest(HTTPRequest request, RequestTypes type);
    }
}

#endif