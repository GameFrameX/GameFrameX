using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using BestHTTP.PlatformSupport.Threading;

#if NET_STANDARD_2_0 || NETFX_CORE
using System.Runtime.CompilerServices;
#endif

namespace BestHTTP.PlatformSupport.Memory
{
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppEagerStaticClassConstructionAttribute]
    public struct BufferSegment
    {
        private const int ToStringMaxDumpLength = 128;

        public static readonly BufferSegment Empty = new BufferSegment(null, 0, 0);

        public readonly byte[] Data;
        public readonly int Offset;
        public readonly int Count;

        public BufferSegment(byte[] data, int offset, int count)
        {
            this.Data = data;
            this.Offset = offset;
            this.Count = count;
        }

        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.NullChecks, false)]
        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.ArrayBoundsChecks, false)]
        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.DivideByZeroChecks, false)]
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is BufferSegment))
                return false;

            return Equals((BufferSegment)obj);
        }

        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.NullChecks, false)]
        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.ArrayBoundsChecks, false)]
        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.DivideByZeroChecks, false)]
        public bool Equals(BufferSegment other)
        {
            return this.Data == other.Data &&
                   this.Offset == other.Offset &&
                   this.Count == other.Count;
        }

        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.NullChecks, false)]
        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.ArrayBoundsChecks, false)]
        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.DivideByZeroChecks, false)]
        public override int GetHashCode()
        {
            return (this.Data != null ? this.Data.GetHashCode() : 0) * 21 + this.Offset + this.Count;
        }

        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.NullChecks, false)]
        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.ArrayBoundsChecks, false)]
        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.DivideByZeroChecks, false)]
        public static bool operator ==(BufferSegment left, BufferSegment right)
        {
            return left.Equals(right);
        }

        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.NullChecks, false)]
        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.ArrayBoundsChecks, false)]
        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.DivideByZeroChecks, false)]
        public static bool operator !=(BufferSegment left, BufferSegment right)
        {
            return !left.Equals(right);
        }

        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.NullChecks, false)]
        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.ArrayBoundsChecks, false)]
        [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.DivideByZeroChecks, false)]
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder("[BufferSegment ");
            sb.AppendFormat("Offset: {0} ", this.Offset);
            sb.AppendFormat("Count: {0} ", this.Count);
            sb.Append("Data: [");

            if (this.Count > 0)
            {
                if (this.Count <= ToStringMaxDumpLength)
                {
                    sb.AppendFormat("{0:X2}", this.Data[this.Offset]);
                    for (int i = 1; i < this.Count; ++i)
                        sb.AppendFormat(", {0:X2}", this.Data[this.Offset + i]);
                }
                else
                    sb.Append("...");
            }

            sb.Append("]]");
            return sb.ToString();
        }
    }

    [BestHTTP.PlatformSupport.IL2CPP.Il2CppEagerStaticClassConstructionAttribute]
    public struct PooledBuffer : IDisposable
    {
        public byte[] Data;
        public int Length;

        public void Dispose()
        {
            if (this.Data != null)
                BufferPool.Release(this.Data);
            this.Data = null;
        }
    }

    /// <summary>
    /// Private data struct that contains the size <-> byte arrays mapping. 
    /// </summary>
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppEagerStaticClassConstructionAttribute]
    struct BufferStore
    {
        /// <summary>
        /// Size/length of the arrays stored in the buffers.
        /// </summary>
        public readonly long Size;

        /// <summary>
        /// 
        /// </summary>
        public List<BufferDesc> buffers;

        public BufferStore(long size)
        {
            this.Size = size;
            this.buffers = new List<BufferDesc>();
        }

        /// <summary>
        /// Create a new store with its first byte[] to store.
        /// </summary>
        public BufferStore(long size, byte[] buffer)
            : this(size)
        {
            this.buffers.Add(new BufferDesc(buffer));
        }

        public override string ToString()
        {
            return string.Format("[BufferStore Size: {0:N0}, Buffers: {1}]", this.Size, this.buffers.Count);
        }
    }

    [BestHTTP.PlatformSupport.IL2CPP.Il2CppEagerStaticClassConstructionAttribute]
    struct BufferDesc
    {
        public static readonly BufferDesc Empty = new BufferDesc(null);

        /// <summary>
        /// The actual reference to the stored byte array.
        /// </summary>
        public byte[] buffer;

        /// <summary>
        /// When the buffer is put back to the pool. Based on this value the pool will calculate the age of the buffer.
        /// </summary>
        public DateTime released;

#if UNITY_EDITOR
        public string stackTrace;
#endif

        public BufferDesc(byte[] buff)
        {
            this.buffer = buff;
            this.released = DateTime.UtcNow;
#if UNITY_EDITOR
            if (BufferPool.EnableDebugStackTraceCollection)
                this.stackTrace = ProcessStackTrace(System.Environment.StackTrace);
            else
                this.stackTrace = string.Empty;
#endif
        }

#if UNITY_EDITOR
        private static string ProcessStackTrace(string stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace))
                return null;

            var lines = stackTrace.Split('\n');

            StringBuilder sb = new StringBuilder(lines.Length - 3);
            // skip top 4 lines that would show the logger.
            for (int i = 3; i < lines.Length; ++i)
                sb.Append(lines[i].Replace("BestHTTP.", ""));

            return sb.ToString();
        }
