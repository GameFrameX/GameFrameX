using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ProtoExporterGUI.Resources;

/// <summary>
/// 运行时本地化服务。封装 resx 资源读取与 Culture 切换，
/// 并通过 <see cref="PropertyChanged"/> 通知 XAML 绑定刷新所有本地化文本。
/// </summary>
/// <remarks>
/// 用法：XAML 绑定到 <c>Localization.Instance.ExportType</c> 等属性；
/// 切换语言时调用 <see cref="SetCulture"/>，触发 PropertyChanged 让全部绑定重新求值。
///
/// 设计选择：用单例 + 索引属性而非 DynamicResource，因为本工具字符串数量少、
/// 且需要在 C# 代码（ExportLogger 消息）中复用同一份资源，单例访问更直接。
/// </remarks>
public sealed class Localization : INotifyPropertyChanged
{
    /// <summary>支持的语言（与 resx 附属程序集对应）。</summary>
    public static readonly (string Code, string Display)[] SupportedCultures =
    {
        ("zh-CN", "中文"),
        ("en", "English"),
    };

    // 资源根名必须匹配 SDK 生成的嵌入资源名：<RootNamespace>.<resx 相对路径去掉扩展名>。
    // ProtoExporterGUI 默认 RootNamespace=ProtoExporterGUI，resx 在 Resources/Strings.resx，
    // 故实际嵌入名为 ProtoExporterGUI.Resources.Strings（见 MissingManifestResourceException 错误）。
    private static readonly ResourceManager Manager =
        new("ProtoExporterGUI.Resources.Strings", typeof(Localization).Assembly);

    /// <summary>全局单例，XAML 与代码共用。</summary>
    public static Localization Instance { get; } = new Localization();

