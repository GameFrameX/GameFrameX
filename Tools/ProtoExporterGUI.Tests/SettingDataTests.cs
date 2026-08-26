using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using GameFrameX.ProtoExport;
using ProtoExporterGUI.Models;
using Xunit;

namespace ProtoExporterGUI.Tests;

/// <summary>
/// SettingData 深度合并逻辑单元测试。
/// </summary>
/// <remarks>
/// <para>
/// 测试隔离策略：SettingData.Instance 是进程级静态单例，Options 为私有字典；
/// LoadSetting 按"字段覆盖"语义把用户 JSON 合并到该共享字典上（不重置未提及的字段）。
/// 因此测试间存在状态泄漏，必须在每个测试前后做两件事：
/// </para>
/// <list type="number">
///   <item>重置 Instance.Options 回全新默认表（通过反射替换私有 Options 字典为 new SettingData() 的产物）。</item>
///   <item>清理 SettingPath 指向的配置文件。SettingPath 为 static readonly，锚定 AppContext.BaseDirectory
///         （即测试运行目录），无法改写路径，故直接读写该文件。</item>
/// </list>
/// <para>
/// 不直接 new SettingData().LoadSetting()，因为 LoadSetting/SaveSetting 都操作静态 Instance；
/// 单例替换走反射是当前 API 下唯一可靠的隔离手段。
/// </para>
/// </remarks>
public class SettingDataTests : IDisposable
{
    public SettingDataTests()
    {
        ResetInstanceToDefaults();
        CleanSettingFile();
    }

    public void Dispose()
    {
        CleanSettingFile();
        ResetInstanceToDefaults();
    }

    // ---- 测试辅助 ----

    /// <summary>
    /// 把共享单例的私有 Options 字典替换为全新默认表，等价于重置单例。
    /// Options 是普通 auto-property（非 initonly），其 setter 通过反射写入完全合法；
    /// new SettingData() 的构造函数已用 NewDefaultOptions() 填充 Options，取出注入即可。
    /// </summary>
    private static void ResetInstanceToDefaults()
    {
        var instance = typeof(SettingData)
            .GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)!
            .GetValue(null);

        var optionsProp = typeof(SettingData)
            .GetProperty("Options", BindingFlags.NonPublic | BindingFlags.Instance)!;

