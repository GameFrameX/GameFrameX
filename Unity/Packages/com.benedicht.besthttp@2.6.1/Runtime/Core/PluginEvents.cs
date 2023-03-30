using System;
using System.Collections.Concurrent;

using BestHTTP.Logger;

// Required for ConcurrentQueue.Clear extension.
using BestHTTP.Extensions;

namespace BestHTTP.Core
{
    public enum PluginEvents
    {
#if !BESTHTTP_DISABLE_COOKIES
        SaveCookieLibrary,
#endif

        SaveCacheLibrary,

        AltSvcHeader,

        HTTP2ConnectProtocol
    }

    public
#if CSHARP_7_OR_LATER
        readonly
#endif
        struct PluginEventInfo
    {
        public readonly PluginEvents Event;
        public readonly object Payload;

        public PluginEventInfo(PluginEvents @event)
        {
            this.Event = @event;
            this.Payload = null;
        }

        public PluginEventInfo(PluginEvents @event, object payload)
        {
            this.Event = @event;
            this.Payload = payload;
        }

        public override string ToString()
        {
            return string.Format("[PluginEventInfo Event: {0}]", this.Event);
        }
    }

    public static class PluginEventHelper
    {
        private static ConcurrentQueue<PluginEventInfo> pluginEvents = new ConcurrentQueue<PluginEventInfo>();

#pragma warning disable 0649
        public static Action<PluginEventInfo> OnEvent;
#pragma warning restore

        public static void EnqueuePluginEvent(PluginEventInfo @event)
        {
            if (HTTPManager.Logger.Level == Loglevels.All)
                HTTPManager.Logger.Information("PluginEventHelper", "Enqueue plugin event: " + @event.ToString());

            pluginEvents.Enqueue(@event);
        }

        internal static void Clear()
        {
            pluginEvents.Clear();
        }

        internal static void ProcessQueue()
        {
#if !BESTHTTP_DISABLE_COOKIES
            bool saveCookieLibrary = false;
#endif

#if !BESTHTTP_DISABLE_CACHING
            bool saveCacheLibrary = false;
#endif

            PluginEventInfo pluginEvent;
            while (pluginEvents.TryDequeue(out pluginEvent))
            {
                if (HTTPManager.Logger.Level == Loglevels.All)
                    HTTPManager.Logger.Information("PluginEventHelper", "Processing plugin event: " + pluginEvent.ToString());

                if (OnEvent != null)
                {
                    try
                    {
                        OnEvent(pluginEvent);
                    }
                    catch (Exception ex)
                    {
                        HTTPManager.Logger.Exception("PluginEventHelper", "ProcessQueue", ex);
                    }
                }

                switch (pluginEvent.Event)
                {
#if !BESTHTTP_DISABLE_COOKIES
                    case PluginEvents.SaveCookieLibrary:
                        saveCookieLibrary = true;
                        break;
#endif

#if !BESTHTTP_DISABLE_CACHING
                    case PluginEvents.SaveCacheLibrary:
                        saveCacheLibrary = true;
                        break;
#endif

                    case PluginEvents.AltSvcHeader:
                        AltSvcEventInfo altSvcEventInfo = pluginEvent.Payload as AltSvcEventInfo;
                        HostManager.GetHost(altSvcEventInfo.Host)
                                    .HandleAltSvcHeader(altSvcEventInfo.Response);
                        break;

                    case PluginEvents.HTTP2ConnectProtocol:
                        HTTP2ConnectProtocolInfo info = pluginEvent.Payload as HTTP2ConnectProtocolInfo;
                        HostManager.GetHost(info.Host)
                                    .HandleConnectProtocol(info);
                        break;
                }
            }

#if !BESTHTTP_DISABLE_COOKIES
            if (saveCookieLibrary)
                PlatformSupport.Threading.ThreadedRunner.RunShortLiving(Cookies.CookieJar.Persist);
#endif

#if !BESTHTTP_DISABLE_CACHING
            if (saveCacheLibrary)
                PlatformSupport.Threading.ThreadedRunner.RunShortLiving(Caching.HTTPCacheService.SaveLibrary);
#endif
        }
    }

    public sealed class AltSvcEventInfo
    {
        public readonly string Host;
        public readonly HTTPResponse Response;

        public AltSvcEventInfo(string host, HTTPResponse resp)
        {
            this.Host = host;
            this.Response = resp;
        }
    }

    public sealed class HTTP2ConnectProtocolInfo
    {
        public readonly string Host;
        public readonly bool Enabled;

        public HTTP2ConnectProtocolInfo(string host, bool enabled)
        {
            this.Host = host;
            this.Enabled = enabled;
        }
    }
}
