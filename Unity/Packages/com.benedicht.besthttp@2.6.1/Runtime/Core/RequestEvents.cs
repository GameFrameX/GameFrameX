using BestHTTP.Extensions;
using BestHTTP.Logger;
using BestHTTP.PlatformSupport.Memory;
using BestHTTP.Timings;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BestHTTP.Core
{
    public enum RequestEvents
    {
        Upgraded,
        DownloadProgress,
        UploadProgress,
        StreamingData,
        StateChange,
        Resend,
        Headers,
        TimingData
    }

    public
#if CSHARP_7_OR_LATER
        readonly
#endif
        struct RequestEventInfo
    {
        public readonly HTTPRequest SourceRequest;
        public readonly RequestEvents Event;

        public readonly HTTPRequestStates State;

        public readonly long Progress;
        public readonly long ProgressLength;

        public readonly byte[] Data;
        public readonly int DataLength;

        // Timing Data
        public readonly string Name;
        public readonly DateTime Time;
        public readonly TimeSpan Duration;

        // Headers
        public readonly Dictionary<string, List<string>> Headers;

        public RequestEventInfo(HTTPRequest request, RequestEvents @event)
        {
            this.SourceRequest = request;
            this.Event = @event;

            this.State = HTTPRequestStates.Initial;

            this.Progress = this.ProgressLength = 0;

            this.Data = null;
            this.DataLength = 0;

            // TimingData
            this.Name = null;
            this.Time = DateTime.MinValue;
            this.Duration = TimeSpan.Zero;

            // Headers
            this.Headers = null;
        }

        public RequestEventInfo(HTTPRequest request, HTTPRequestStates newState)
        {
            this.SourceRequest = request;
            this.Event = RequestEvents.StateChange;
            this.State = newState;

            this.Progress = this.ProgressLength = 0;
            this.Data = null;
            this.DataLength = 0;

            // TimingData
            this.Name = null;
            this.Time = DateTime.MinValue;
            this.Duration = TimeSpan.Zero;

            // Headers
            this.Headers = null;
        }

        public RequestEventInfo(HTTPRequest request, RequestEvents @event, long progress, long progressLength)
        {
            this.SourceRequest = request;
            this.Event = @event;
            this.State = HTTPRequestStates.Initial;

            this.Progress = progress;
            this.ProgressLength = progressLength;
            this.Data = null;
            this.DataLength = 0;

            // TimingData
            this.Name = null;
            this.Time = DateTime.MinValue;
            this.Duration = TimeSpan.Zero;

            // Headers
            this.Headers = null;
        }

        public RequestEventInfo(HTTPRequest request, byte[] data, int dataLength)
        {
            this.SourceRequest = request;
            this.Event = RequestEvents.StreamingData;
            this.State = HTTPRequestStates.Initial;

            this.Progress = this.ProgressLength = 0;
            this.Data = data;
            this.DataLength = dataLength;

            // TimingData
            this.Name = null;
            this.Time = DateTime.MinValue;
            this.Duration = TimeSpan.Zero;

            // Headers
            this.Headers = null;
        }

        public RequestEventInfo(HTTPRequest request, string name, DateTime time)
        {
            this.SourceRequest = request;
            this.Event = RequestEvents.TimingData;
            this.State = HTTPRequestStates.Initial;

            this.Progress = this.ProgressLength = 0;
            this.Data = null;
            this.DataLength = 0;

            // TimingData
            this.Name = name;
            this.Time = time;
            this.Duration = TimeSpan.Zero;

            // Headers
            this.Headers = null;
        }

        public RequestEventInfo(HTTPRequest request, string name, TimeSpan duration)
        {
            this.SourceRequest = request;
            this.Event = RequestEvents.TimingData;
            this.State = HTTPRequestStates.Initial;

            this.Progress = this.ProgressLength = 0;
            this.Data = null;
            this.DataLength = 0;

            // TimingData
            this.Name = name;
            this.Time = DateTime.Now;
            this.Duration = duration;

            // Headers
            this.Headers = null;
        }

        public RequestEventInfo(HTTPRequest request, Dictionary<string, List<string>> headers)
        {
            this.SourceRequest = request;
            this.Event = RequestEvents.Headers;
            this.State = HTTPRequestStates.Initial;

            this.Progress = this.ProgressLength = 0;
            this.Data = null;
            this.DataLength = 0;

            // TimingData
            this.Name = null;
            this.Time = DateTime.MinValue;
            this.Duration = TimeSpan.Zero;

            // Headers
            this.Headers = headers;
        }

        public override string ToString()
        {
            switch (this.Event)
            {
                case RequestEvents.Upgraded:
                    return string.Format("[RequestEventInfo SourceRequest: {0}, Event: Upgraded]", this.SourceRequest.CurrentUri);
                case RequestEvents.DownloadProgress:
                    return string.Format("[RequestEventInfo SourceRequest: {0}, Event: DownloadProgress, Progress: {1}, ProgressLength: {2}]", this.SourceRequest.CurrentUri, this.Progress, this.ProgressLength);
                case RequestEvents.UploadProgress:
                    return string.Format("[RequestEventInfo SourceRequest: {0}, Event: UploadProgress, Progress: {1}, ProgressLength: {2}]", this.SourceRequest.CurrentUri, this.Progress, this.ProgressLength);
                case RequestEvents.StreamingData:
                    return string.Format("[RequestEventInfo SourceRequest: {0}, Event: StreamingData, DataLength: {1}]", this.SourceRequest.CurrentUri, this.DataLength);
                case RequestEvents.StateChange:
                    return string.Format("[RequestEventInfo SourceRequest: {0}, Event: StateChange, State: {1}]", this.SourceRequest.CurrentUri, this.State);
                case RequestEvents.Resend:
                    return string.Format("[RequestEventInfo SourceRequest: {0}, Event: Resend]", this.SourceRequest.CurrentUri);
                case RequestEvents.Headers:
                    return string.Format("[RequestEventInfo SourceRequest: {0}, Event: Headers]", this.SourceRequest.CurrentUri);
                case RequestEvents.TimingData:
                    if (this.Duration == TimeSpan.Zero)
                        return string.Format("[RequestEventInfo SourceRequest: {0}, Event: TimingData, Name: {1}, Time: {2}]", this.SourceRequest.CurrentUri, this.Name, this.Time);
                    else
                        return string.Format("[RequestEventInfo SourceRequest: {0}, Event: TimingData, Name: {1}, Time: {2}, Duration: {3}]", this.SourceRequest.CurrentUri, this.Name, this.Time, this.Duration);
                default:
                    throw new NotImplementedException(this.Event.ToString());
            }
        }
    }

    public static class RequestEventHelper
    {
        private static ConcurrentQueue<RequestEventInfo> requestEventQueue = new ConcurrentQueue<RequestEventInfo>();

#pragma warning disable 0649
        public static Action<RequestEventInfo> OnEvent;
#pragma warning restore

        public static void EnqueueRequestEvent(RequestEventInfo @event)
        {
            if (HTTPManager.Logger.Level == Loglevels.All)
                HTTPManager.Logger.Information("RequestEventHelper", "Enqueue request event: " + @event.ToString(), @event.SourceRequest.Context);

            requestEventQueue.Enqueue(@event);
        }

        internal static void Clear()
        {
            requestEventQueue.Clear();
        }

        internal static void ProcessQueue()
        {
            RequestEventInfo requestEvent;
            while (requestEventQueue.TryDequeue(out requestEvent))
            {
                HTTPRequest source = requestEvent.SourceRequest;

                if (HTTPManager.Logger.Level == Loglevels.All)
                    HTTPManager.Logger.Information("RequestEventHelper", "Processing request event: " + requestEvent.ToString(), source.Context);

                if (OnEvent != null)
                {
                    try
                    {
                        OnEvent(requestEvent);
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception("RequestEventHelper", "ProcessQueue", ex, source.Context);
                    }
                }
                
                switch (requestEvent.Event)
                {
                    case RequestEvents.StreamingData:
                        {
                            var response = source.Response;
                            if (response != null)
                                System.Threading.Interlocked.Decrement(ref response.UnprocessedFragments);

                            bool reuseBuffer = true;
                            try
                            {
                                if (source.UseStreaming)
                                    reuseBuffer = source.OnStreamingData(source, response, requestEvent.Data, requestEvent.DataLength);
                            }
                            catch (Exception ex)
                            {
                                HTTPManager.Logger.Exception("RequestEventHelper", "Process RequestEventQueue - RequestEvents.StreamingData", ex, source.Context);
                            }

                            if (reuseBuffer)
                                BufferPool.Release(requestEvent.Data);
                            break;
                        }

                    case RequestEvents.DownloadProgress:
                        try
                        {
                            if (source.OnDownloadProgress != null)
                                source.OnDownloadProgress(source, requestEvent.Progress, requestEvent.ProgressLength);
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("RequestEventHelper", "Process RequestEventQueue - RequestEvents.DownloadProgress", ex, source.Context);
                        }
                        break;

                    case RequestEvents.UploadProgress:
                        try
                        {
                            if (source.OnUploadProgress != null)
                                source.OnUploadProgress(source, requestEvent.Progress, requestEvent.ProgressLength);
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("RequestEventHelper", "Process RequestEventQueue - RequestEvents.UploadProgress", ex, source.Context);
                        }
                        break;

#if !UNITY_WEBGL || UNITY_EDITOR
                    case RequestEvents.Upgraded:
                        try
                        {
                            if (source.OnUpgraded != null)
                                source.OnUpgraded(source, source.Response);
                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("RequestEventHelper", "Process RequestEventQueue - RequestEvents.Upgraded", ex, source.Context);
                        }

                        IProtocol protocol = source.Response as IProtocol;
                        if (protocol != null)
                            ProtocolEventHelper.AddProtocol(protocol);
                        break;
#endif

                    case RequestEvents.Resend:
                        source.State = HTTPRequestStates.Initial;
                        
                        var host = HostManager.GetHost(source.CurrentUri.Host);

                        host.Send(source);

                        break;

                    case RequestEvents.Headers:
                        {
                            try
                            {
                                var response = source.Response;
                                if (source.OnHeadersReceived != null && response != null)
                                    source.OnHeadersReceived(source, response, requestEvent.Headers);
                            }
                            catch (Exception ex)
                            {
                                HTTPManager.Logger.Exception("RequestEventHelper", "Process RequestEventQueue - RequestEvents.Headers", ex, source.Context);
                            }
                            break;
                        }

                    case RequestEvents.StateChange:
                        try
                        {
                            RequestEventHelper.HandleRequestStateChange(requestEvent);
                        }
                        catch(Exception ex)
                        {
                            HTTPManager.Logger.Exception("RequestEventHelper", "HandleRequestStateChange", ex, source.Context);
                        }
                        break;

                    case RequestEvents.TimingData:
                        source.Timing.AddEvent(requestEvent.Name, requestEvent.Time, requestEvent.Duration);
                        break;
                }
            }
        }

        private static bool AbortRequestWhenTimedOut(DateTime now, object context)
        {
            HTTPRequest request = context as HTTPRequest;

            if (request.State >= HTTPRequestStates.Finished)
                return false; // don't repeat

            // Protocols will shut down themselves
            if (request.Response is IProtocol)
                return false;

            if (request.IsTimedOut)
            {
                HTTPManager.Logger.Information("RequestEventHelper", "AbortRequestWhenTimedOut - Request timed out. CurrentUri: " + request.CurrentUri.ToString(), request.Context);
                request.Abort();

                return false; // don't repeat
            }

            return true;  // repeat
        }

        internal static void HandleRequestStateChange(RequestEventInfo @event)
        {
            HTTPRequest source = @event.SourceRequest;

            // Because there's a race condition between setting the request's State in its Abort() function running on Unity's main thread
            //  and the HTTP1/HTTP2 handlers running on an another one.
            // Because of these race conditions cases violating expectations can be:
            //  1.) State is finished but the response null
            //  2.) State is (Connection)TimedOut and the response non-null
            // We have to make sure that no callbacks are called twice and in the request must be in a consistent state!

            //    State        | Request
            //   ---------     +---------
            // 1                  Null
            //   Finished      |   Skip
            //   Timeout/Abort |   Deliver
            //                 
            // 2                 Non-Null
            //   Finished      |    Deliver
            //   Timeout/Abort |    Skip

            switch (@event.State)
            {
                case HTTPRequestStates.Queued:
                    source.QueuedAt = DateTime.UtcNow;
                    if ((!source.UseStreaming && source.UploadStream == null) || source.EnableTimoutForStreaming)
                        BestHTTP.Extensions.Timer.Add(new TimerData(TimeSpan.FromSeconds(1), @event.SourceRequest, AbortRequestWhenTimedOut));
                    break;

                case HTTPRequestStates.ConnectionTimedOut:
                case HTTPRequestStates.TimedOut:
                case HTTPRequestStates.Error:
                case HTTPRequestStates.Aborted:
                    source.Response = null;
                    goto case HTTPRequestStates.Finished;

                case HTTPRequestStates.Finished:

#if !BESTHTTP_DISABLE_CACHING
                    // Here we will try to load content for a failed load. Failed load is a request with ConnectionTimedOut, TimedOut or Error state.
                    // A request with Finished state but response with status code >= 500 also something that we will try to load from the cache.
                    // We have to set what we going to try to load here too (other place is inside IsCachedEntityExpiresInTheFuture) as we don't want to load a cached content for
                    // a request that just finished without any problem!

                    try
                    {
                        bool tryLoad = !source.DisableCache && source.State != HTTPRequestStates.Aborted && (source.State != HTTPRequestStates.Finished || source.Response == null || source.Response.StatusCode >= 500);
                        if (tryLoad && Caching.HTTPCacheService.IsCachedEntityExpiresInTheFuture(source))
                        {
                            HTTPManager.Logger.Information("RequestEventHelper", "IsCachedEntityExpiresInTheFuture check returned true! CurrentUri: " + source.CurrentUri.ToString(), source.Context);

                            PlatformSupport.Threading.ThreadedRunner.RunShortLiving<HTTPRequest>((req) =>
                            {
                                // Disable any other cache activity.
                                req.DisableCache = true;

                                var originalState = req.State;
                                if (Connections.ConnectionHelper.TryLoadAllFromCache("RequestEventHelper", req, req.Context))
                                {
                                    if (req.State != HTTPRequestStates.Finished)
                                        req.State = HTTPRequestStates.Finished;
                                    else
                                        RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(req, HTTPRequestStates.Finished));
                                }
                                else
                                {
                                    HTTPManager.Logger.Information("RequestEventHelper", "TryLoadAllFromCache failed to load! CurrentUri: " + req.CurrentUri.ToString(), source.Context);

                                    // If for some reason it couldn't load we place back the request to the queue.
                                    RequestEventHelper.EnqueueRequestEvent(new RequestEventInfo(req, originalState));
                                }
                            }, source);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception("RequestEventHelper", string.Format("HandleRequestStateChange - Cache probe - CurrentUri: \"{0}\" State: {1} StatusCode: {2}", source.CurrentUri, source.State, source.Response != null ? source.Response.StatusCode : 0), ex, source.Context);
                    }
#endif

                    source.Timing.AddEvent(TimingEventNames.Queued_For_Disptach, DateTime.Now, TimeSpan.Zero);
                    source.Timing.AddEvent(TimingEventNames.Finished, DateTime.Now, DateTime.Now - source.Timing.Start);

                    if (source.Callback != null)
                    {
                        try
                        {
                            source.Callback(source, source.Response);

                            source.Timing.AddEvent(TimingEventNames.Callback, DateTime.Now, TimeSpan.Zero);

                            if (HTTPManager.Logger.Level <= Loglevels.Information)
                                HTTPManager.Logger.Information("RequestEventHelper", "Finishing request. Timings: " + source.Timing.ToString(), source.Context);

                        }
                        catch (Exception ex)
                        {
                            HTTPManager.Logger.Exception("RequestEventHelper", "HandleRequestStateChange " + @event.State, ex, source.Context);
                        }
                    }

                    source.Dispose();

                    HostManager.GetHost(source.CurrentUri.Host)
                                .GetHostDefinition(HostDefinition.GetKeyForRequest(source))
                                .TryToSendQueuedRequests();
                    break;
            }
        }
    }
}