        var freshOptions = optionsProp.GetValue(new SettingData());
        optionsProp.SetValue(instance, freshOptions);
    }

    private static void CleanSettingFile()
    {
        if (File.Exists(SettingData.SettingPath))
        {
            File.Delete(SettingData.SettingPath);
        }
    }

    private static void WriteSettingJson(string json)
        => File.WriteAllText(SettingData.SettingPath, json);

    // ---- 测试用例 ----

    /// <summary>
    /// 配置文件不存在时，LoadSetting 必须原样保留全部默认值（含 v0.2.0 新增模式）。
    /// </summary>
    [Fact]
    public void LoadSetting_NoFile_保留全部默认值()
    {
        // 不写 Setting.json
        SettingData.LoadSetting();

        var server = SettingData.GetOptions("Server");
        Assert.NotNull(server);
        Assert.Equal("CSharp", server.Mode);
        Assert.True(server.IsServer);
        Assert.True(server.IsGenerateErrorCode);
        Assert.True(server.IsGenerateDescription);
        Assert.Equal("GameFrameX.Proto.Proto", server.NamespaceName);
        Assert.Equal("", server.InputPath);

        // v0.2.0 新增模式必须存在（旧版 GUI 缺失这些条目，此处为回归保护）
        Assert.NotNull(SettingData.GetOptions("Unity"));
        Assert.NotNull(SettingData.GetOptions("Godot"));
        Assert.NotNull(SettingData.GetOptions("TypeScript"));
        Assert.NotNull(SettingData.GetOptions("C++"));
        Assert.NotNull(SettingData.GetOptions("Lua"));
        Assert.NotNull(SettingData.GetOptions("Go"));

        Assert.Equal("CSharp", SettingData.GetOptions("Unity").Mode);
        Assert.Equal("CSharp", SettingData.GetOptions("Godot").Mode);
        Assert.Equal("TypeScript", SettingData.GetOptions("TypeScript").Mode);
        Assert.Equal("Cpp", SettingData.GetOptions("C++").Mode);
        Assert.Equal("Lua", SettingData.GetOptions("Lua").Mode);
        Assert.Equal("Go", SettingData.GetOptions("Go").Mode);
    }

    /// <summary>
    /// 用户仅覆盖部分字段时：覆盖字段用新值，未提及字段保留默认，未出现在 JSON 的新模式仍然存在。
    /// </summary>
    [Fact]
    public void LoadSetting_用户覆盖部分字段_其余保留默认()
    {
        WriteSettingJson("""
        {
          "Server": { "InputPath": "/proto/server" },
          "Unity": { "NamespaceName": "MyGame.Proto" }
        }
        """);

        SettingData.LoadSetting();

        var server = SettingData.GetOptions("Server");
        Assert.Equal("/proto/server", server.InputPath);               // 覆盖生效
        Assert.Equal("CSharp", server.Mode);                           // 未提及，保留默认
        Assert.True(server.IsServer);                                  // 未提及，保留默认
        Assert.True(server.IsGenerateErrorCode);                       // 未提及，保留默认
        Assert.Equal("GameFrameX.Proto.Proto", server.NamespaceName);  // 未提及，保留默认

        var unity = SettingData.GetOptions("Unity");
        Assert.Equal("MyGame.Proto", unity.NamespaceName);             // 覆盖生效
        Assert.Equal("CSharp", unity.Mode);                            // 未提及，保留默认
        Assert.False(unity.IsServer);                                  // 默认 false

        // 未在 JSON 出现的新模式必须仍在（默认表未被整表替换清空）
        var cpp = SettingData.GetOptions("C++");
        Assert.NotNull(cpp);
        Assert.Equal("Cpp", cpp.Mode);
        Assert.NotNull(SettingData.GetOptions("Go"));
        Assert.NotNull(SettingData.GetOptions("Lua"));
    }

    /// <summary>
    /// P0 回归：旧 Newtonsoft.Json 默认写出 null，旧实现整表覆盖会用 null 清空默认值。
    /// 新实现必须跳过 null/缺失字段，保留默认值。
    /// </summary>
    [Fact]
    public void LoadSetting_null字段不清空默认()
    {
        // 模拟旧版 Newtonsoft 写出的配置：带非空默认值的字段被显式写成 null
        WriteSettingJson("""
        {
          "Server": {
            "InputPath": null,
            "NamespaceName": null,
            "Mode": null,
            "UsingStatements": null,
            "IsServer": null,
            "IsGenerateErrorCode": null
          }
        }
        """);

        SettingData.LoadSetting();

        var server = SettingData.GetOptions("Server");
        Assert.NotNull(server);
        Assert.Equal("", server.InputPath);                            // 默认 ""，未被 null 清空
        Assert.Equal("GameFrameX.Proto.Proto", server.NamespaceName);  // 默认值保留
        Assert.Equal("CSharp", server.Mode);                           // 默认值保留
        Assert.NotNull(server.UsingStatements);                        // 非空默认保留
        Assert.True(server.IsServer);                                  // 默认 true 保留
        Assert.True(server.IsGenerateErrorCode);                       // 默认 true 保留
    }

    /// <summary>
    /// 配置文件损坏（非法 JSON）时，LoadSetting 必须不抛异常并保留默认值。
    /// </summary>
    [Fact]
    public void LoadSetting_损坏JSON不崩溃_保留默认()
    {
        WriteSettingJson("{ this is : not valid json ][");

        var ex = Record.Exception(() => SettingData.LoadSetting());
        Assert.Null(ex);

        // 默认值应完整保留
        var server = SettingData.GetOptions("Server");
        Assert.NotNull(server);
        Assert.Equal("GameFrameX.Proto.Proto", server.NamespaceName);
        Assert.Equal("CSharp", server.Mode);
        Assert.NotNull(SettingData.GetOptions("Go"));
        Assert.NotNull(SettingData.GetOptions("C++"));
    }

    /// <summary>
    /// SaveSetting 产出完整 JSON，重置单例后 LoadSetting 应还原全部字段（含默认值，往返一致）。
    /// </summary>
    [Fact]
    public void SaveLoad_往返一致()
    {
        // 先通过部分 JSON 注入覆盖（避免直接访问私有 Options）
        WriteSettingJson("""
        {
          "Server": { "InputPath": "/in/server", "OutputPath": "/out/server", "NamespaceName": "RoundTrip.Server" },
          "Lua": { "ImportPath": "./proto-net/" }
        }
        """);
        SettingData.LoadSetting();

        // 持久化全部 7 个模式的完整字段
        SettingData.SaveSetting();
        Assert.True(File.Exists(SettingData.SettingPath));

        // 序列化产物必须是可解析的合法 JSON 且包含新模式
        var savedText = File.ReadAllText(SettingData.SettingPath);
        using (var doc = JsonDocument.Parse(savedText))
        {
            Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
            Assert.True(doc.RootElement.TryGetProperty("Server", out _));
            Assert.True(doc.RootElement.TryGetProperty("C++", out _));
            Assert.True(doc.RootElement.TryGetProperty("Go", out _));
        }

        // 重置单例后重新加载，验证往返一致
        ResetInstanceToDefaults();
        SettingData.LoadSetting();

        var server = SettingData.GetOptions("Server");
        Assert.Equal("/in/server", server.InputPath);              // 覆盖往返保留
        Assert.Equal("/out/server", server.OutputPath);            // 覆盖往返保留
        Assert.Equal("RoundTrip.Server", server.NamespaceName);    // 覆盖往返保留
        Assert.Equal("CSharp", server.Mode);                       // 默认字段未被抹掉
        Assert.True(server.IsServer);                              // 默认 bool 往返保留

        var lua = SettingData.GetOptions("Lua");
        Assert.Equal("./proto-net/", lua.ImportPath);              // 覆盖生效
        Assert.Equal("Lua", lua.Mode);                             // 默认保留

        // 默认未触碰的模式经往返后仍完整存在
        Assert.NotNull(SettingData.GetOptions("C++"));
        Assert.NotNull(SettingData.GetOptions("Go"));
        Assert.NotNull(SettingData.GetOptions("TypeScript"));
    }
}
