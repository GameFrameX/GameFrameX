#if !BESTHTTP_DISABLE_CACHING

using System;
using System.Collections.Generic;
using System.IO;

namespace BestHTTP.Caching
{
    using BestHTTP.Extensions;
    using BestHTTP.PlatformSupport.FileSystem;

    /// <summary>
    /// Holds all metadata that need for efficient caching, so we don't need to touch the disk to load headers.
    /// </summary>
    public class HTTPCacheFileInfo : IComparable<HTTPCacheFileInfo>
    {
        #region Properties

        /// <summary>
        /// The uri that this HTTPCacheFileInfo belongs to.
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// The last access time to this cache entity. The date is in UTC.
        /// </summary>
        public DateTime LastAccess { get; private set; }

        /// <summary>
        /// The length of the cache entity's body.
        /// </summary>
        public int BodyLength { get; internal set; }

        /// <summary>
        /// ETag of the entity.
        /// </summary>
        public string ETag { get; private set; }

        /// <summary>
        /// LastModified date of the entity.
        /// </summary>
        public string LastModified { get; private set; }

        /// <summary>
        /// When the cache will expire.
        /// </summary>
        public DateTime Expires { get; private set; }

        /// <summary>
        /// The age that came with the response
        /// </summary>
        public long Age { get; private set; }

        /// <summary>
        /// Maximum how long the entry should served from the cache without revalidation.
        /// </summary>
        public long MaxAge { get; private set; }

        /// <summary>
        /// The Date that came with the response.
        /// </summary>
        public DateTime Date { get; private set; }

        /// <summary>
        /// Indicates whether the entity must be revalidated with the server or can be serverd directly from the cache without touching the server when the content is considered stale.
        /// </summary>
        public bool MustRevalidate { get; private set; }

        /// <summary>
        /// If it's true, the client always have to revalidate the cached content when it's stale.
        /// </summary>
        public bool NoCache { get; private set; }

        /// <summary>
        /// It's a grace period to serve staled content without revalidation.
        /// </summary>
        public long StaleWhileRevalidate { get; private set; }

        /// <summary>
        /// Allows the client to serve stale content if the server responds with an 5xx error.
        /// </summary>
        public long StaleIfError { get; private set; }

        /// <summary>
        /// The date and time when the HTTPResponse received.
        /// </summary>
        public DateTime Received { get; private set; }

        /// <summary>
        /// Cached path.
        /// </summary>
        public string ConstructedPath { get; private set; }

        /// <summary>
        /// This is the index of the entity. Filenames are generated from this value.
        /// </summary>
        internal UInt64 MappedNameIDX { get; private set; }

        #endregion

        #region Constructors

        internal HTTPCacheFileInfo(Uri uri)
            :this(uri, DateTime.UtcNow, -1)
        {
        }

        internal HTTPCacheFileInfo(Uri uri, DateTime lastAcces, int bodyLength)
        {
            this.Uri = uri;
            this.LastAccess = lastAcces;
            this.BodyLength = bodyLength;
            this.MaxAge = -1;

            this.MappedNameIDX = HTTPCacheService.GetNameIdx();
        }

        internal HTTPCacheFileInfo(Uri uri, System.IO.BinaryReader reader, int version)
        {
            this.Uri = uri;
            this.LastAccess = DateTime.FromBinary(reader.ReadInt64());
            this.BodyLength = reader.ReadInt32();

            switch(version)
            {
                case 3:
                    this.NoCache = reader.ReadBoolean();
                    this.StaleWhileRevalidate = reader.ReadInt64();
                    this.StaleIfError = reader.ReadInt64();
                    goto case 2;

                case 2:
                    this.MappedNameIDX = reader.ReadUInt64();
                    goto case 1;

                case 1:
                {
                    this.ETag = reader.ReadString();
                    this.LastModified = reader.ReadString();
                    this.Expires = DateTime.FromBinary(reader.ReadInt64());
                    this.Age = reader.ReadInt64();
                    this.MaxAge = reader.ReadInt64();
                    this.Date = DateTime.FromBinary(reader.ReadInt64());
                    this.MustRevalidate = reader.ReadBoolean();
                    this.Received = DateTime.FromBinary(reader.ReadInt64());
                    break;
                }
            }
        }

