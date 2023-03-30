namespace BestHTTP.Connections
{
    /// <summary>
    /// Possible states of a Http Connection.
    /// The ideal lifecycle of a connection that has KeepAlive is the following: Initial => [Processing => WaitForRecycle => Free] => Closed.
    /// </summary>
    public enum HTTPConnectionStates
    {
        /// <summary>
        /// This Connection instance is just created.
        /// </summary>
        Initial,

        /// <summary>
        /// This Connection is processing a request
        /// </summary>
        Processing,

        /// <summary>
        /// Wait for the upgraded protocol to shut down.
        /// </summary>
        WaitForProtocolShutdown,

        /// <summary>
        /// The Connection is finished processing the request, it's waiting now to deliver it's result.
        /// </summary>
        Recycle,

        /// <summary>
        /// The request result's delivered, it's now up to processing again.
        /// </summary>
        Free,

        /// <summary>
        /// If it's not a KeepAlive connection, or something happened, then we close this connection and remove from the pool.
        /// </summary>
        Closed,

        /// <summary>
        /// Same as the Closed state, but processing this request requires resending the last processed request too.
        /// </summary>
        ClosedResendRequest
    }
}
