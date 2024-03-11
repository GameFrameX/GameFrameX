using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.EntryUtility;
using Server.Extension;
using Server.Proto;

namespace Server.Launcher
{
    internal static class Program
    {
        private static readonly Dictionary<Type, StartUpTagAttribute> startUpTypes = new();

        static async Task Main(string[] args)
        {
            LoggerHandler.Start();
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                
                NullValueHandling = NullValueHandling.Ignore, // 忽略 null 值
                // Formatting = Formatting.Indented, // 生成格式化的 JSON
                MissingMemberHandling = MissingMemberHandling.Ignore, // 忽略缺失的成员
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter() // 将枚举转换为字符串
                }
            };
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
                bool isFind = false;
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
                                // LogHelper.Info($"启动服务器类型：{keyValuePair.Value.ServerType},配置信息：{JsonConvert.SerializeObject(appSetting)}");
                                var task = AppEnter.Entry(startUp.EnterAsync);
                                tasks.Add(task);
                            }
                        }

                        isFind = true;
                        break;
                    }
                }

                if (isFind == false)
                {
                    LogHelper.Error("没有找到对应的服务器类型的启动配置,已跳过=>" + keyValuePair.Value.ServerType);
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}