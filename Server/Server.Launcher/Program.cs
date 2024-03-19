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
            Console.WriteLine("启动参数：" + string.Join(" ", args));

            Console.WriteLine("当前环境变量START---------------------");
            var environmentVariables = Environment.GetEnvironmentVariables();
            foreach (var environmentVariable in environmentVariables)
            {
                Console.WriteLine($"{environmentVariable}");
            }

            Console.WriteLine("当前环境变量END---------------------");
            Console.WriteLine();
            Console.WriteLine();
            var serverType = Environment.GetEnvironmentVariable("ServerType");
            if (serverType != null)
            {
                Console.WriteLine("启动的服务器类型 ServerType: " + serverType);
            }

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
            CacheStateTypeManager.Init();
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
            LogHelper.Info($"----------------------------开始启动服务器啦------------------------------");
            var appSettings = GlobalSettings.GetSettings<AppSetting>();
            if (serverType != null && Enum.TryParse(serverType, out ServerType serverTypeValue))
            {
                var startKv = sortedStartUpTypes.FirstOrDefault(m => m.Value.ServerType == serverTypeValue);
                if (startKv.Value != null)
                {
                    var appSetting = appSettings.FirstOrDefault(m => m.ServerType == serverTypeValue);
                    if (appSetting != null)
                    {
                        LogHelper.Error("从配置文件中找到对应的服务器类型的启动配置,将以配置启动=>" + startKv.Value.ServerType);
                        var task = Start(args, startKv.Key, startKv.Value.ServerType, appSetting);
                        tasks.Add(task);
                    }
                    else
                    {
                        LogHelper.Error("没有找到对应的服务器类型的启动配置,将以默认配置启动=>" + startKv.Value.ServerType);
                        var task = Start(args, startKv.Key, startKv.Value.ServerType, null);
                        tasks.Add(task);
                    }
                }
            }
            else
            {
                foreach (var keyValuePair in sortedStartUpTypes)
                {
                    bool isFind = false;

                    foreach (var appSetting in appSettings)
                    {
                        if (keyValuePair.Value.ServerType == appSetting.ServerType)
                        {
                            var task = Start(args, keyValuePair.Key, appSetting.ServerType, appSetting);
                            tasks.Add(task);
                            isFind = true;
                            break;
                        }
                    }


                    if (isFind == false)
                    {
                        LogHelper.Error("没有找到对应的服务器类型的启动配置,将以默认配置启动=>" + keyValuePair.Value.ServerType);
                        var task = Start(args, keyValuePair.Key, keyValuePair.Value.ServerType, null);
                        tasks.Add(task);
                    }
                }
            }

            await Task.WhenAll(tasks);
        }

        private static Task Start(string[] args, Type appStartUpType, ServerType serverType, BaseSetting setting)
        {
            var startUp = (IAppStartUp)Activator.CreateInstance(appStartUpType);
            if (startUp != null)
            {
                bool isSuccess = startUp.Init(serverType, setting, args);
                if (isSuccess)
                {
                    // LogHelper.Info($"启动服务器类型：{keyValuePair.Value.ServerType},配置信息：{JsonConvert.SerializeObject(appSetting)}");
                    LogHelper.Info($"----------------------------START-----{serverType}------------------------------");
                    var task = AppEnter.Entry(startUp.EnterAsync);
                    LogHelper.Info($"-----------------------------END------{serverType}------------------------------");
                    return task;
                }
            }

            return Task.CompletedTask;
        }
    }
}