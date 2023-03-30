namespace BestHTTP.Timings
{
    public static class TimingEventNames
    {
        public const string Queued = "Queued";
        public const string Queued_For_Redirection = "Queued for redirection";
        public const string DNS_Lookup = "DNS Lookup";
        public const string TCP_Connection = "TCP Connection";
        public const string Proxy_Negotiation = "Proxy Negotiation";
        public const string TLS_Negotiation = "TLS Negotiation";
        public const string Request_Sent = "Request Sent";
        public const string Waiting_TTFB = "Waiting (TTFB)";
        public const string Headers = "Headers";
        public const string Loading_From_Cache = "Loading from Cache";
        public const string Writing_To_Cache = "Writing to Cache";
        public const string Response_Received = "Response Received";
        public const string Queued_For_Disptach = "Queued for Dispatch";
        public const string Finished = "Finished in";
        public const string Callback = "Callback";
    }
}
