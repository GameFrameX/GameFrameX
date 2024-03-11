using System.Collections.Concurrent;
using System.Reflection;

using Server.Core.Actors;
using Server.Core.Comps;
using Server.Core.Events;
using Server.Core.Hotfix.Agent;
using Server.Core.Net.BaseHandler;
using Server.Extension;
using Server.Log;
using Server.NetWork.HTTP;
using Server.NetWork.Messages;
using Server.Setting;

namespace Server.Core.Hotfix
{
    internal class HotfixModule
    {
        private DllLoader _dllLoader = null;
        readonly string _dllPath;

        internal IHotfixBridge HotfixBridge { get; private set; }

        internal Assembly HotfixAssembly = null;

        /// <summary>
        /// comp -> compAgent
        /// </summary>
        readonly Dictionary<Type, Type> agentCompMap = new Dictionary<Type, Type>(512);

        readonly Dictionary<Type, Type> compAgentMap = new Dictionary<Type, Type>(512);

        readonly Dictionary<Type, Type> agentAgentWrapperMap = new Dictionary<Type, Type>(512);

        /// <summary>
        /// cmd -> handler
        /// </summary>
        readonly Dictionary<string, BaseHttpHandler> httpHandlerMap = new Dictionary<string, BaseHttpHandler>(512);

        /// <summary>
        /// msgId -> handler
        /// </summary>
        readonly Dictionary<int, Type> tcpHandlerMap = new Dictionary<int, Type>(512);

        /// <summary>
        /// actorType -> evtId -> listeners
        /// </summary>
        readonly Dictionary<ActorType, Dictionary<int, List<IEventListener>>> actorEvtListeners = new Dictionary<ActorType, Dictionary<int, List<IEventListener>>>(512);

        readonly bool useAgentWrapper = true;

        internal HotfixModule(string dllPath)
        {
            _dllPath = dllPath;
        }

        internal HotfixModule()
        {
            HotfixAssembly = Assembly.GetEntryAssembly();

            ParseDll();
        }

        internal bool Init(bool reload)
        {
            bool success = false;
            try
            {
                _dllLoader = new DllLoader(_dllPath);
                HotfixAssembly = _dllLoader.HotfixDll;
                if (!reload)
                {
                    // 启动服务器时加载关联的dll
                    LoadRefAssemblies();
                }

                ParseDll();

                File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "dllPath.txt"), _dllPath);

                LogHelper.Info($"hotfix dll init success: {_dllPath}");
                success = true;
            }
            catch (Exception e)
            {
                LogHelper.Error($"hotfix dll init failed...\n{e}");
                if (!reload)
                    throw;
            }

