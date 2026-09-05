using GameFrameX.ProtoExport;
using Xunit;

namespace ProtoExporterGUI.Tests;

/// <summary>
/// MessageHelper 模块 ID 三态解析的契约：
/// 1) 文件名前缀 _&lt;模块ID&gt;_ 优先，option module 声明兜底；
/// 2) 两者都有必须一致，不一致抛 FormatException（绝不静默取其一）；
/// 3) 文件名以 _+数字 开头但缺第二个下划线（如 _0010Basic）视为命名格式错误；
/// 4) 都没有时报 Module not found；
/// 5) ModuleSource 记录实际来源（FileName / Option）。
/// </summary>
/// <remarks>
/// Parse 内部写 ExportLogger.WriteLine（进程级静态委托），与 ExportLoggerTests 同 collection 串行执行，
/// 避免并行时日志写入对方捕获列表造成交叉失败。
/// </remarks>
[Collection("ExportLogger")]
public class MessageHelperModuleTests
{
    private const string ProtoWithModule10 = @"syntax = ""proto3"";

package Test;

option module = 10;

// 测试请求
message ReqDemo
{
  string Value = 1;
}
";

    /// <summary>
    /// 文件名前缀与 option 一致时，取文件名来源。
    /// </summary>
    [Fact]
    public void FileNamePrefixMatchesOption_UsesFileNameSource()
    {
        var info = MessageHelper.Parse(ProtoWithModule10, "_0010_Basic", "out", false);

        Assert.Equal(10, info.Module);
        Assert.Equal(MessageInfoList.ModuleSourceKind.FileName, info.ModuleSource);
        // module 解析不影响消息解析主流程
        Assert.Single(info.Infos);
    }

    /// <summary>
    /// 带路径与扩展名的文件名，仍取前缀。
    /// </summary>
    [Fact]
    public void FileNameWithPathAndExtension_StillUsesPrefix()
    {
        var info = MessageHelper.Parse(ProtoWithModule10, "Protobuf/_0010_Basic.proto", "out", false);

        Assert.Equal(10, info.Module);
        Assert.Equal(MessageInfoList.ModuleSourceKind.FileName, info.ModuleSource);
    }

    /// <summary>
    /// 仅文件名前缀、省略 option 时，用文件名。
    /// </summary>
    [Fact]
    public void FileNamePrefixOnly_OptionOmitted_UsesFileName()
    {
        var proto = ProtoWithModule10.Replace("option module = 10;", string.Empty);

        var info = MessageHelper.Parse(proto, "_0010_Basic", "out", false);

        Assert.Equal(10, info.Module);
        Assert.Equal(MessageInfoList.ModuleSourceKind.FileName, info.ModuleSource);
    }

    /// <summary>
    /// 文件名前缀与 option 不一致时，报错并携带两个值。
    /// </summary>
    [Fact]
    public void FileNamePrefixMismatchesOption_ThrowsWithBothValues()
    {
        var proto = ProtoWithModule10.Replace("option module = 10;", "option module = 20;");

        var ex = Assert.Throws<FormatException>(() => MessageHelper.Parse(proto, "_0010_Basic", "out", false));

        // 断言取资源文案本体（与被测方同 culture，自洽），数字/文件名为字面量、语言无关
        Assert.Equal(string.Format(Loc.Err_ModuleMismatch, "_0010_Basic", 10, 20), ex.Message);
        Assert.Contains("_0010_Basic", ex.Message);
    }

    /// <summary>
    /// 无前缀文件名，用 option 兜底。
    /// </summary>
    [Fact]
    public void NoPrefixFileName_FallsBackToOption()
    {
        var info = MessageHelper.Parse(ProtoWithModule10, "Basic", "out", false);

        Assert.Equal(10, info.Module);
        Assert.Equal(MessageInfoList.ModuleSourceKind.Option, info.ModuleSource);
    }

    /// <summary>
    /// 无前缀且无 option 时，报 ModuleNotFound。
    /// </summary>
    [Fact]
    public void NoPrefixAndNoOption_ThrowsModuleNotFound()
    {
        var proto = ProtoWithModule10.Replace("option module = 10;", string.Empty);

        var ex = Assert.Throws<Exception>(() => MessageHelper.Parse(proto, "Basic", "out", false));

        Assert.Equal(Loc.Err_ModuleNotFound, ex.Message);
    }

    /// <summary>
    /// 缺第二个下划线的疑似前缀，报格式错误。
    /// </summary>
    [Fact]
    public void PrefixLikeNameMissingSecondUnderscore_ThrowsFormatError()
    {
        var ex = Assert.Throws<FormatException>(() => MessageHelper.Parse(ProtoWithModule10, "_0010Basic", "out", false));

        Assert.Equal(string.Format(Loc.Err_ModuleFileNameFormat, "_0010Basic"), ex.Message);
    }

