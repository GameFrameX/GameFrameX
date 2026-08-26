using GameFrameX.Foundation.Options.Attributes;

namespace GameFrameX.ProtoExport;

public sealed class LauncherOptions
{
    /// <summary>
    /// 协议文件路径
    /// </summary>
    [Option("inputPath", Required = true, Description = "协议文件路径")]
    public string InputPath { get; set; }

    /// <summary>
    /// 运行模式
    /// </summary>
    [Option("mode", Required = true, Description = "运行模式")]
    public string Mode { get; set; }

    /// <summary>
    /// 文件路径
    /// </summary>
    [Option("outputPath", Required = true, Description = "文件路径")]
    public string OutputPath { get; set; }

    /// <summary>
    /// 命名空间
    /// </summary>
    [Option("namespaceName", Required = false, DefaultValue = "", Description = "命名空间")]
    public string NamespaceName { get; set; }

    /// <summary>
    /// 是否生成错误码
    /// </summary>
    [Option("isGenerateErrorCode", Required = false, DefaultValue = true, Description = "是否生成错误码")]
    public bool IsGenerateErrorCode { get; set; }

    /// <summary>
    /// 是否生成错误码Excel文件
    /// </summary>
    [Option("isGenerateErrorCodeExcelFile", Required = false, DefaultValue = true, Description = "是否生成错误码Excel文件")]
    public bool IsGenerateErrorCodeExcelFile { get; set; }

    /// <summary>
    /// 错误码Excel文件路径
    /// </summary>
    [Option("errorCodeExcelFilePath", Required = false, Description = "错误码Excel文件路径")]
    public string ErrorCodeExcelFilePath { get; set; }

    /// <summary>
    /// using 语句，使用 | 分隔
    /// </summary>
    [Option("usingStatements", Required = false, DefaultValue = "", Description = "using 语句，使用 | 分隔")]
    public string UsingStatements { get; set; }

    /// <summary>
    /// 是否生成 Description 特性
    /// </summary>
    [Option("isGenerateDescription", Required = false, DefaultValue = false, Description = "是否生成 Description 特性")]
    public bool IsGenerateDescription { get; set; }

    /// <summary>
    /// 是否为服务器模式（包含 _s/-s 后缀的服务器内部协议文件）
    /// </summary>
    [Option("isServer", Required = false, DefaultValue = false, Description = "是否为服务器模式")]
    public bool IsServer { get; set; }

    /// <summary>
    /// TypeScript import 路径前缀
    /// </summary>
    [Option("importPath", Required = false, DefaultValue = "../network/", Description = "TypeScript import 路径前缀")]
    public string ImportPath { get; set; }

    /// <summary>
    /// 注释校验级别: none(不校验) | container(类型级) | member(成员级) | all(全部)
    /// </summary>
    [Option("requireComments", Required = false, DefaultValue = "none", Description = "注释校验级别: none(不校验) | container(类型级) | member(成员级) | all(全部)")]
    public string RequireComments { get; set; }

    /// <summary>
    /// 子 ID 持久化 lock 文件路径。空字符串表示禁用 lock 模式（保留旧的自增分配行为，向后兼容）。
    /// </summary>
    [Option("messageIdLockPath", Required = false, DefaultValue = "", Description = "子 ID 持久化 lock 文件路径，留空则禁用 lock 模式（向后兼容旧行为）")]
    public string MessageIdLockPath { get; set; }

    /// <summary>
    /// 解析后的注释校验级别
    /// </summary>
    public CommentValidationLevel CommentValidation
    {
        get
        {
            if (Enum.TryParse<CommentValidationLevel>(RequireComments, true, out var level))
            {
                return level;
            }

            return CommentValidationLevel.None;
        }
    }
}