            return success;
        }

        public void Unload()
        {
            if (_dllLoader != null)
            {
                var weak = _dllLoader.Unload();
                if (GlobalSettings.IsDebug)
                {
                    //检查hotfix dll是否已经释放
                    Task.Run(async () =>
                    {
                        int tryCount = 0;
                        while (weak.IsAlive && tryCount++ < 10)
                        {
                            await Task.Delay(100);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }

                        LogHelper.Warn($"hotfix dll unloaded {(weak.IsAlive ? "failed" : "success")}");
                    });
                }
            }
        }

        private void LoadRefAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var nameSet = new HashSet<string>(assemblies.Select(t => t.GetName().Name));
            var hotfixRefAssemblies = HotfixAssembly.GetReferencedAssemblies();
            foreach (var refAssembly in hotfixRefAssemblies)
            {
                if (nameSet.Contains(refAssembly.Name))
                {
                    continue;
                }

                var refPath = $"{Environment.CurrentDirectory}/{refAssembly.Name}.dll";
                if (File.Exists(refPath))
                {
                    Assembly.LoadFrom(refPath);
                }
            }
        }

        private void ParseDll()
        {
            var fullName = typeof(IHotfixBridge).FullName;
            var types = HotfixAssembly.GetTypes();
            foreach (var type in types)
            {
                if (!AddAgent(type)
                    && !AddEvent(type)
                    && !AddTcpHandler(type)
                    && !AddHttpHandler(type))
                {
                    if ((HotfixBridge == null && type.GetInterface(fullName) != null))
                    {
                        var bridge = (IHotfixBridge)Activator.CreateInstance(type);
                        if (bridge.BridgeType == ServerType.Game)
                        {
                            HotfixBridge = bridge;
                        }
                    }
                }
            }
        }

        private bool AddHttpHandler(Type type)
        {
            if (!type.IsSubclassOf(typeof(BaseHttpHandler)))
            {
                return false;
            }

            var attr = (HttpMsgMappingAttribute)type.GetCustomAttribute(typeof(HttpMsgMappingAttribute));
            if (attr == null)
            {
                // 不是最终实现类
                return true;
            }

            var handler = (BaseHttpHandler)Activator.CreateInstance(type);
            // 注册原始命令
            if (!httpHandlerMap.TryAdd(attr.OriginalCmd, handler))
            {
                throw new Exception($"http handler cmd重复注册，cmd:{attr.OriginalCmd}");
            }

            // 注册标准化的命名
            if (!httpHandlerMap.TryAdd(attr.StandardCmd, handler))
            {
                throw new Exception($"http handler cmd重复注册，cmd:{attr.OriginalCmd}");
            }

            return true;
        }

        private bool AddTcpHandler(Type type)
        {
            var attribute = (MessageMappingAttribute)type.GetCustomAttribute(typeof(MessageMappingAttribute), true);
            if (attribute == null)
            {
                return false;
            }

            var msgIdField = (MessageTypeHandlerAttribute)attribute.MessageType.GetCustomAttribute(typeof(MessageTypeHandlerAttribute), true);
            if (msgIdField == null)
            {
                return false;
            }

            int msgId = msgIdField.MessageId;
            if (!tcpHandlerMap.ContainsKey(msgId))
            {
                tcpHandlerMap.Add(msgId, type);
            }
            else
            {
                LogHelper.Error("重复注册消息tcp handler:[{}] msg:[{}]", msgId, type);
            }

            return true;
        }

        private bool AddEvent(Type type)
        {
            if (!type.IsImplWithInterface(typeof(IEventListener)))
            {
                return false;
            }

            var compAgentType = type.BaseType.GetGenericArguments()[0];
            var compType = compAgentType.BaseType.GetGenericArguments()[0];
            var actorType = ComponentRegister.CompActorDic[compType];
            var evtListenersDic = actorEvtListeners.GetOrAdd(actorType);

            bool find = false;
            foreach (var attr in type.GetCustomAttributes())
            {
                if (attr is EventInfoAttribute evt)
                {
                    find = true;

                    var evtId = evt.EventId;
                    var listeners = evtListenersDic.GetOrAdd(evtId);
                    listeners.Add((IEventListener)Activator.CreateInstance(type));
                }
            }

            if (!find)
            {
                throw new Exception($"IEventListener:{type.FullName}没有指定监听的事件");
            }

            return true;
        }

        private bool AddAgent(Type type)
        {
            if (!type.IsImplWithInterface(typeof(IComponentAgent)))
            {
                return false;
            }

            var fullName = type.FullName;
            if (fullName == "Server.Launcher.Logic.Server.ServerComp")
            {
                return false;
            }

            if (fullName.StartsWith("Server.Hotfix.") && fullName.EndsWith("ComponentAgentWrapper"))
            {
                agentAgentWrapperMap[type.BaseType] = type;
                return true;
            }

            var compType = type.BaseType.GetGenericArguments()[0];
            if (compAgentMap.ContainsKey(compType))
            {
                throw new Exception($"comp:{compType.FullName}有多个agent");
            }

            compAgentMap[compType] = type;
            agentCompMap[type] = compType;
            return true;
        }

        internal BaseMessageHandler GetTcpHandler(int msgId)
        {
            if (tcpHandlerMap.TryGetValue(msgId, out var handlerType))
            {
                var ins = Activator.CreateInstance(handlerType);
                if (ins is BaseMessageHandler handler)
                {
                    return handler;
                }
                else
                {
                    throw new Exception($"错误的tcp handler类型，{ins.GetType().FullName}");
                }
            }

            return null;
            //throw new HandlerNotFoundException($"消息id：{msgId}");
        }

        internal BaseHttpHandler GetHttpHandler(string cmd)
        {
            if (httpHandlerMap.TryGetValue(cmd, out var handler))
            {
                return handler;
            }

            return null;
            // throw new HttpHandlerNotFoundException($"未注册的http命令:{cmd}");
        }

        internal T GetAgent<T>(BaseComponent component) where T : IComponentAgent
        {
            var type = component.GetType();
            if (compAgentMap.TryGetValue(type, out var agentType))
            {
                T agent = default;
                if (useAgentWrapper)
                {
                    if (agentAgentWrapperMap.TryGetValue(agentType, out var warpType))
                    {
                        agent = (T)Activator.CreateInstance(warpType);
                    }
                }

                if (agent == null)
                {
                    agent = (T)Activator.CreateInstance(agentType);
                }

                if (agent == null)
                {
                    throw new ArgumentNullException(nameof(agent));
                }

                agent.Owner = component;
                return agent;
            }

            throw new KeyNotFoundException(nameof(compAgentMap) + " ===>" + nameof(type));
        }

        internal List<IEventListener> FindListeners(ActorType actorType, int evtId)
        {
            if (actorEvtListeners.TryGetValue(actorType, out var evtListeners)
                && evtListeners.TryGetValue(evtId, out var listeners))
            {
                return listeners;
            }

            return null;
        }

        readonly ConcurrentDictionary<string, object> typeCacheMap = new();

        /// <summary>
        /// 获取实例(主要用于获取Event,Timer, Schedule,的Handler实例)
        /// </summary>
        /// <param name="typeName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal T GetInstance<T>(string typeName)
        {
            return (T)typeCacheMap.GetOrAdd(typeName, k => HotfixAssembly.CreateInstance(k));
        }

        internal Type GetAgentType(Type compType)
        {
            compAgentMap.TryGetValue(compType, out var agentType);
            return agentType;
        }

        internal Type GetCompType(Type agentType)
        {
            agentCompMap.TryGetValue(agentType, out var compType);
            return compType;
        }
    }
}