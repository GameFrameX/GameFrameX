using System;

namespace BestHTTP.Logger
{
    /// <summary>
    /// Available logging levels.
    /// </summary>
    public enum Loglevels : int
    {
        /// <summary>
        /// All message will be logged.
        /// </summary>
        All,

        /// <summary>
        /// Only Informations and above will be logged.
        /// </summary>
        Information,

        /// <summary>
        /// Only Warnings and above will be logged.
        /// </summary>
        Warning,

        /// <summary>
        /// Only Errors and above will be logged.
        /// </summary>
        Error,

        /// <summary>
        /// Only Exceptions will be logged.
        /// </summary>
        Exception,

        /// <summary>
        /// No logging will occur.
        /// </summary>
        None
    }

    public interface ILogOutput : IDisposable
    {
        void Write(Loglevels level, string logEntry);
    }

    public interface ILogger
    {
        /// <summary>
        /// The minimum severity to log
        /// </summary>
        Loglevels Level { get; set; }

        ILogOutput Output { get; set; }

        void Verbose(string division, string msg, LoggingContext context1 = null, LoggingContext context2 = null, LoggingContext context3 = null);

        void Information(string division, string msg, LoggingContext context1 = null, LoggingContext context2 = null, LoggingContext context3 = null);

        void Warning(string division, string msg, LoggingContext context1 = null, LoggingContext context2 = null, LoggingContext context3 = null);

        void Error(string division, string msg, LoggingContext context1 = null, LoggingContext context2 = null, LoggingContext context3 = null);

        void Exception(string division, string msg, Exception ex, LoggingContext context1 = null, LoggingContext context2 = null, LoggingContext context3 = null);
    }
}