        #endregion

        #region Helper Functions

        internal void SaveTo(System.IO.BinaryWriter writer)
        {
            // base
            writer.Write(this.LastAccess.ToBinary());
            writer.Write(this.BodyLength);

            // version 3
            writer.Write(this.NoCache);
            writer.Write(this.StaleWhileRevalidate);
            writer.Write(this.StaleIfError);

            // version 2
            writer.Write(this.MappedNameIDX);

            // version 1
            writer.Write(this.ETag);
            writer.Write(this.LastModified);
            writer.Write(this.Expires.ToBinary());
            writer.Write(this.Age);
            writer.Write(this.MaxAge);
            writer.Write(this.Date.ToBinary());
            writer.Write(this.MustRevalidate);
            writer.Write(this.Received.ToBinary());
        }

        public string GetPath()
        {
            if (ConstructedPath != null)
                return ConstructedPath;

            return ConstructedPath = System.IO.Path.Combine(HTTPCacheService.CacheFolder, MappedNameIDX.ToString("X"));
        }

        public bool IsExists()
        {
            if (!HTTPCacheService.IsSupported)
                return false;

            return HTTPManager.IOService.FileExists(GetPath());
        }

        internal void Delete()
        {
            if (!HTTPCacheService.IsSupported)
                return;

            string path = GetPath();
            try
            {
                HTTPManager.IOService.FileDelete(path);
            }
            catch
            { }
            finally
            {
                Reset();
            }
        }

        private void Reset()
        {
            // MappedNameIDX will remain the same. When we re-save an entity, it will not reset the MappedNameIDX.
            this.BodyLength = -1;
            this.ETag = string.Empty;
            this.Expires = DateTime.FromBinary(0);
            this.LastModified = string.Empty;
            this.Age = 0;
            this.MaxAge = -1;
            this.Date = DateTime.FromBinary(0);
            this.MustRevalidate = false;
            this.Received = DateTime.FromBinary(0);
            this.NoCache = false;
            this.StaleWhileRevalidate = 0;
            this.StaleIfError = 0;
        }

        #endregion

        #region Caching
        