#endif

        public override string ToString()
        {
#if UNITY_EDITOR
            if (BufferPool.EnableDebugStackTraceCollection)
                return string.Format("[BufferDesc Size: {0}, Released: {1}, Released StackTrace: {2}]", this.buffer.Length, DateTime.UtcNow - this.released, this.stackTrace);
            else
                return string.Format("[BufferDesc Size: {0}, Released: {1}]", this.buffer.Length, DateTime.UtcNow - this.released);
#else
            return string.Format("[BufferDesc Size: {0}, Released: {1}]", this.buffer.Length, DateTime.UtcNow - this.released);
#endif
        }
    }

    [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.NullChecks, false)]
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.ArrayBoundsChecks, false)]
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppSetOption(BestHTTP.PlatformSupport.IL2CPP.Option.DivideByZeroChecks, false)]
    [BestHTTP.PlatformSupport.IL2CPP.Il2CppEagerStaticClassConstructionAttribute]
    public static class BufferPool
    {
        public static readonly byte[] NoData = new byte[0];

        /// <summary>
        /// Setting this property to false the pooling mechanism can be disabled.
        /// </summary>
        public static bool IsEnabled {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;

                // When set to non-enabled remove all stored entries
                if (!_isEnabled)
                    Clear();
            }
        }
        private static volatile bool _isEnabled = true;

        /// <summary>
        /// Buffer entries that released back to the pool and older than this value are moved when next maintenance is triggered.
        /// </summary>
        public static TimeSpan RemoveOlderThan = TimeSpan.FromSeconds(30);

        /// <summary>
        /// How often pool maintenance must run.
        /// </summary>
        public static TimeSpan RunMaintenanceEvery = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Minimum buffer size that the plugin will allocate when the requested size is smaller than this value, and canBeLarger is set to true.
        /// </summary>
        public static long MinBufferSize = 32;

        /// <summary>
        /// Maximum size of a buffer that the plugin will store.
        /// </summary>
        public static long MaxBufferSize = long.MaxValue;

        /// <summary>
        /// Maximum accumulated size of the stored buffers.
        /// </summary>
        public static long MaxPoolSize = 10 * 1024 * 1024;

        /// <summary>
        /// Whether to remove empty buffer stores from the free list.
        /// </summary>
        public static bool RemoveEmptyLists = false;

        /// <summary>
        /// If it set to true and a byte[] is released more than once it will log out an error.
        /// </summary>
        public static bool IsDoubleReleaseCheckEnabled = false;

#if UNITY_EDITOR
        /// <summary>
        /// When set to true, the plugin collects Get and Release stack trace informations.
        /// </summary>
        public static bool EnableDebugStackTraceCollection = false;
