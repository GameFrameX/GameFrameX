#if !BESTHTTP_DISABLE_CACHING

using System;

namespace BestHTTP.Caching
{
    public sealed class HTTPCacheMaintananceParams
    {
        /// <summary>
        /// Delete cache entries that accessed older then this value. If TimeSpan.FromSeconds(0) is used then all cache entries will be deleted. With TimeSpan.FromDays(2) entries that older then two days will be deleted.
        /// </summary>
        public TimeSpan DeleteOlder { get; private set; }

        /// <summary>
        /// If the cache is larger then the MaxCacheSize after the first maintanance step, then the maintanance job will forcedelete cache entries starting with the oldest last accessed one.
        /// </summary>
        public ulong MaxCacheSize { get; private set; }

        public HTTPCacheMaintananceParams(TimeSpan deleteOlder, ulong maxCacheSize)
        {
            this.DeleteOlder = deleteOlder;
            this.MaxCacheSize = maxCacheSize;
        }
    }
}

#endif