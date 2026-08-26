using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GameFrameX.ProtoExport;
using ProtoExporterGUI.Models;
using ProtoExporterGUI.Resources;

namespace ProtoExporterGUI.Views;

public partial class MainWindow : Window
{
    StringWriter stringWriter;
    DispatcherTimer timer;

    /// <summary>
    /// 构造完成标志。Mode_OnSelectionChanged 在 XAML 初始化期间（EndInit 设置
    /// SelectedIndex 时）会被 Avalonia 触发,此时 NameSpaceRow 等后续 x:Name 字段
    /// 还未绑定,null 引用会爆。构造期跳过事件处理,用户交互不受影响。
    /// </summary>
    bool _initialized;

    public MainWindow()
    {
        InitializeComponent();
        // 窗口尺寸已移至 XAML 定义，且允许缩放（原 MaxWidth=450 会阻止拉宽，影响长参数可见性）
        stringWriter = new StringWriter();
        // 不再劫持 Console.SetOut（全局副作用，窗口多次构造会泄漏、与 CLI 共存场景冲突）。
        // 改为把 ExportLogger 网关的输出委托指向本地 StringWriter，仅捕获库内日志。
        ExportLogger.WriteLine = msg => stringWriter.WriteLine(msg);
        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100),
        };
        SettingData.LoadSetting();
        InitLanguageSelector();
        ApplyOptionsToUI(SettingData.GetOptions(this.Mode.SelectionBoxItem?.ToString()));
        UpdateModeSpecificVisibility(this.Mode.SelectionBoxItem?.ToString());
        RefreshLockPanel();
        _initialized = true;
    }

    /// <summary>
    /// 按当前 Mode 切换「命名空间 / Using 语句 / import 路径」三组的可见性。
    /// 规则与 <see cref="ExportAsync"/> 的 needsNamespace 判定一致(数据层校验),
    /// 仅作用于 UI 隐藏;实际校验仍由 ExportAsync 兜底,避免 UI 与逻辑分离后遗漏场景。
    /// </summary>
    private void UpdateModeSpecificVisibility(string modeKey)
    {
        // 模式名解析失败时全部显示,作为最保守的兜底。
        var showNamespace = true;
        var showImportPath = true;
        if (!string.IsNullOrWhiteSpace(modeKey)
            && Enum.TryParse<ModeType>(GetModeTypeFor(modeKey), true, out var modeType))
        {
            showNamespace = modeType == ModeType.CSharp || modeType == ModeType.Cpp || modeType == ModeType.Go;
            showImportPath = modeType == ModeType.TypeScript || modeType == ModeType.Lua;
        }

        this.NameSpaceRow.IsVisible = showNamespace;
        this.UsingStatementsRow.IsVisible = showNamespace;
        this.ImportPathRow.IsVisible = showImportPath;
    }

    /// <summary>
    /// 刷新子 ID lock 状态面板。读取当前模式对应的 MessageIdLockPath 并渲染观测快照。
    /// 面板只做观测显示,不参与导出决策;任何读取失败都显示为错误状态而非抛异常。
    /// </summary>
    private void RefreshLockPanel()
    {
        var modeKey = this.Mode?.SelectionBoxItem?.ToString();
        var options = SettingData.GetOptions(modeKey);
        var lockPath = options?.MessageIdLockPath;

        // 列头文案:语言切换后由本方法整体重刷,无需单独响应 Culture 变化
        this.LockModuleHeader.Text = Localization.Instance.LockModuleColumn;
        this.LockModuleNameHeader.Text = Localization.Instance.LockModuleNameColumn;
        this.LockMessageCountHeader.Text = Localization.Instance.LockMessageCountColumn;
        this.LockRetiredCountHeader.Text = Localization.Instance.LockRetiredCountColumn;

        this.LockPathText.Text = string.IsNullOrWhiteSpace(lockPath)
            ? Localization.Instance.LockPathEmpty
            : lockPath;

        var data = LockPanelData.Observe(lockPath);
        RenderLockPanel(data);
    }

    /// <summary>
    /// 把观测快照渲染到面板控件。按状态切换状态文字、表格可见性与占位文案。
    /// </summary>
    private void RenderLockPanel(LockPanelData data)
    {
        switch (data.State)
        {
            case LockPanelData.LoadState.Found:
                this.LockStateText.Text = Localization.Instance.LockStateFound;
                break;
            case LockPanelData.LoadState.Failed:
                this.LockStateText.Text = Localization.Instance.LockLoadFailed;
                break;
            default:
                this.LockStateText.Text = Localization.Instance.LockStateNotFound;
                break;
        }

        var lastWrite = data.FormatLastWriteTime("yyyy-MM-dd HH:mm:ss");
        this.LockLastWriteText.Text = lastWrite ?? "—";

        this.LockModuleList.Items.Clear();
        foreach (var row in data.Modules)
        {
            this.LockModuleList.Items.Add(row);
        }

        // 占位/错误提示:Found 且有模块时隐藏;未找到显示「未找到」,解析失败显示错误,空 lock 显示空表提示。
        var hasModules = data.State == LockPanelData.LoadState.Found && data.Modules.Count > 0;
        string placeholder;
        if (data.State == LockPanelData.LoadState.Failed)
        {
            placeholder = Localization.Instance.LockLoadFailed + ": " + data.ErrorMessage;
        }
        else if (data.State == LockPanelData.LoadState.NotFound)
        {
            placeholder = Localization.Instance.LockStateNotFound;
        }
        else
        {
            placeholder = Localization.Instance.LockEmptyModules;
        }
        this.LockPlaceholderRow.Text = hasModules ? string.Empty : placeholder;
        this.LockTableSection.IsVisible = data.State == LockPanelData.LoadState.Found;
    }

    private void LockRefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        RefreshLockPanel();
    }

    /// <summary>
    /// UI 模式显示名(ComboBox 项) → 数据层 ModeType 字符串。
    /// 与 <see cref="SettingData"/> 的 Options key 一致:Server/Unity/Godot 都映射到 CSharp。
    /// </summary>
    private static string GetModeTypeFor(string displayKey)
    {
        return displayKey switch
        {
            "Server" or "Unity" or "Godot" => ModeType.CSharp.ToString(),
            "TypeScript" => ModeType.TypeScript.ToString(),
            "C++" => ModeType.Cpp.ToString(),
            "Lua" => ModeType.Lua.ToString(),
            "Go" => ModeType.Go.ToString(),
            _ => displayKey,
        };
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        FlushLog();
    }

    /// <summary>
    /// 把当前 StringWriter 缓冲同步到日志区。限制最大长度避免超大输出卡 UI。
    /// </summary>
    private void FlushLog()
    {
        var output = stringWriter.ToString();
        if (output.Length > 16384)
        {
            output = output.Substring(output.Length - 16384);
        }
        ErrorLog.Text = output;
    }

    /// <summary>
    /// 数据层 LauncherOptions.UsingStatements 以 | 分隔（与 CLI --usingStatements 契约一致），
    /// 但 UI 上单行展示多条 using 难读难编辑。本组 helper 在 UI 与数据之间做 | ↔ 换行 的双向转换。
    /// 规则：每个非空段一行，平台换行符统一用 Environment.NewLine。
    /// </summary>
    internal static string PipeToMultiline(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var parts = value.Split('|', StringSplitOptions.RemoveEmptyEntries)
                          .Select(p => p.Trim())
                          .Where(p => p.Length > 0);
        return string.Join(Environment.NewLine, parts);
    }

    internal static string MultilineToPipe(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        // 兼容 \r\n / \r / \n 三种换行，逐行 trim 后用 | 拼接（与 CLI 解析一致）。
        var lines = value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(l => l.Trim())
                         .Where(l => l.Length > 0);
        return string.Join("|", lines);
    }
    /// <summary>
    /// 将 LauncherOptions 的全部字段同步到 UI 控件。统一用于初始化和模式切换，
    /// 保证 UI 与配置一一对应。null 时跳过（保留控件原值）。
    /// </summary>
    private void ApplyOptionsToUI(LauncherOptions options)
    {
        if (options == null)
        {
            return;
        }

        this.InputPath.Text = options.InputPath;
        this.OutputPath.Text = options.OutputPath;
        this.NameSpace.Text = options.NamespaceName;
        this.IsGenerateErrorCode.IsChecked = options.IsGenerateErrorCode;
        this.UsingStatements.Text = PipeToMultiline(options.UsingStatements);
        this.ImportPath.Text = options.ImportPath;
        this.IsGenerateDescription.IsChecked = options.IsGenerateDescription;
        this.IsServer.IsChecked = options.IsServer;

        // 注释校验级别：ComboBox 按枚举字符串匹配，匹配失败回退到 none（索引 0）
        var requireComments = string.IsNullOrWhiteSpace(options.RequireComments) ? "none" : options.RequireComments;
        var rcIndex = 0;
        for (var i = 0; i < this.RequireComments.ItemCount; i++)
        {
            if (string.Equals(this.RequireComments.Items[i]?.ToString(), requireComments, StringComparison.OrdinalIgnoreCase))
            {
                rcIndex = i;
                break;
            }
        }
        this.RequireComments.SelectedIndex = rcIndex;
    }

    /// <summary>
    /// 导出按钮事件处理器（必须为 async void 以满足 Avalonia 事件签名）。
    /// 仅负责防重入与顶层异常兜底，核心逻辑下沉到 <see cref="ExportAsync"/>，
    /// 异常冒泡至此记录完整堆栈（原实现仅记录 ex.Message，丢失堆栈无法定位问题）。
    /// </summary>
    private async void Button_OnClick(object sender, RoutedEventArgs e)
    {
        // 防重入：导出过程会清空/重建输出目录（ProtoBufMessageHandler.Start），
        // 并发触发会产生目录竞态。点击即禁用，结束时恢复。
        if (!this.ExportButton.IsEnabled)
        {
            return;
        }

        try
        {
            await ExportAsync();
        }
        catch (Exception ex)
        {
            // 记录完整异常（含堆栈与内部异常），便于定位。
            ExportLogger.WriteLine(Localization.Instance.ExportFailed + ": " + ex);
            FlushLog();
            // 自动展开日志，避免用户错过失败原因。
            this.LogExpander.IsExpanded = true;
        }
    }

    /// <summary>
    /// 导出核心流程。验证失败用 return 正常退出（不抛异常）；运行时异常冒泡到
    /// <see cref="Button_OnClick"/> 由顶层 catch 记录完整堆栈。
    /// </summary>
    private async Task ExportAsync()
    {
        this.ExportButton.IsEnabled = false;
        stringWriter.GetStringBuilder().Clear();
        timer.Start();

        try
        {
            var modeName = this.Mode.SelectionBoxItem?.ToString();
            var savedOptions = SettingData.GetOptions(modeName);
            if (savedOptions == null)
            {
                ExportLogger.WriteLine(Localization.Instance.ErrUnsupportedMode);
                return;
            }

            var launcherOptions = new LauncherOptions
            {
                Mode = savedOptions.Mode,
                UsingStatements = MultilineToPipe(this.UsingStatements.Text),
                ImportPath = this.ImportPath.Text,
                IsGenerateDescription = this.IsGenerateDescription.IsChecked ?? false,
                IsServer = this.IsServer.IsChecked ?? false,
                InputPath = this.InputPath.Text,
                OutputPath = this.OutputPath.Text,
                NamespaceName = this.NameSpace.Text,
                IsGenerateErrorCode = this.IsGenerateErrorCode.IsChecked ?? true,
                RequireComments = this.RequireComments.SelectedItem?.ToString() ?? "none",
            };

            if (!Enum.TryParse<ModeType>(launcherOptions.Mode, true, out var modeType))
            {
                ExportLogger.WriteLine(Localization.Instance.ErrUnsupportedMode);
                return;
            }

            if (string.IsNullOrWhiteSpace(launcherOptions.InputPath))
            {
                ExportLogger.WriteLine(Localization.Instance.ErrInputPathEmpty);
                return;
            }

            if (string.IsNullOrWhiteSpace(launcherOptions.OutputPath))
            {
                ExportLogger.WriteLine(Localization.Instance.ErrOutputPathEmpty);
                return;
            }

            // C# / C++ / Go 模式需要命名空间；TypeScript / Lua 模式忽略此参数（允许留空）。
            var needsNamespace = modeType == ModeType.CSharp || modeType == ModeType.Cpp || modeType == ModeType.Go;
            if (needsNamespace && string.IsNullOrWhiteSpace(launcherOptions.NamespaceName))
            {
                ExportLogger.WriteLine(Localization.Instance.ErrNamespaceEmpty);
                return;
            }

            #region Save

            // 全字段回写：保存用户对任意参数的修改，下次启动时恢复。
            savedOptions.InputPath = launcherOptions.InputPath;
            savedOptions.OutputPath = launcherOptions.OutputPath;
            savedOptions.NamespaceName = launcherOptions.NamespaceName;
            savedOptions.IsGenerateErrorCode = launcherOptions.IsGenerateErrorCode;
            savedOptions.UsingStatements = launcherOptions.UsingStatements;
            savedOptions.ImportPath = launcherOptions.ImportPath;
            savedOptions.IsGenerateDescription = launcherOptions.IsGenerateDescription;
            savedOptions.IsServer = launcherOptions.IsServer;
            savedOptions.RequireComments = launcherOptions.RequireComments;
            SettingData.SaveSetting();

            #endregion

            ProtoBufMessageHandler.Start(launcherOptions, modeType);
            ExportLogger.WriteLine(Localization.Instance.ExportSuccess);
        }
        finally
        {
            // 多停留 500ms 让最后的日志被 DispatcherTimer 刷到 UI，再停止定时器。
            await Task.Delay(500);
            timer.Stop();
            FlushLog();
            UpdateLockSummaryFromLog();
            RefreshLockPanel();
            this.ExportButton.IsEnabled = true;
        }
    }

    /// <summary>
    /// 从本次导出日志中提取 lock 变更统计并显示到面板底部。
    /// 导出器会输出一行「[Lock] 涉及模块 N 个，新增 SubId M 条：…」，
    /// GUI 只做观测展示，不重新解析 lock 文件。
    /// </summary>
    private void UpdateLockSummaryFromLog()
    {
        this.LockSummaryText.Text = string.Empty;
        var output = stringWriter.ToString();
        if (string.IsNullOrEmpty(output))
        {
            return;
        }

        var lines = output.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
        // 从后往前找最近一条 [Lock] 统计行,与导出器最后一次落盘保持一致。
        for (var i = lines.Length - 1; i >= 0; i--)
        {
            int moduleCount;
            int newlyAssignedCount;
            if (LockSummaryParser.TryParse(lines[i], out moduleCount, out newlyAssignedCount))
            {
                this.LockSummaryText.Text = string.Format(
                    System.Globalization.CultureInfo.CurrentCulture,
                    Localization.Instance.LockSummaryTemplate,
                    moduleCount,
                    newlyAssignedCount);
                return;
            }
        }
    }

    private void Mode_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 构造期跳过:XAML 解析过程中 Mode.SelectedIndex 初次赋值就会触发此事件,
        // 但后续 x:Name 字段(NameSpaceRow 等)此时还未绑定,直接访问会 NRE。
        if (!_initialized)
        {
            return;
        }
        var modeKey = this.Mode?.SelectionBoxItem?.ToString();
        ApplyOptionsToUI(SettingData.GetOptions(modeKey));
        UpdateModeSpecificVisibility(modeKey);
        // 模式切换可能改变 lock 文件路径(MessageIdLockPath 按模式配置),同步刷新观测面板。
        RefreshLockPanel();
    }

    /// <summary>
    /// 初始化语言下拉框。用 SupportedCultures 填充，默认选中当前 UI Culture。
    /// </summary>
    private void InitLanguageSelector()
    {
        this.LanguageSelector.Items.Clear();
        foreach (var (code, display) in Localization.SupportedCultures)
        {
            this.LanguageSelector.Items.Add(display);
        }
        // 默认选中当前 UI Culture 对应项；未匹配则回退第一项（中文）。
        var current = System.Globalization.CultureInfo.CurrentUICulture.Name;
        var matchIndex = Array.FindIndex(Localization.SupportedCultures, c => c.Code == current);
        // CurrentUICulture 可能是 "zh-CN" 之外的中性名（如 "zh"），宽松匹配首字母。
        if (matchIndex < 0)
        {
            matchIndex = Array.FindIndex(Localization.SupportedCultures, c => current.StartsWith(c.Code.Split('-')[0], StringComparison.OrdinalIgnoreCase));
        }
        this.LanguageSelector.SelectedIndex = matchIndex < 0 ? 0 : matchIndex;
    }

    /// <summary>
    /// 语言切换：按选择索引切 Culture，Localization 触发 PropertyChanged 刷新所有绑定。
    /// </summary>
    private void LanguageSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 构造期间 Items 还没填完会触发，跳过无效索引。
        var idx = this.LanguageSelector.SelectedIndex;
        if (idx < 0 || idx >= Localization.SupportedCultures.Length)
        {
            return;
        }
        Localization.Instance.SetCulture(Localization.SupportedCultures[idx].Code);
        // 面板列头/占位文案由 code-behind 直接赋值,不参与绑定,语言切换后需手动重刷。
        if (_initialized)
        {
            RefreshLockPanel();
        }
    }

    /// <summary>
    /// 选择协议文件目录。跨平台 FolderPicker（Avalonia StorageProvider），
    /// 替代手敲路径。若当前 TextBox 已有有效路径，从中开始浏览。
    /// </summary>
    private async void BrowseInputPath_OnClick(object sender, RoutedEventArgs e)
    {
        var folder = await PickFolder(Localization.Instance.PickInputFolder, this.InputPath.Text);
        if (folder != null)
        {
            this.InputPath.Text = folder;
        }
    }

    /// <summary>
    /// 选择导出文件目录。用户可能选已存在目录（会被工具清空重建）或新目录。
    /// </summary>
    private async void BrowseOutputPath_OnClick(object sender, RoutedEventArgs e)
    {
        var folder = await PickFolder(Localization.Instance.PickOutputFolder, this.OutputPath.Text);
        if (folder != null)
        {
            this.OutputPath.Text = folder;
        }
    }

    /// <summary>
    /// 打开文件夹选择器，返回本地路径字符串；用户取消返回 null。
    /// 起始位置：优先用 current 值（若指向存在的目录），否则交给系统默认。
    /// </summary>
    private async Task<string> PickFolder(string title, string current)
    {
        var startLocation = await TryGetStartLocation(current);
        var options = new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            SuggestedStartLocation = startLocation,
        };
        var folders = await StorageProvider.OpenFolderPickerAsync(options);
        if (folders == null || folders.Count == 0)
        {
            return null;
        }
        return folders[0].Path.LocalPath;
    }

    /// <summary>
    /// 仅当路径指向存在的目录时返回对应的 IStorageFolder，否则 null。
    /// </summary>
    private async Task<IStorageFolder> TryGetStartLocation(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            return null;
        }
        try
        {
            return await StorageProvider.TryGetFolderFromPathAsync(path);
        }
        catch
        {
            return null;
        }
    }

    private void HelpButton_OnClick(object sender, RoutedEventArgs e)
    {
        // 跨平台打开 URL：UseShellExecute=true 在 macOS/Linux 上会抛异常，
        // 按平台选择原生命令（open / xdg-open）或回退到 Windows 的 UseShellExecute。
        var url = "https://gameframex.doc.alianblank.com/tools/index.html";
        try
        {
            if (OperatingSystem.IsMacOS())
            {
                Process.Start(new ProcessStartInfo("open", url));
            }
            else if (OperatingSystem.IsLinux())
            {
                Process.Start(new ProcessStartInfo("xdg-open", url));
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            ExportLogger.WriteLine(Localization.Instance.HelpOpenFailed + ": " + ex.Message + " " + url);
        }
    }
}
