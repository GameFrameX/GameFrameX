using UnityGameFramework.Runtime;

public static class GameApp
{
    /// <summary>
    /// 获取游戏基础组件。
    /// </summary>
    public static BaseComponent Base
    {
        get
        {
            if (_base == null)
            {
                _base = GameEntry.GetComponent<BaseComponent>();
            }

            return _base;
        }
    }

    private static BaseComponent _base;

    /// <summary>
    /// 获取配置组件。
    /// </summary>
    public static ConfigComponent Config
    {
        get
        {
            if (_config == null)
            {
                _config = GameEntry.GetComponent<ConfigComponent>();
            }

            return _config;
        }
    }

    private static ConfigComponent _config;

    /// <summary>
    /// 获取下载组件。
    /// </summary>
    public static DownloadComponent Download
    {
        get
        {
            if (_download == null)
            {
                _download = GameEntry.GetComponent<DownloadComponent>();
            }

            return _download;
        }
    }

    private static DownloadComponent _download;

    /// <summary>
    /// 获取实体组件。
    /// </summary>
    public static EntityComponent Entity
    {
        get
        {
            if (_entity == null)
            {
                _entity = GameEntry.GetComponent<EntityComponent>();
            }

            return _entity;
        }
    }

    private static EntityComponent _entity;

    /// <summary>
    /// 获取事件组件。
    /// </summary>
    public static EventComponent Event
    {
        get
        {
            if (_event == null)
            {
                _event = GameEntry.GetComponent<EventComponent>();
            }

            return _event;
        }
    }

    private static EventComponent _event;

    /// <summary>
    /// 获取文件系统组件。
    /// </summary>
    public static FileSystemComponent FileSystem
    {
        get
        {
            if (_fileSystem == null)
            {
                _fileSystem = GameEntry.GetComponent<FileSystemComponent>();
            }

            return _fileSystem;
        }
    }

    private static FileSystemComponent _fileSystem;

    /// <summary>
    /// 获取有限状态机组件。
    /// </summary>
    public static FsmComponent Fsm
    {
        get
        {
            if (_fsm == null)
            {
                _fsm = GameEntry.GetComponent<FsmComponent>();
            }

            return _fsm;
        }
    }

    private static FsmComponent _fsm;

    /// <summary>
    /// 获取本地化组件。
    /// </summary>
    public static LocalizationComponent Localization
    {
        get
        {
            if (_localization == null)
            {
                _localization = GameEntry.GetComponent<LocalizationComponent>();
            }

            return _localization;
        }
    }

    private static LocalizationComponent _localization;

    /// <summary>
    /// 获取网络组件。
    /// </summary>
    public static NetworkComponent Network
    {
        get
        {
            if (_network == null)
            {
                _network = GameEntry.GetComponent<NetworkComponent>();
            }

            return _network;
        }
    }

    private static NetworkComponent _network;

    /// <summary>
    /// 获取对象池组件。
    /// </summary>
    public static ObjectPoolComponent ObjectPool
    {
        get
        {
            if (_objectPool == null)
            {
                _objectPool = GameEntry.GetComponent<ObjectPoolComponent>();
            }

            return _objectPool;
        }
    }

    private static ObjectPoolComponent _objectPool;

    /// <summary>
    /// 获取流程组件。
    /// </summary>
    public static ProcedureComponent Procedure
    {
        get
        {
            if (_procedure == null)
            {
                _procedure = GameEntry.GetComponent<ProcedureComponent>();
            }

            return _procedure;
        }
    }

    private static ProcedureComponent _procedure;

    /// <summary>
    /// 获取资源组件。
    /// </summary>
    public static ResourceComponent Resource
    {
        get
        {
            if (_resource == null)
            {
                _resource = GameEntry.GetComponent<ResourceComponent>();
            }

            return _resource;
        }
    }

    private static ResourceComponent _resource;

    /// <summary>
    /// 获取场景组件。
    /// </summary>
    public static SceneComponent Scene
    {
        get
        {
            if (_scene == null)
            {
                _scene = GameEntry.GetComponent<SceneComponent>();
            }

            return _scene;
        }
    }

    private static SceneComponent _scene;

    /// <summary>
    /// 获取配置组件。
    /// </summary>
    public static SettingComponent Setting
    {
        get
        {
            if (_setting)
            {
                _setting = GameEntry.GetComponent<SettingComponent>();
            }

            return _setting;
        }
    }

    private static SettingComponent _setting;

    /// <summary>
    /// 获取声音组件。
    /// </summary>
    public static SoundComponent Sound
    {
        get
        {
            if (_sound == null)
            {
                _sound = GameEntry.GetComponent<SoundComponent>();
            }

            return _sound;
        }
    }

    private static SoundComponent _sound;

    /// <summary>
    /// 获取网络组件。
    /// </summary>
    public static WebRequestComponent WebRequest
    {
        get
        {
            if (_webRequest)
            {
                _webRequest = GameEntry.GetComponent<WebRequestComponent>();
            }

            return _webRequest;
        }
    }

    private static WebRequestComponent _webRequest;

    /// <summary>
    /// 获取网络组件。
    /// </summary>
    public static WebComponent Web
    {
        get
        {
            if (_web == null)
            {
                _web = GameEntry.GetComponent<WebComponent>();
            }

            return _web;
        }
    }

    private static WebComponent _web;


    /// <summary>
    /// 获取UI组件。
    /// </summary>
    public static FUIComponent UI
    {
        get
        {
            if (_ui == null)
            {
                _ui = GameEntry.GetComponent<FUIComponent>();
            }

            return _ui;
        }
    }

    private static FUIComponent _ui;

    /// <summary>
    /// 获取Asset组件。
    /// </summary>
    public static AssetComponent Asset
    {
        get
        {
            if (_asset == null)
            {
                _asset = GameEntry.GetComponent<AssetComponent>();
            }

            return _asset;
        }
    }

    private static AssetComponent _asset;
}