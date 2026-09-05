using System.Globalization;
using System.Resources;

namespace GameFrameX.ProtoExport;

/// <summary>
/// 导出器文案本地化网关。resx 按 <see cref="CultureInfo.CurrentUICulture"/> 自动选择语言：
/// neutral（简体中文）为最终回退，en 为英文。GUI 宿主通过其 Localization.SetCulture
/// 设置线程 culture 后，导出日志与异常文案随之切换；CLI 直接跟随系统语言。
/// </summary>
/// <remarks>
/// 插值参数一律走复合格式（<c>string.Format</c>），resx 值用 {0}/{1} 占位。
/// 两套 resx 的 key 集合一致性由 LocalizationParityTests 守护，新增文案务必同步两个文件。
/// </remarks>
public static class Loc
{
    private static readonly ResourceManager s_manager =
        new ResourceManager("GameFrameX.ProtoExport.Strings", typeof(Loc).Assembly);

    /// <summary>
    /// 取当前 UI 文化的文案。找不到 key 时返回 key 本身（便于发现遗漏的条目）；
    /// key 为 null / 空串时原样返回，不抛异常。
    /// </summary>
    public static string Get(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return key;
        }

        var value = s_manager.GetString(key, CultureInfo.CurrentUICulture);
        return string.IsNullOrEmpty(value) ? key : value;
    }

    // 前置短句日志（异常抛出前的日志区标记）
    public static string Log_PackageNotFound => Get(nameof(Log_PackageNotFound));
    public static string Log_ModuleRangeError => Get(nameof(Log_ModuleRangeError));
    public static string Log_ModuleFileNameFormatError => Get(nameof(Log_ModuleFileNameFormatError));
    public static string Log_ModuleMismatch => Get(nameof(Log_ModuleMismatch));
    public static string Log_ModuleNotFound => Get(nameof(Log_ModuleNotFound));

    // 模块 ID 解析（文件名前缀 / option 声明）
    public static string Err_PackageNotFound => Get(nameof(Err_PackageNotFound));
    public static string Err_ModuleRangeFileName => Get(nameof(Err_ModuleRangeFileName));
    public static string Err_ModuleFileNameFormat => Get(nameof(Err_ModuleFileNameFormat));
    public static string Err_ModuleRangeOption => Get(nameof(Err_ModuleRangeOption));
    public static string Err_ModuleMismatch => Get(nameof(Err_ModuleMismatch));
    public static string Err_ModuleNotFound => Get(nameof(Err_ModuleNotFound));
    public static string Log_PackageModuleLine => Get(nameof(Log_PackageModuleLine));

    // proto 命名与序列校验
    public static string Err_EnumNameNotCamelCase => Get(nameof(Err_EnumNameNotCamelCase));
    public static string Err_EnumFieldNotCamelCase => Get(nameof(Err_EnumFieldNotCamelCase));
    public static string Err_EnumMemberDuplicated => Get(nameof(Err_EnumMemberDuplicated));
    public static string Err_MessageNameNotCamelCase => Get(nameof(Err_MessageNameNotCamelCase));
    public static string Err_MessageMemberDuplicated => Get(nameof(Err_MessageMemberDuplicated));
    public static string Err_MapCommaMissing => Get(nameof(Err_MapCommaMissing));
    public static string Err_FieldNotCamelCase => Get(nameof(Err_FieldNotCamelCase));
    public static string Err_MemberTagExceed => Get(nameof(Err_MemberTagExceed));

    // CLI 入口
    public static string Log_ArgsParseError => Get(nameof(Log_ArgsParseError));
    public static string Log_UnsupportedMode => Get(nameof(Log_UnsupportedMode));
    public static string Log_ExportSuccess => Get(nameof(Log_ExportSuccess));
    public static string Log_ExportFailed => Get(nameof(Log_ExportFailed));
    public static string Err_InputPathNotExist => Get(nameof(Err_InputPathNotExist));

    // 导出流程日志
    public static string Log_SkipServerOnlyFile => Get(nameof(Log_SkipServerOnlyFile));
    public static string Log_SkipInternalModule => Get(nameof(Log_SkipInternalModule));
    public static string Log_LockSummary => Get(nameof(Log_LockSummary));
    public static string Log_LockSeedGenerated => Get(nameof(Log_LockSeedGenerated));
    public static string Log_ScanCompleted => Get(nameof(Log_ScanCompleted));
    public static string Term_ServerMode => Get(nameof(Term_ServerMode));
    public static string Term_ClientMode => Get(nameof(Term_ClientMode));
    public static string Err_UnsupportedModeType => Get(nameof(Err_UnsupportedModeType));

    // lock 持久化
    public static string Err_LockFileParseFailed => Get(nameof(Err_LockFileParseFailed));
    public static string Err_LockFileEmpty => Get(nameof(Err_LockFileEmpty));
    public static string Err_LockSchemaIncompatible => Get(nameof(Err_LockSchemaIncompatible));
    public static string Err_LockModuleKeyInvalid => Get(nameof(Err_LockModuleKeyInvalid));
    public static string Err_LockNullField => Get(nameof(Err_LockNullField));
    public static string Err_LockSubIdOutOfRange => Get(nameof(Err_LockSubIdOutOfRange));
    public static string Err_SubIdExhausted => Get(nameof(Err_SubIdExhausted));
    public static string Err_SeedOpcodeInvalid => Get(nameof(Err_SeedOpcodeInvalid));
    public static string Err_SeedOpcodeExceed => Get(nameof(Err_SeedOpcodeExceed));
    public static string Err_SeedOpcodeDuplicated => Get(nameof(Err_SeedOpcodeDuplicated));

    // 注释校验
    public static string Err_CommentMissingContainer => Get(nameof(Err_CommentMissingContainer));
    public static string Err_CommentMissing => Get(nameof(Err_CommentMissing));
    public static string Err_CommentValidationFailed => Get(nameof(Err_CommentValidationFailed));
}
