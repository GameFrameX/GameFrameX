using System;
using BestHTTP.Logger;

namespace BestHTTP.Connections
{
    public abstract class ConnectionBase : IDisposable
    {
        #region Public Properties

        /// <summary>
        /// The address of the server that this connection is bound to.
        /// </summary>
        public string ServerAddress { get; protected set; }

        /// <summary>
        /// The state of this connection.
        /// </summary>
        public HTTPConnectionStates State { get; internal set; }

        /// <summary>
        /// If the State is HTTPConnectionStates.Processing, then it holds a HTTPRequest instance. Otherwise it's null.
        /// </summary>
        public HTTPRequest CurrentRequest { get; internal set; }

        /// <summary>
        /// How much the connection kept alive after its last request processing.
        /// </summary>
        public virtual TimeSpan KeepAliveTime { get; protected set; }

        public virtual bool CanProcessMultiple { get { return false; } }

        /// <summary>
        /// When we start to process the current request. It's set after the connection is established.
        /// </summary>
        public DateTime StartTime { get; protected set; }

        public Uri LastProcessedUri { get; protected set; }

        public DateTime LastProcessTime { get; protected set; }

        internal LoggingContext Context;

        #endregion

        #region Privates

        private bool IsThreaded;

        #endregion

        public ConnectionBase(string serverAddress)
            :this(serverAddress, true)
        {}

        public ConnectionBase(string serverAddress, bool threaded)
        {
            this.ServerAddress = serverAddress;
            this.State = HTTPConnectionStates.Initial;
            this.LastProcessTime = DateTime.Now;
            this.KeepAliveTime = HTTPManager.MaxConnectionIdleTime;
            this.IsThreaded = threaded;

            this.Context = new LoggingContext(this);
            this.Context.Add("ServerAddress", serverAddress);
            this.Context.Add("Threaded", threaded);
        }

        internal virtual void Process(HTTPRequest request)
        {
            if (State == HTTPConnectionStates.Processing)
                throw new Exception("Connection already processing a request! " + this.ToString());

            StartTime = DateTime.MaxValue;
            State = HTTPConnectionStates.Processing;

            CurrentRequest = request;
            LastProcessedUri = CurrentRequest.CurrentUri;

            if (IsThreaded)
                PlatformSupport.Threading.ThreadedRunner.RunLongLiving(ThreadFunc);
            else
                ThreadFunc();
        }

        protected virtual void ThreadFunc()
        {

        }

        public ShutdownTypes ShutdownType { get; protected set; }

        /// <summary>
        /// Called when the plugin shuts down immediately.
        /// </summary>
        public virtual void Shutdown(ShutdownTypes type)
        {
            this.ShutdownType = type;
        }
       
        #region Dispose Pattern

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        ~ConnectionBase()
        {
            Dispose(false);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("[{0}:{1}]", this.GetHashCode(), this.ServerAddress);
        }

        public virtual bool TestConnection() => true;
    }
}