    private Localization() { }

    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// 切换 UI Culture 并通知所有绑定刷新。
    /// 无效或缺失的 culture 静默回退到默认 resx（中性资源）。
    /// </summary>
    /// <remarks>
    /// 两个关键修正：
    /// 1. <see cref="CultureInfo.CurrentUICulture"/> 必须显式设置当前线程的值——
    ///    <see cref="CultureInfo.DefaultThreadCurrentUICulture"/> 只影响之后新建的线程，
    ///    不会改变当前线程的 CurrentUICulture，导致 ResourceManager.GetString 仍读旧 culture。
    /// 2. Avalonia 绑定系统不像 WPF 那样把空属性名当作"全部属性已变"——
    ///    必须对每个具名属性显式触发 PropertyChanged，绑定才会重新求值。
    /// </remarks>
    public void SetCulture(string cultureCode)
    {
        if (string.IsNullOrWhiteSpace(cultureCode))
        {
            return;
        }
        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureCode);
            // 当前线程 + 未来新线程都设，确保 ResourceManager.GetString 读到新 culture。
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // 对每个本地化属性显式触发 PropertyChanged，让 XAML 绑定重新求值。
            // （Avalonia 不支持用空字符串 property name 广播。）
            foreach (var name in LocalizedPropertyNames)
            {
                OnPropertyChanged(name);
            }
        }
        catch (CultureNotFoundException)
        {
            // 静默忽略：保持当前语言不变。
        }
    }

    /// <summary>
    /// 所有本地化字符串属性名。用于 SetCulture 时批量触发 PropertyChanged。
    /// 新增属性时务必在此同步登记，否则切换语言后该属性不会刷新。
    /// </summary>
    private static readonly string[] LocalizedPropertyNames =
    {
        nameof(WindowTitle), nameof(ExportType), nameof(Namespace), nameof(InputPath),
        nameof(OutputPath), nameof(UsingStatements), nameof(ImportPath), nameof(RequireComments),
        nameof(GenerateErrorCode), nameof(GenerateDescription), nameof(IsServer), nameof(ServerModeHint),
        nameof(Export), nameof(Help), nameof(Browse), nameof(Language),
        nameof(PickInputFolder), nameof(PickOutputFolder),
        nameof(ErrUnsupportedMode), nameof(ErrInputPathEmpty), nameof(ErrOutputPathEmpty),
        nameof(ErrNamespaceEmpty), nameof(ExportSuccess), nameof(ExportFailed), nameof(HelpOpenFailed),
        nameof(AppSubtitle), nameof(ExportHint), nameof(LogTitle),
        nameof(GroupExportType), nameof(GroupPaths), nameof(GroupGeneration),
        nameof(UsingStatementsHint),
        nameof(GenerateErrorCodeTip), nameof(GenerateDescriptionTip), nameof(IsServerTip), nameof(RequireCommentsTip),
        nameof(ModeTip), nameof(InputPathTip), nameof(OutputPathTip), nameof(NamespaceTip), nameof(ImportPathTip),
        nameof(GroupLockStatus), nameof(LockFilePath), nameof(LockStateFound), nameof(LockStateNotFound),
        nameof(LockFileLastWrite), nameof(LockModuleColumn), nameof(LockModuleNameColumn), nameof(LockMessageCountColumn),
        nameof(LockRetiredCountColumn), nameof(LockEmptyModules), nameof(LockRefresh),
        nameof(LockLoadFailed), nameof(LockSummaryTemplate), nameof(LockPathEmpty),
    };

    /// <summary>
    /// 取本地化字符串。找不到时返回 key 本身（便于发现遗漏的翻译条目）。
    /// </summary>
    public string this[string key]
    {
        get
        {
            var value = Manager.GetString(key, CultureInfo.CurrentUICulture);
            return string.IsNullOrEmpty(value) ? key : value;
        }
    }

    // 强类型访问器：供 XAML 绑定（x:Static 无法索引器，需要具名属性）。
    public string WindowTitle => this[nameof(WindowTitle)];
    public string ExportType => this[nameof(ExportType)];
    public string Namespace => this[nameof(Namespace)];
    public string InputPath => this[nameof(InputPath)];
    public string OutputPath => this[nameof(OutputPath)];
    public string UsingStatements => this[nameof(UsingStatements)];
    public string ImportPath => this[nameof(ImportPath)];
    public string RequireComments => this[nameof(RequireComments)];
    public string GenerateErrorCode => this[nameof(GenerateErrorCode)];
    public string GenerateDescription => this[nameof(GenerateDescription)];
    public string IsServer => this[nameof(IsServer)];
    public string ServerModeHint => this[nameof(ServerModeHint)];
    public string Export => this[nameof(Export)];
    public string Help => this[nameof(Help)];
    public string Browse => this[nameof(Browse)];
    public string Language => this[nameof(Language)];
    public string PickInputFolder => this[nameof(PickInputFolder)];
    public string PickOutputFolder => this[nameof(PickOutputFolder)];
    public string ErrUnsupportedMode => this[nameof(ErrUnsupportedMode)];
    public string ErrInputPathEmpty => this[nameof(ErrInputPathEmpty)];
    public string ErrOutputPathEmpty => this[nameof(ErrOutputPathEmpty)];
    public string ErrNamespaceEmpty => this[nameof(ErrNamespaceEmpty)];
    public string ExportSuccess => this[nameof(ExportSuccess)];
    public string ExportFailed => this[nameof(ExportFailed)];
    public string HelpOpenFailed => this[nameof(HelpOpenFailed)];
    public string AppSubtitle => this[nameof(AppSubtitle)];
    public string ExportHint => this[nameof(ExportHint)];
    public string LogTitle => this[nameof(LogTitle)];
    public string GroupExportType => this[nameof(GroupExportType)];
    public string GroupPaths => this[nameof(GroupPaths)];
    public string GroupGeneration => this[nameof(GroupGeneration)];
    public string UsingStatementsHint => this[nameof(UsingStatementsHint)];
    public string GenerateErrorCodeTip => this[nameof(GenerateErrorCodeTip)];
    public string GenerateDescriptionTip => this[nameof(GenerateDescriptionTip)];
    public string IsServerTip => this[nameof(IsServerTip)];
    public string RequireCommentsTip => this[nameof(RequireCommentsTip)];
    public string ModeTip => this[nameof(ModeTip)];
    public string InputPathTip => this[nameof(InputPathTip)];
    public string OutputPathTip => this[nameof(OutputPathTip)];
    public string NamespaceTip => this[nameof(NamespaceTip)];
    public string ImportPathTip => this[nameof(ImportPathTip)];
    public string GroupLockStatus => this[nameof(GroupLockStatus)];
    public string LockFilePath => this[nameof(LockFilePath)];
    public string LockStateFound => this[nameof(LockStateFound)];
    public string LockStateNotFound => this[nameof(LockStateNotFound)];
    public string LockFileLastWrite => this[nameof(LockFileLastWrite)];
    public string LockModuleColumn => this[nameof(LockModuleColumn)];
    public string LockModuleNameColumn => this[nameof(LockModuleNameColumn)];
    public string LockMessageCountColumn => this[nameof(LockMessageCountColumn)];
    public string LockRetiredCountColumn => this[nameof(LockRetiredCountColumn)];
    public string LockEmptyModules => this[nameof(LockEmptyModules)];
    public string LockRefresh => this[nameof(LockRefresh)];
    public string LockLoadFailed => this[nameof(LockLoadFailed)];
    public string LockSummaryTemplate => this[nameof(LockSummaryTemplate)];
    public string LockPathEmpty => this[nameof(LockPathEmpty)];

    private void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
