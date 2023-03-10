namespace Geek.Server
{
    public static class Logger
    {
        public static void LogError(string info)
        {
            UnityEngine.Debug.LogError(info);
        }
    }
}