        internal void SetUpCachingValues(HTTPResponse response)
        {
            response.CacheFileInfo = this;

            this.ETag = response.GetFirstHeaderValue("ETag").ToStr(this.ETag ?? string.Empty);
            this.Expires = response.GetFirstHeaderValue("Expires").ToDateTime(this.Expires);
            this.LastModified = response.GetFirstHeaderValue("Last-Modified").ToStr(this.LastModified ?? string.Empty);

            this.Age = response.GetFirstHeaderValue("Age").ToInt64(this.Age);

            this.Date = response.GetFirstHeaderValue("Date").ToDateTime(this.Date);

            List<string> cacheControls = response.GetHeaderValues("cache-control");
            if (cacheControls != null && cacheControls.Count > 0)
            {
                // Merge all Cache-Control header values into one
                string cacheControl = cacheControls[0];
                for (int i = 1; i < cacheControls.Count; ++i)
                    cacheControl += "," + cacheControls[i];

                if (!string.IsNullOrEmpty(cacheControl))
                {
                    HeaderParser parser = new HeaderParser(cacheControl);

                    if (parser.Values != null)
                    {
                        for (int i = 0; i < parser.Values.Count; ++i)
                        {
                            var kvp = parser.Values[i];

                            switch(kvp.Key.ToLowerInvariant())
                            {
                                case "max-age":
                                    if (kvp.HasValue)
                                    {
                                        // Some cache proxies will return float values
                                        double maxAge;
                                        if (double.TryParse(kvp.Value, out maxAge))
                                            this.MaxAge = (int)maxAge;
                                        else
                                            this.MaxAge = 0;
                                    }
                                    else
                                        this.MaxAge = 0;
                                    break;

                                case "stale-while-revalidate":
                                    this.StaleWhileRevalidate = kvp.HasValue ? kvp.Value.ToInt64(0) : 0;
                                    break;

                                case "stale-if-error":
                                    this.StaleIfError = kvp.HasValue ? kvp.Value.ToInt64(0) : 0;
                                    break;

                                case "must-revalidate":
                                    this.MustRevalidate = true;
                                    break;

                                case "no-cache":
                                    this.NoCache = true;
                                    break;
                            }
                        }
                    }

                    //string[] options = cacheControl.ToLowerInvariant().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    //
                    //string[] kvp = options.FindOption("max-age");
                    //if (kvp != null && kvp.Length > 1)
                    //{
                    //    // Some cache proxies will return float values
                    //    double maxAge;
                    //    if (double.TryParse(kvp[1], out maxAge))
                    //        this.MaxAge = (int)maxAge;
                    //    else
                    //        this.MaxAge = 0;
                    //}
                    //else
                    //    this.MaxAge = 0;
                    //
                    //kvp = options.FindOption("stale-while-revalidate");
                    //if (kvp != null && kvp.Length == 2 && !string.IsNullOrEmpty(kvp[1]))
                    //    this.StaleWhileRevalidate = kvp[1].ToInt64(0);
                    //
                    //kvp = options.FindOption("stale-if-error");
                    //if (kvp != null && kvp.Length == 2 && !string.IsNullOrEmpty(kvp[1]))
                    //    this.StaleIfError = kvp[1].ToInt64(0);
                    //
                    //this.MustRevalidate = cacheControl.Contains("must-revalidate");
                    //this.NoCache = cacheControl.Contains("no-cache");
                }
            }

            this.Received = DateTime.UtcNow;
        }

        /// <summary>
        /// isInError should be true if downloading the content fails, and in that case, it might extend the content's freshness
        /// </summary>
        public bool WillExpireInTheFuture(bool isInError)
        {
            if (!IsExists())
                return false;

            // https://csswizardry.com/2019/03/cache-control-for-civilians/#no-cache
            // no-cache will always hit the network as it has to revalidate with the server before it can release the browser’s cached copy (unless the server responds with a fresher response),
            // but if the server responds favourably, the network transfer is only a file’s headers: the body can be grabbed from cache rather than redownloaded.
            if (this.NoCache)
                return false;

            // http://www.w3.org/Protocols/rfc2616/rfc2616-sec13.html#sec13.2.4 :
            //  The max-age directive takes priority over Expires
            if (MaxAge > 0)
            {
                // Age calculation:
                // http://www.w3.org/Protocols/rfc2616/rfc2616-sec13.html#sec13.2.3

                long apparent_age = Math.Max(0, (long)(this.Received - this.Date).TotalSeconds);
                long corrected_received_age = Math.Max(apparent_age, this.Age);
                long resident_time = (long)(DateTime.UtcNow - this.Date).TotalSeconds;
                long current_age = corrected_received_age + resident_time;

                long maxAge = this.MaxAge + (this.NoCache ? 0 : this.StaleWhileRevalidate) + (isInError ? this.StaleIfError : 0);

                return current_age < maxAge || this.Expires > DateTime.UtcNow;
            }

            return this.Expires > DateTime.UtcNow;
        }

