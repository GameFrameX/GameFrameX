using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameFrameX.ProtoExport.Persistence;

/// <summary>
/// 协议子 ID lock 文件的落盘与解析。
/// <para>
/// 设计边界：本类只负责 IO + JSON 校验；分配决策（沿用 vs. 新增 vs. 墓碑）由调用方根据 <see cref="MessageIdLock"/> 自行计算。
/// 这样能让 MessageIdLock 保持纯数据形态，便于单测覆盖。
/// </para>
/// </summary>
public static class MessageIdLockStore
{
    /// <summary>
    /// lock 文件 schema 版本号。
    /// </summary>
    public const int CurrentSchemaVersion = 1;

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// 从指定路径读取 lock 文件。若文件不存在则返回空 lock（不抛异常）。
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// 抛出当文件存在但内容无法解析、<see cref="MessageIdLock.SchemaVersion"/> 不兼容、或 module key 非 short 范围时。
    /// </exception>
    public static MessageIdLock Load(string path)
    {
        if (!File.Exists(path))
        {
            return MessageIdLock.CreateEmpty();
        }

        string text = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(text))
        {
            return MessageIdLock.CreateEmpty();
        }

        MessageIdLock lockData;
        try
        {
            lockData = JsonSerializer.Deserialize<MessageIdLock>(text, s_jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"lock 文件 {path} 解析失败：{ex.Message}", ex);
        }

        if (lockData == null)
        {
            throw new InvalidDataException($"lock 文件 {path} 解析结果为空");
        }

        if (lockData.SchemaVersion != CurrentSchemaVersion)
        {
            throw new InvalidDataException(
                $"lock 文件 {path} 的 schemaVersion={lockData.SchemaVersion} 与当前支持版本 {CurrentSchemaVersion} 不兼容，"
                + "请按迁移说明手动升级或删除该文件后重新生成。");
        }

        // 校验 module key 必须落在 short 范围内，避免后续位运算溢出。
        foreach (var key in lockData.Modules.Keys)
        {
            if (!short.TryParse(key, out _))
            {
                throw new InvalidDataException($"lock 文件 {path} 中 module key '{key}' 不是合法的 short 范围");
            }
        }

        return lockData;
    }

    /// <summary>
    /// 将 lock 写入指定路径，原子覆盖（先写 .tmp 再 rename）。
    /// </summary>
    public static void Save(string path, MessageIdLock lockData)
    {
        ArgumentNullException.ThrowIfNull(lockData);

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var tmp = path + ".tmp";
        File.WriteAllText(tmp, Serialize(lockData));

        if (File.Exists(path))
        {
            File.Replace(tmp, path, null);
        }
        else
        {
            File.Move(tmp, path);
        }
    }

    /// <summary>
    /// 把 lock 序列化为 JSON 字符串，便于测试做字节级断言或 PR diff。
    /// </summary>
    public static string SaveToString(MessageIdLock lockData)
    {
        ArgumentNullException.ThrowIfNull(lockData);
        return Serialize(lockData);
    }

    private static string Serialize(MessageIdLock lockData)
    {
        return JsonSerializer.Serialize(lockData, s_jsonOptions);
    }
}