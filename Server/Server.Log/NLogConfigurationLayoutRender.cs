using System.Text;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using Server.Setting;

namespace Server.Log
{
    [ThreadAgnostic]
    [LayoutRenderer("NLogConfiguration")]
    public class NLogConfigurationLayoutRender : LayoutRenderer
    {
        private string logConfig;

        private string GetBuildConfig()
        {
            if (!string.IsNullOrEmpty(logConfig))
                return logConfig;

            logConfig = GlobalSettings.IsDebug ? "debug" : "release";
            return logConfig;
        }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(GetBuildConfig());
        }
    }
}