        internal void SetUpRevalidationHeaders(HTTPRequest request)
        {
            if (!IsExists())
                return;

            // -If an entity tag has been provided by the origin server, MUST use that entity tag in any cache-conditional request (using If-Match or If-None-Match).
            // -If only a Last-Modified value has been provided by the origin server, SHOULD use that value in non-subrange cache-conditional requests (using If-Modified-Since).
            // -If both an entity tag and a Last-Modified value have been provided by the origin server, SHOULD use both validators in cache-conditional requests. This allows both HTTP/1.0 and HTTP/1.1 caches to respond appropriately.

            if (!string.IsNullOrEmpty(ETag))
                request.SetHeader("If-None-Match", ETag);

            if (!string.IsNullOrEmpty(LastModified))
                request.SetHeader("If-Modified-Since", LastModified);
        }

        public System.IO.Stream GetBodyStream(out int length)
        {
            if (!IsExists())
            {
                length = 0;
                return null;
            }

            length = BodyLength;

            LastAccess = DateTime.UtcNow;

            Stream stream = HTTPManager.IOService.CreateFileStream(GetPath(), FileStreamModes.OpenRead);
            stream.Seek(-length, System.IO.SeekOrigin.End);

            return stream;
        }

        internal HTTPResponse ReadResponseTo(HTTPRequest request)
        {
            if (!IsExists())
                return null;

            LastAccess = DateTime.UtcNow;

            using (Stream stream = HTTPManager.IOService.CreateFileStream(GetPath(), FileStreamModes.OpenRead))
            {
                var response = new HTTPResponse(request, stream, request.UseStreaming, true);
                response.CacheFileInfo = this;
                response.Receive(BodyLength);
                return response;
            }
        }

        internal void Store(HTTPResponse response)
        {
            if (!HTTPCacheService.IsSupported)
                return;

            string path = GetPath();

            // Path name too long, we don't want to get exceptions
            if (path.Length > HTTPManager.MaxPathLength)
                return;

            if (HTTPManager.IOService.FileExists(path))
                Delete();

            using (Stream writer = HTTPManager.IOService.CreateFileStream(GetPath(), FileStreamModes.Create))
            {
                writer.WriteLine("HTTP/{0}.{1} {2} {3}", response.VersionMajor, response.VersionMinor, response.StatusCode, response.Message);
                foreach (var kvp in response.Headers)
                {
                    for (int i = 0; i < kvp.Value.Count; ++i)
                        writer.WriteLine("{0}: {1}", kvp.Key, kvp.Value[i]);
                }

                writer.WriteLine();

                writer.Write(response.Data, 0, response.Data.Length);
            }

            BodyLength = response.Data.Length;

            LastAccess = DateTime.UtcNow;

            SetUpCachingValues(response);
        }

        internal System.IO.Stream GetSaveStream(HTTPResponse response)
        {
            if (!HTTPCacheService.IsSupported)
                return null;

            LastAccess = DateTime.UtcNow;

            string path = GetPath();

            if (HTTPManager.IOService.FileExists(path))
                Delete();

            // Path name too long, we don't want to get exceptions
            if (path.Length > HTTPManager.MaxPathLength)
                return null;

            // First write out the headers
            using (Stream writer = HTTPManager.IOService.CreateFileStream(GetPath(), FileStreamModes.Create))
            {
                writer.WriteLine("HTTP/1.1 {0} {1}", response.StatusCode, response.Message);
                foreach (var kvp in response.Headers)
                {
                    for (int i = 0; i < kvp.Value.Count; ++i)
                        writer.WriteLine("{0}: {1}", kvp.Key, kvp.Value[i]);
                }

                writer.WriteLine();
            }

            // If caching is enabled and the response is from cache, and no content-length header set, then we set one to the response.
            if (response.IsFromCache && !response.HasHeader("content-length"))
                response.AddHeader("content-length", BodyLength.ToString());

            SetUpCachingValues(response);

            // then create the stream with Append FileMode
            return HTTPManager.IOService.CreateFileStream(GetPath(), FileStreamModes.Append);
        }

        #endregion

        #region IComparable<HTTPCacheFileInfo>

        public int CompareTo(HTTPCacheFileInfo other)
        {
            return this.LastAccess.CompareTo(other.LastAccess);
        }

        #endregion
    }
}

#endif
