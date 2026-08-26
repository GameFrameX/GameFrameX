using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameFrameX.ProtoExport;

namespace ProtoExporterGUI.Models;

public class SettingData
{
    /// <summary>
    /// System.Text.Json 序列化选项：缩进写入、宽松解析（兼容历史 PascalCase 配置）。
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// 全部内置导出模式的默认配置。作为 source-of-truth，用户配置在加载时按字段深度合并到默认值之上。
    /// 新增模式时在此处登记一条默认值即可，无需改动 UI 或合并逻辑。
    /// </summary>
    Dictionary<string, LauncherOptions> Options { get; set; }

    public SettingData()
    {
        Options = NewDefaultOptions();
    }

    /// <summary>
    /// 构建一份全新的默认模式配置表。覆盖 CLI（ProtoExport）当前支持的全部 7 种模式。
    /// 默认值与 ProtoExport/Properties/launchSettings.json 及 README 文档保持一致。
    /// </summary>
    private static Dictionary<string, LauncherOptions> NewDefaultOptions()
    {
        var options = new Dictionary<string, LauncherOptions>();

        // --- C# 语言族（Server / Unity / Godot） ---
        options.Add("Server", new LauncherOptions
        {
            Mode = ModeType.CSharp.ToString(),
            UsingStatements = "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.NetWork.Abstractions|using GameFrameX.NetWork.Messages",
            IsGenerateDescription = true,
            IsServer = true,
            IsGenerateErrorCode = true,
            NamespaceName = "GameFrameX.Proto.Proto",
            OutputPath = "",
            InputPath = ""
        });
        options.Add("Unity", new LauncherOptions
        {
            Mode = ModeType.CSharp.ToString(),
            UsingStatements = "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.Network.Runtime",
            IsGenerateDescription = false,
            IsServer = false,
            IsGenerateErrorCode = true,
            NamespaceName = "Hotfix.Proto",
            OutputPath = "",
            InputPath = ""
        });
        options.Add("Godot", new LauncherOptions
        {
            Mode = ModeType.CSharp.ToString(),
            UsingStatements = "using System|using ProtoBuf|using System.Collections.Generic|using GameFrameX.Network.Runtime",
            IsGenerateDescription = false,
            IsServer = false,
            IsGenerateErrorCode = true,
            NamespaceName = "Proto",
            OutputPath = "",
            InputPath = ""
        });

        // --- TypeScript 语言族 ---
        options.Add("TypeScript", new LauncherOptions
        {
            Mode = ModeType.TypeScript.ToString(),
            IsGenerateErrorCode = true,
            NamespaceName = "",
            OutputPath = "",
            InputPath = "",
            ImportPath = "../network/",
        });

        // --- C++ 语言族（v0.2.0 起新增，原 GUI 缺失，此处补齐） ---
        options.Add("C++", new LauncherOptions
        {
            Mode = ModeType.Cpp.ToString(),
            IsGenerateErrorCode = true,
            NamespaceName = "GameFrameX.Proto",
            UsingStatements = "#include <cstdint>|#include <string>|#include <vector>|#include <unordered_map>",
            OutputPath = "",
            InputPath = ""
        });

        // --- Lua 语言族（v0.2.0 起新增，原 GUI 缺失，此处补齐） ---
        options.Add("Lua", new LauncherOptions
        {
            Mode = ModeType.Lua.ToString(),
            IsGenerateErrorCode = true,
            NamespaceName = "",
            OutputPath = "",
            InputPath = "",
            ImportPath = "./network/",
        });

        // --- Go 语言族（v0.2.0 起新增，原 GUI 缺失，此处补齐） ---
        options.Add("Go", new LauncherOptions
        {
            Mode = ModeType.Go.ToString(),
            IsGenerateErrorCode = true,
            NamespaceName = "proto",
            UsingStatements = "google.golang.org/protobuf/runtime/protoimpl",
            OutputPath = "",
            InputPath = ""
        });

        return options;
    }

    public static SettingData Instance { get; } = new SettingData();

    /// <summary>
    /// 配置文件路径。锚定到程序基目录（<see cref="AppContext.BaseDirectory"/>），
    /// 避免依赖进程工作目录（CWD）——打包成单文件或从不同目录启动时 CWD 不可预测。
    /// </summary>
    public static readonly string SettingPath = Path.Combine(AppContext.BaseDirectory, "Setting.json");

    public static LauncherOptions GetOptions(string mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            return null;
        }