    /// <summary>
    /// 连字符作分隔符时，同样取前缀。
    /// </summary>
    [Fact]
    public void HyphenAsSeparator_StillUsesPrefix()
    {
        var info = MessageHelper.Parse(ProtoWithModule10, "_0010-Basic", "out", false);

        Assert.Equal(10, info.Module);
        Assert.Equal(MessageInfoList.ModuleSourceKind.FileName, info.ModuleSource);
    }

    /// <summary>
    /// 负数前缀，剥离前导零得到负模块。
    /// </summary>
    [Fact]
    public void NegativePrefix_StripsLeadingZerosToNegativeModule()
    {
        var proto = ProtoWithModule10.Replace("option module = 10;", "option module = -120;");

        var info = MessageHelper.Parse(proto, "_-0120_Inner_Social", "out", false);

        Assert.Equal(-120, info.Module);
        Assert.Equal(MessageInfoList.ModuleSourceKind.FileName, info.ModuleSource);
    }

    /// <summary>
    /// 文件名模块超出 short 范围时，报错。
    /// </summary>
    [Fact]
    public void FileNameModuleExceedsShortRange_Throws()
    {
        Assert.Throws<FormatException>(() => MessageHelper.Parse(ProtoWithModule10, "_99999_Overflow", "out", false));
    }

    /// <summary>
    /// option 模块超出 short 范围时，报错。
    /// </summary>
    [Fact]
    public void OptionModuleExceedsShortRange_Throws()
    {
        var proto = ProtoWithModule10.Replace("option module = 10;", "option module = 99999;");

        Assert.Throws<FormatException>(() => MessageHelper.Parse(proto, "Basic", "out", false));
    }

    /// <summary>
    /// 缺 package 声明时报 PackageNotFound（该分支先输出日志再抛异常）。
    /// </summary>
    [Fact]
    public void MissingPackageDeclaration_ThrowsPackageNotFound()
    {
        var proto = ProtoWithModule10.Replace("package Test;", string.Empty);

        var ex = Assert.Throws<Exception>(() => MessageHelper.Parse(proto, "Basic", "out", false));

        Assert.Equal(string.Format(Loc.Err_PackageNotFound, "Basic"), ex.Message);
    }

    /// <summary>
    /// 模块号 0 是合法 short，照常解析（不能把 0 当哨兵拒绝）。
    /// </summary>
    [Fact]
    public void ModuleZero_ParsesNormally()
    {
        var proto = ProtoWithModule10.Replace("option module = 10;", string.Empty);

        var info = MessageHelper.Parse(proto, "_0_Basic", "out", false);

        Assert.Equal(0, info.Module);
        Assert.Equal(MessageInfoList.ModuleSourceKind.FileName, info.ModuleSource);
    }

    /// <summary>
    /// short 边界值 32767 / -32768 合法：上限下界的模块号照常解析，不报范围错。
    /// </summary>
    [Fact]
    public void ShortBoundaryValues_ParseSuccessfully()
    {
        var proto = ProtoWithModule10.Replace("option module = 10;", string.Empty);
        var fromFileName = MessageHelper.Parse(proto, "_32767_Basic", "out", false);
        Assert.Equal(short.MaxValue, fromFileName.Module);

        var protoOption = ProtoWithModule10.Replace("option module = 10;", "option module = -32768;");
        var fromOption = MessageHelper.Parse(protoOption, "Basic", "out", false);
        Assert.Equal(short.MinValue, fromOption.Module);
        Assert.Equal(MessageInfoList.ModuleSourceKind.Option, fromOption.ModuleSource);
    }

    /// <summary>
    /// option 等号两侧必须有空格（行为固化）：module 声明模式要求 "module = " 精确空格，
    /// "option module=10;" 不匹配，落入 ModuleNotFound 而非解析成功。
    /// </summary>
    [Fact]
    public void OptionWithoutSpacesAroundEquals_NotMatched()
    {
        var proto = ProtoWithModule10.Replace("option module = 10;", "option module=10;");

        var ex = Assert.Throws<Exception>(() => MessageHelper.Parse(proto, "Basic", "out", false));

        Assert.Equal(Loc.Err_ModuleNotFound, ex.Message);
    }

    /// <summary>
    /// 多条 option module 声明时取第一条（Regex.Match 语义固化）。
    /// </summary>
    [Fact]
    public void MultipleOptionDeclarations_FirstOneWins()
    {
        var proto = ProtoWithModule10.Replace("option module = 10;", "option module = 10;\noption module = 20;");

        var info = MessageHelper.Parse(proto, "Basic", "out", false);

        Assert.Equal(10, info.Module);
    }

    /// <summary>
    /// 文件名含中文与空格的路径，仍正确剥离目录与扩展名后取前缀。
    /// </summary>
    [Fact]
    public void FileNameWithChineseAndSpaces_StillUsesPrefix()
    {
        var info = MessageHelper.Parse(ProtoWithModule10, "我的 Protobuf 目录/_0010_基础.proto", "out", false);

        Assert.Equal(10, info.Module);
        Assert.Equal(MessageInfoList.ModuleSourceKind.FileName, info.ModuleSource);
    }
}