#endif

        // It must be sorted by buffer size!
        private readonly static List<BufferStore> FreeBuffers = new List<BufferStore>();
        private static DateTime lastMaintenance = DateTime.MinValue;

        // Statistics
        private static long PoolSize = 0;
        private static long GetBuffers = 0;
        private static long ReleaseBuffers = 0;
        private readonly static System.Text.StringBuilder statiscticsBuilder = new System.Text.StringBuilder();

        private readonly static ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

#if UNITY_EDITOR
        private readonly static Dictionary<string, int> getStackStats = new Dictionary<string, int>();
        private readonly static Dictionary<string, int> releaseStackStats = new Dictionary<string, int>();
#endif

        static BufferPool()
        {
#if UNITY_EDITOR
            IsDoubleReleaseCheckEnabled = true;
#else
            IsDoubleReleaseCheckEnabled = false;
#endif
        }

        /// <summary>
        /// Get byte[] from the pool. If canBeLarge is true, the returned buffer might be larger than the requested size.
        /// </summary>
        public static byte[] Get(long size, bool canBeLarger)
        {
            if (!_isEnabled)
                return new byte[size];

            // Return a fix reference for 0 length requests. Any resize call (even Array.Resize) creates a new reference
            //  so we are safe to expose it to multiple callers.
            if (size == 0)
                return BufferPool.NoData;

#if UNITY_EDITOR
            if (EnableDebugStackTraceCollection)
            {
                lock (getStackStats)
                {
                    string stack = ProcessStackTrace(System.Environment.StackTrace);
                    int value;
                    if (!getStackStats.TryGetValue(stack, out value))
                        getStackStats.Add(stack, 1);
                    else
                        getStackStats[stack] = ++value;
                }
            }
#endif

            if (canBeLarger)
            {
                if (size < MinBufferSize)
                    size = MinBufferSize;
                else if (!IsPowerOfTwo(size))
                    size = NextPowerOf2(size);
            }

            if (FreeBuffers.Count == 0)
                return new byte[size];

            BufferDesc bufferDesc = FindFreeBuffer(size, canBeLarger);

            if (bufferDesc.buffer == null)
                return new byte[size];
            else
                Interlocked.Increment(ref GetBuffers);

            Interlocked.Add(ref PoolSize, -bufferDesc.buffer.Length);

            return bufferDesc.buffer;
        }

        /// <summary>
        /// Release back a BufferSegment's data to the pool.
        /// </summary>
        /// <param name="segment"></param>
        public static void Release(BufferSegment segment)
        {
            Release(segment.Data);
        }

        /// <summary>
        /// Release back a byte array to the pool.
        /// </summary>
        public static void Release(byte[] buffer)
        {
            if (!_isEnabled || buffer == null)
                return;

#if UNITY_EDITOR
            if (EnableDebugStackTraceCollection)
            {
                lock (releaseStackStats)
                {
                    string stack = ProcessStackTrace(System.Environment.StackTrace);
                    int value;
                    if (!releaseStackStats.TryGetValue(stack, out value))
                        releaseStackStats.Add(stack, 1);
                    else
                        releaseStackStats[stack] = ++value;
                }
            }
#endif

            int size = buffer.Length;

            if (size == 0 || size > MaxBufferSize)
                return;

            using (new WriteLock(rwLock))
            {
                if (PoolSize + size > MaxPoolSize)
                    return;
                PoolSize += size;

                ReleaseBuffers++;

                AddFreeBuffer(buffer);
            }
        }

        /// <summary>
        /// Resize a byte array. It will release the old one to the pool, and the new one is from the pool too.
        /// </summary>
        public static byte[] Resize(ref byte[] buffer, int newSize, bool canBeLarger, bool clear)
        {
            if (!_isEnabled)
            {
                Array.Resize<byte>(ref buffer, newSize);
                return buffer;
            }

            byte[] newBuf = BufferPool.Get(newSize, canBeLarger);
            if (buffer != null)
            {
                if (!clear)
                    Array.Copy(buffer, 0, newBuf, 0, Math.Min(newBuf.Length, buffer.Length));
                BufferPool.Release(buffer);
            }

            if (clear)
                Array.Clear(newBuf, 0, newSize);

            return buffer = newBuf;
        }

        /// <summary>
        /// Get textual statistics about the buffer pool.
        /// </summary>
        public static string GetStatistics(bool showEmptyBuffers = true)
        {
            using (new ReadLock(rwLock))
            {
                statiscticsBuilder.Length = 0;
                statiscticsBuilder.AppendFormat("Pooled array reused count: {0:N0}\n", GetBuffers);
                statiscticsBuilder.AppendFormat("Release call count: {0:N0}\n", ReleaseBuffers);
                statiscticsBuilder.AppendFormat("PoolSize: {0:N0}\n", PoolSize);
                statiscticsBuilder.AppendFormat("Buffers: {0}\n", FreeBuffers.Count);

                for (int i = 0; i < FreeBuffers.Count; ++i)
                {
                    BufferStore store = FreeBuffers[i];
                    List<BufferDesc> buffers = store.buffers;

                    if (showEmptyBuffers || buffers.Count > 0)
                        statiscticsBuilder.AppendFormat("- Size: {0:N0} Count: {1:N0}\n", store.Size, buffers.Count);
                }

#if UNITY_EDITOR
                if (EnableDebugStackTraceCollection)
                {
                    lock (getStackStats)
                    {
                        int sum = 0;
                        foreach (var kvp in getStackStats)
                            sum += kvp.Value;

                        statiscticsBuilder.AppendFormat("Get stacks: {0:N0}\n", sum);

                        foreach (var kvp in getStackStats)
                        {
                            statiscticsBuilder.AppendFormat("- {0:N0}: {1}\n", kvp.Value, kvp.Key);
                        }
                    }

                    lock (releaseStackStats)
                    {
                        int sum = 0;
                        foreach (var kvp in releaseStackStats)
                            sum += kvp.Value;

                        statiscticsBuilder.AppendFormat("Release stacks: {0:N0}\n", sum);

                        foreach (var kvp in releaseStackStats)
                        {
                            statiscticsBuilder.AppendFormat("- {0:N0}: {1}\n", kvp.Value, kvp.Key);
                        }
                    }
                }
#endif

                return statiscticsBuilder.ToString();
            }
        }

        /// <summary>
        /// Remove all stored entries instantly.
        /// </summary>
        public static void Clear()
        {
            using (new WriteLock(rwLock))
            {
                FreeBuffers.Clear();
                PoolSize = 0;
            }
        }

        /// <summary>
        /// Internal function called by the plugin to remove old, non-used buffers.
        /// </summary>
        internal static void Maintain()
        {
            DateTime now = DateTime.UtcNow;
            if (!_isEnabled || lastMaintenance + RunMaintenanceEvery > now)
                return;
            lastMaintenance = now;

            //if (HTTPManager.Logger.Level == Logger.Loglevels.All)
            //    HTTPManager.Logger.Information("BufferPool", "Before Maintain: " + GetStatistics());

            DateTime olderThan = now - RemoveOlderThan;
            using (new WriteLock(rwLock))
            {
                for (int i = 0; i < FreeBuffers.Count; ++i)
                {
                    BufferStore store = FreeBuffers[i];
                    List<BufferDesc> buffers = store.buffers;

                    for (int cv = buffers.Count - 1; cv >= 0; cv--)
                    {
                        BufferDesc desc = buffers[cv];

                        if (desc.released < olderThan)
                        {
                            // buffers stores available buffers ascending by age. So, when we find an old enough, we can
                            //  delete all entries in the [0..cv] range.

                            int removeCount = cv + 1;
                            buffers.RemoveRange(0, removeCount);
                            PoolSize -= (int)(removeCount * store.Size);
                            break;
                        }
                    }

                    if (RemoveEmptyLists && buffers.Count == 0)
                        FreeBuffers.RemoveAt(i--);
                }
            }

            //if (HTTPManager.Logger.Level == Logger.Loglevels.All)
            //    HTTPManager.Logger.Information("BufferPool", "After Maintain: " + GetStatistics());
        }