        return Instance.Options.GetValueOrDefault(mode);
    }

    /// <summary>
    /// 加载用户配置并按字段深度合并到默认值之上。
    ///
    /// 关键修复：旧实现用用户 JSON 整表覆盖默认 Options，一旦用户 Setting.json 是旧版
    /// （缺少新模式条目），切换到新模式会拿到 null 并静默失败。新实现以默认表为基线，
    /// 仅用用户 JSON 中明确出现的字段覆盖对应默认值，未提及的模式/字段保留默认值，
    /// 从而支持从旧配置无损升级。
    /// </summary>
    public static void LoadSetting()
    {
        if (!File.Exists(SettingPath))
        {
            return;
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(File.ReadAllText(SettingPath));
        }
        catch (JsonException)
        {
            // 配置文件损坏时不覆盖默认值，避免破坏工具可用性。
            return;
        }

        if (doc.RootElement.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var entry in doc.RootElement.EnumerateObject())
        {
            var modeKey = entry.Name;
            if (entry.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            // 默认表没有的模式（用户自定义）按全新对象填充。
            if (!Instance.Options.TryGetValue(modeKey, out var target))
            {
                target = new LauncherOptions();
                Instance.Options[modeKey] = target;
            }

            ApplyOverrides(target, entry.Value);
        }
    }

    /// <summary>
    /// 将用户 JSON 元素中的字段逐个覆盖到目标 LauncherOptions。
    /// 合并语义：用户显式提供的值覆盖默认值，未提供（null / 缺失）保留默认。
    /// 这对从 Newtonsoft.Json（默认写出 null）升级来的旧配置至关重要——
    /// 旧 JSON 里未设置的字段会被写成 "X": null，若直接覆盖会清空默认值。
    /// </summary>
    private static void ApplyOverrides(LauncherOptions target, JsonElement user)
    {
        ApplyString(user, nameof(LauncherOptions.InputPath), v => target.InputPath = v);
        ApplyString(user, nameof(LauncherOptions.Mode), v => target.Mode = v);
        ApplyString(user, nameof(LauncherOptions.OutputPath), v => target.OutputPath = v);
        ApplyString(user, nameof(LauncherOptions.NamespaceName), v => target.NamespaceName = v);
        ApplyString(user, nameof(LauncherOptions.UsingStatements), v => target.UsingStatements = v);
        ApplyString(user, nameof(LauncherOptions.ImportPath), v => target.ImportPath = v);
        ApplyString(user, nameof(LauncherOptions.RequireComments), v => target.RequireComments = v);
        ApplyString(user, nameof(LauncherOptions.ErrorCodeExcelFilePath), v => target.ErrorCodeExcelFilePath = v);

        ApplyBool(user, nameof(LauncherOptions.IsGenerateErrorCode), v => target.IsGenerateErrorCode = v);
        ApplyBool(user, nameof(LauncherOptions.IsGenerateErrorCodeExcelFile), v => target.IsGenerateErrorCodeExcelFile = v);
        ApplyBool(user, nameof(LauncherOptions.IsGenerateDescription), v => target.IsGenerateDescription = v);
        ApplyBool(user, nameof(LauncherOptions.IsServer), v => target.IsServer = v);
    }

    /// <summary>
    /// 仅当字段存在且非 null 时覆盖目标字符串属性，避免旧 JSON 的 "X": null 清空默认值。
    /// </summary>
    private static void ApplyString(JsonElement user, string name, Action<string> set)
    {
        if (user.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String)
        {
            set(el.GetString());
        }
    }

    /// <summary>
    /// 仅当 JSON 中明确存在该布尔字段时才覆盖目标，避免 false 默认值误覆盖默认 true 配置。
    /// </summary>
    private static void ApplyBool(JsonElement user, string name, Action<bool> set)
    {
        if (user.TryGetProperty(name, out var el) && (el.ValueKind == JsonValueKind.True || el.ValueKind == JsonValueKind.False))
        {
            set(el.GetBoolean());
        }
    }

    /// <summary>
    /// 持久化全部模式配置。保存完整字段，确保用户对任意参数的修改都能在下次启动时恢复。
    /// </summary>
    public static void SaveSetting()
    {
        File.WriteAllText(SettingPath, JsonSerializer.Serialize(Instance.Options, SettingJsonContext.Default.DictionaryStringLauncherOptions));
    }
}

/// <summary>
/// 编译期 source-generated JSON 序列化上下文，提供 trim/AOT 安全的类型元数据，
/// 避免 PublishTrimmed=true 发布时反射序列化 LauncherOptions 失败（IL2026 警告）。
/// </summary>
[JsonSerializable(typeof(Dictionary<string, LauncherOptions>))]
internal sealed partial class SettingJsonContext : JsonSerializerContext
{
}
