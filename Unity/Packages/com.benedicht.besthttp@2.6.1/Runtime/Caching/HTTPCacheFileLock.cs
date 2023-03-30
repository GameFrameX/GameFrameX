#if !BESTHTTP_DISABLE_CACHING

using System;
using System.Collections.Generic;

namespace BestHTTP.Caching
{
    //static class HTTPCacheFileLock
    //{
    //    private static Dictionary<Uri, object> FileLocks = new Dictionary<Uri, object>();
    //    //private static object SyncRoot = new object();
    //    private static System.Threading.ReaderWriterLockSlim rwLock = new System.Threading.ReaderWriterLockSlim(System.Threading.LockRecursionPolicy.NoRecursion);
    //
    //    internal static object Acquire(Uri uri)
    //    {
    //        rwLock.EnterUpgradeableReadLock();
    //        try
    //        {
    //            object fileLock;
    //            if (!FileLocks.TryGetValue(uri, out fileLock))
    //            {
    //                rwLock.EnterWriteLock();
    //                try
    //                {
    //                    FileLocks.Add(uri, fileLock = new object());
    //                }
    //                finally
    //                {
    //                    rwLock.ExitWriteLock();
    //                }
    //            }
    //
    //            return fileLock;
    //        }
    //        finally
    //        {
    //            rwLock.ExitUpgradeableReadLock();
    //        }
    //    }
    //}
}

#endif