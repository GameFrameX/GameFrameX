using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameFrameX.ProtoExport.Persistence;

/// <summary>
/// 协议子 ID 持久化分配表。
/// <para>
/// 设计目标：保证 (Module, Name) → SubId 的映射在 proto 文件被增删/重排时保持稳定。
/// 由 <see cref="MessageIdLockStore"/> 负责落盘，由导出器在 <c>MessageIdHandler</c> 阶段消费。
/// </para>
/// <para>
/// 关键约束：
/// <list type="bullet">
///   <item>已分配的 SubId 一旦写入，永不复用（含模块被删除的情况）。</item>
///   <item>SubId 取值范围 1..65535；0 保留为「未分配」哨兵。</item>
///   <item>每个模块独立计数；跨模块 ID 空间不互通。</item>
///   <item>序列化顺序按 key 字典序固定，便于 PR review 时只看 diff。</item>
/// </list>
/// </para>
/// </summary>
public sealed class MessageIdLock
{
    /// <summary>
    /// lock 文件 schema 版本。当前为 1；解析时若发现不兼容版本应直接报错，避免静默重排。
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; } = 1;

    /// <summary>
    /// 模块号 → 该模块的分配条目。
    /// 键使用 <c>Module</c>（short）字符串形式，避免大整数 JSON key 在不同语言解析上的歧义。
    /// </summary>
    [JsonPropertyName("modules")]
    public SortedDictionary<string, ModuleEntry> Modules { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// 创建一份空 lock。
    /// </summary>
    public static MessageIdLock CreateEmpty()
    {
        return new MessageIdLock();
    }
}

/// <summary>
/// 单模块下的分配条目。
/// </summary>
public sealed class ModuleEntry
{
    /// <summary>
    /// 逻辑模块名（proto <c>package</c> 名）。冗余存储，便于人眼审查。
    /// </summary>
    [JsonPropertyName("moduleName")]
    public string ModuleName { get; set; } = string.Empty;

    /// <summary>
    /// 消息名 → 当前分配的 SubId。键为 <see cref="MessageInfo.Name"/>。
    /// </summary>
    [JsonPropertyName("messages")]
    public SortedDictionary<string, int> Messages { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// 已被删除 / 重命名走新号的消息名，保留其历史 SubId，避免被新消息占用。
    /// key = 历史消息名；value = 历史 SubId。
    /// </summary>
    [JsonPropertyName("retired")]
    public SortedDictionary<string, int> Retired { get; set; } = new(StringComparer.Ordinal);
}