using System.Reflection;
using NLog;
using Server.Core.StartUp;
using Server.Core.StartUp.Attributes;
using Server.EntryUtility;
using Server.Extension;
using Server.Launcher.Common;
using Server.Proto;
using Server.Setting;

namespace Server.Launcher
{
    internal static class Program
    {
        private static readonly NLog.Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<Type, StartUpTagAttribute> startUpTypes = new();

        static async Task Main(string[] args)
        {
            GlobalSettings.Load<AppSetting>($"Configs/app_config.json");

            ProtoMessageIdHandler.Init();

            var types = Assembly.GetEntryAssembly()?.GetTypes();
            if (types != null)
            {
                foreach (var type in types)
                {
                    if (type.IsClass && type.IsImplWithInterface(typeof(IAppStartUp)) && type.GetCustomAttribute<StartUpTagAttribute>() != null)
                    {
                        var startUpTag = type.GetCustomAttribute<StartUpTagAttribute>();
                        startUpTypes.Add(type, startUpTag);
                    }
                }
            }

            var sortedStartUpTypes = startUpTypes.OrderBy(m => m.Value.Priority);


            List<Task> tasks = new();


            var appSettings = GlobalSettings.GetSettings<AppSetting>();


            foreach (var keyValuePair in sortedStartUpTypes)
            {
                foreach (var appSetting in appSettings)
                {
                    if (keyValuePair.Value.ServerType == appSetting.ServerType)
                    {
                        var startUp = (IAppStartUp)Activator.CreateInstance(keyValuePair.Key);
                        if (startUp != null)
                        {
                            bool isSuccess = startUp.Init(keyValuePair.Value.ServerType, appSetting, args);
                            if (isSuccess)
                            {
                                var task = AppEnter.Entry(startUp.EnterAsync, Log);
                                tasks.Add(task);
                            }
                        }

                        break;
                    }
                }
            }


            await Task.WhenAll(tasks);
        }
    }
}