#region Private helper functions

#if NET_STANDARD_2_0 || NETFX_CORE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool IsPowerOfTwo(long x)
        {
            return (x & (x - 1)) == 0;
        }

#if NET_STANDARD_2_0 || NETFX_CORE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static long NextPowerOf2(long x)
        {
            long pow = 1;
            while (pow <= x)
                pow *= 2;
            return pow;
        }

#if NET_STANDARD_2_0 || NETFX_CORE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static BufferDesc FindFreeBuffer(long size, bool canBeLarger)
        {
            // Previously it was an upgradable read lock, and later a write lock around store.buffers.RemoveAt.
            // However, checking store.buffers.Count in the if statement, and then get the last buffer and finally write lock the RemoveAt call
            //  has plenty of time for race conditions.
            //  Another thread could change store.buffers after checking count and getting the last element and before the write lock,
            //  so in theory we could return with an element and remove another one from the buffers list.
            //  A new FindFreeBuffer call could return it again causing malformed data and/or releasing it could duplicate it in the store.
            // I tried to reproduce both issues (malformed data, duble entries) with a test where creating growin number of threads getting buffers writing to them, check the buffers and finally release them
            //  would fail _only_ if i used a plain Enter/Exit ReadLock pair, or no locking at all.
            // But, because there's quite a few different platforms and unity's implementation can be different too, switching from an upgradable lock to a more stricter write lock seems safer.
            //
            // An interesting read can be found here: https://stackoverflow.com/questions/21411018/readerwriterlockslim-enterupgradeablereadlock-always-a-deadlock

            using (new WriteLock(rwLock))
            {
                for (int i = 0; i < FreeBuffers.Count; ++i)
                {
                    BufferStore store = FreeBuffers[i];

                    if (store.buffers.Count > 0 && (store.Size == size || (canBeLarger && store.Size > size)))
                    {
                        // Getting the last one has two desired effect:
                        //  1.) RemoveAt should be quicker as it don't have to move all the remaining entries
                        //  2.) Old, non-used buffers will age. Getting a buffer and putting it back will not keep buffers fresh.

                        BufferDesc lastFree = store.buffers[store.buffers.Count - 1];
                        store.buffers.RemoveAt(store.buffers.Count - 1);
                        
                        return lastFree;
                    }
                }
            }

            return BufferDesc.Empty;
        }

