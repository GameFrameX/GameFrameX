using System;

namespace BestHTTP.Logger
{
    public sealed class UnityOutput : ILogOutput
    {
        public void Write(Loglevels level, string logEntry)
        {
            switch (level)
            {
                case Loglevels.All:
                case Loglevels.Information:
                    UnityEngine.Debug.Log(logEntry);
                    break;

                case Loglevels.Warning:
                    UnityEngine.Debug.LogWarning(logEntry);
                    break;

                case Loglevels.Error:
                case Loglevels.Exception:
                    UnityEngine.Debug.LogError(logEntry);
                    break;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
