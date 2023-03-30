using UnityEngine;
using UnityGameFramework.Runtime;

public sealed class GameApp : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InitBuiltinComponents();
        InitCustomsComponents();
    }

    /// <summary>
    /// 获取游戏网络组件。
    /// </summary>
    // public static GameNetworkComponent GameNetwork { get; private set; }
    private void InitCustomsComponents()
    {
        // GameNetwork = GameEntry.GetComponent<GameNetworkComponent>();
    }

    /// <summary>
    /// 获取游戏基础组件。
    /// </summary>
    public static BaseComponent Base { get; private set; }

    /// <summary>
    /// 获取配置组件。
    /// </summary>
    public static ConfigComponent Config { get; private set; }

    /// <summary>
    /// 获取下载组件。
    /// </summary>
    public static DownloadComponent Download { get; private set; }

    /// <summary>
    /// 获取实体组件。
    /// </summary>
    public static EntityComponent Entity { get; private set; }

    /// <summary>
    /// 获取事件组件。
    /// </summary>
    public static EventComponent Event { get; private set; }

    /// <summary>
    /// 获取文件系统组件。
    /// </summary>
    public static FileSystemComponent FileSystem { get; private set; }

    /// <summary>
    /// 获取有限状态机组件。
    /// </summary>
    public static FsmComponent Fsm { get; private set; }

    /// <summary>
    /// 获取本地化组件。
    /// </summary>
    public static LocalizationComponent Localization { get; private set; }

    /// <summary>
    /// 获取网络组件。
    /// </summary>
    public static NetworkComponent Network { get; private set; }

    /// <summary>
    /// 获取对象池组件。
    /// </summary>
    public static ObjectPoolComponent ObjectPool { get; private set; }

    /// <summary>
    /// 获取流程组件。
    /// </summary>
    public static ProcedureComponent Procedure { get; private set; }

    /// <summary>
    /// 获取资源组件。
    /// </summary>
    public static ResourceComponent Resource { get; private set; }

    /// <summary>
    /// 获取场景组件。
    /// </summary>
    public static SceneComponent Scene { get; private set; }

    /// <summary>
    /// 获取配置组件。
    /// </summary>
    public static SettingComponent Setting { get; private set; }

    /// <summary>
    /// 获取声音组件。
    /// </summary>
    public static SoundComponent Sound { get; private set; }


    /// <summary>
    /// 获取网络组件。
    /// </summary>
    public static WebRequestComponent WebRequest { get; private set; }


    /// <summary>
    /// 获取网络组件。
    /// </summary>
    public static WebComponent Web { get; private set; }

    /// <summary>
    /// 获取UI组件。
    /// </summary>
    public static FUIComponent UI { get; private set; }

    /// <summary>
    /// 获取Asset组件。
    /// </summary>
    public static AssetComponent Asset { get; private set; }

    private static void InitBuiltinComponents()
    {
        Base = UnityGameFramework.Runtime.GameEntry.GetComponent<BaseComponent>();
        Config = UnityGameFramework.Runtime.GameEntry.GetComponent<ConfigComponent>();
        Asset = UnityGameFramework.Runtime.GameEntry.GetComponent<AssetComponent>();
        Download = UnityGameFramework.Runtime.GameEntry.GetComponent<DownloadComponent>();
        Entity = UnityGameFramework.Runtime.GameEntry.GetComponent<EntityComponent>();
        Event = UnityGameFramework.Runtime.GameEntry.GetComponent<EventComponent>();
        FileSystem = UnityGameFramework.Runtime.GameEntry.GetComponent<FileSystemComponent>();
        Fsm = UnityGameFramework.Runtime.GameEntry.GetComponent<FsmComponent>();
        Localization = UnityGameFramework.Runtime.GameEntry.GetComponent<LocalizationComponent>();
        Network = UnityGameFramework.Runtime.GameEntry.GetComponent<NetworkComponent>();
        ObjectPool = UnityGameFramework.Runtime.GameEntry.GetComponent<ObjectPoolComponent>();
        Procedure = UnityGameFramework.Runtime.GameEntry.GetComponent<ProcedureComponent>();
        Resource = UnityGameFramework.Runtime.GameEntry.GetComponent<ResourceComponent>();
        Scene = UnityGameFramework.Runtime.GameEntry.GetComponent<SceneComponent>();
        Setting = UnityGameFramework.Runtime.GameEntry.GetComponent<SettingComponent>();
        Sound = UnityGameFramework.Runtime.GameEntry.GetComponent<SoundComponent>();
        WebRequest = UnityGameFramework.Runtime.GameEntry.GetComponent<WebRequestComponent>();
        Web = UnityGameFramework.Runtime.GameEntry.GetComponent<WebComponent>();
        UI = UnityGameFramework.Runtime.GameEntry.GetComponent<FUIComponent>();
    }
}