#if NET_STANDARD_2_0 || NETFX_CORE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static void AddFreeBuffer(byte[] buffer)
        {
            int bufferLength = buffer.Length;

            for (int i = 0; i < FreeBuffers.Count; ++i)
            {
                BufferStore store = FreeBuffers[i];

                if (store.Size == bufferLength)
                {
                    // We highly assume here that every buffer will be released only once.
                    //  Checking for double-release would mean that we have to do another O(n) operation, where n is the
                    //  count of the store's elements.

                    if (IsDoubleReleaseCheckEnabled)
                        for (int cv = 0; cv < store.buffers.Count; ++cv)
                        {
                            var entry = store.buffers[cv];
                            if (System.Object.ReferenceEquals(entry.buffer, buffer))
                            {
                                HTTPManager.Logger.Error("BufferPool", string.Format("Buffer ({0}) already added to the pool!", entry.ToString()));
                                return;
                            }
                        }

                    store.buffers.Add(new BufferDesc(buffer));
                    return;
                }

                if (store.Size > bufferLength)
                {
                    FreeBuffers.Insert(i, new BufferStore(bufferLength, buffer));
                    return;
                }
            }

            // When we reach this point, there's no same sized or larger BufferStore present, so we have to add a new one
            //  to the end of our list.
            FreeBuffers.Add(new BufferStore(bufferLength, buffer));
        }

#if UNITY_EDITOR
        private static string ProcessStackTrace(string stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace))
                return string.Empty;

            var lines = stackTrace.Split('\n');

            StringBuilder sb = new StringBuilder(lines.Length);
            // skip top 4 lines that would show the logger.
            for (int i = 2; i < Math.Min(5, lines.Length); ++i)
                sb.Append(lines[i].Replace("BestHTTP.", ""));

            return sb.ToString();
        }
#endif

#endregion
    }
}
