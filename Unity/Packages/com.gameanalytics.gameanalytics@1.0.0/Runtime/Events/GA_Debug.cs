/// <summary>
/// This class handles error and exception messages, and makes sure they are added to the Quality category
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GameAnalyticsSDK.Events
{
    public static class GA_Debug
    {
        public static int MaxErrorCount = 10;

        private static int _errorCount = 0;

        private static bool _showLogOnGUI = false;
        public static List<string> Messages;

        /// <summary>
        /// If SubmitErrors is enabled on the GA object this makes sure that any exceptions or errors are submitted to the GA server
        /// </summary>
        /// <param name="logString">
        /// The message <see cref="System.String"/>
        /// </param>
        /// <param name="stackTrace">
        /// The exception stack trace <see cref="System.String"/>
        /// </param>
        /// <param name="type">
        /// The type of the log message (we only submit errors and exceptions to the GA server) <see cref="LogType"/>
        /// </param>
        public static void HandleLog(string logString, string stackTrace, LogType type)
        {
            //Only used for testing
            if (_showLogOnGUI)
            {
                if (Messages == null)
                {
                    Messages = new List<string>();
                }
                Messages.Add(logString);
            }

            //We only submit exceptions and errors
            if (GameAnalytics.SettingsGA != null && GameAnalytics.SettingsGA.SubmitErrors && _errorCount < MaxErrorCount && type != LogType.Log)
            {
                if (string.IsNullOrEmpty (stackTrace)) {
                    stackTrace = "";
                }
                _errorCount++;

                string lString = logString.Replace('"', '\'').Replace('\n', ' ').Replace('\r', ' ');
                string sTrace = stackTrace.Replace('"', '\'').Replace('\n', ' ').Replace('\r', ' ');

                string _message = lString + " " + sTrace;
                if (_message.Length > 8192) {
                    _message = _message.Substring (0, 8191);
                }

                SubmitError(_message, type);
            }
        }

        private static void SubmitError(string message, LogType type)
        {
            GAErrorSeverity severity = GAErrorSeverity.Info;

            switch (type)
            {
            case LogType.Assert:
                severity = GAErrorSeverity.Info;
                break;
            case LogType.Error:
                severity = GAErrorSeverity.Error;
                break;
            case LogType.Exception:
                severity = GAErrorSeverity.Critical;
                break;
            case LogType.Log:
                severity = GAErrorSeverity.Debug;
                break;
            case LogType.Warning:
                severity = GAErrorSeverity.Warning;
                break;
            }

            GA_Error.NewEvent(severity, message, null, false);
        }

        /// <summary>
        /// Used only for testing purposes
        /// </summary>
        public static void EnabledLog ()
        {
            _showLogOnGUI = true;
        }
    }
}
