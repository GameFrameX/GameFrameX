using System;
using System.Threading;

namespace BestHTTP.PlatformSupport.Threading
{
    public struct ReadLock : IDisposable
    {
        private ReaderWriterLockSlim rwLock;
        private bool locked;
        public ReadLock(ReaderWriterLockSlim rwLock)
        {
            this.rwLock = rwLock;

            this.locked = this.rwLock.IsReadLockHeld;
            if (!this.locked)
                this.rwLock.EnterReadLock();
        }

        public void Dispose()
        {
            if (!this.locked)
                this.rwLock.ExitReadLock();
        }
    }

    public struct WriteLock : IDisposable
    {
        private ReaderWriterLockSlim rwLock;
        private bool locked;

        public WriteLock(ReaderWriterLockSlim rwLock)
        {
            this.rwLock = rwLock;
            this.locked = rwLock.IsWriteLockHeld;

            if (!locked)
                this.rwLock.EnterWriteLock();
        }

        public void Dispose()
        {
            if (!locked)
                this.rwLock.ExitWriteLock();
        }
    }
}
