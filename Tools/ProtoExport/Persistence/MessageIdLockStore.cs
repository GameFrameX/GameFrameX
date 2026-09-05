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
            throw new InvalidDataException(string.Format(Loc.Err_LockFileParseFailed, path, ex.Message), ex);
        }

        if (lockData == null)
        {
            throw new InvalidDataException(string.Format(Loc.Err_LockFileEmpty, path));
        }

        if (lockData.SchemaVersion != CurrentSchemaVersion)
        {
            throw new InvalidDataException(
                string.Format(Loc.Err_LockSchemaIncompatible, path, lockData.SchemaVersion, CurrentSchemaVersion));
        }

        // modules / messages / retired 显式写 null 会覆盖属性默认值（手改 lock 的典型损坏形态），
        // 此处统一拦截为受控异常，避免后续分配链路 NRE。
        if (lockData.Modules == null)
        {
            throw new InvalidDataException(string.Format(Loc.Err_LockNullField, path, "modules"));
        }

        // 校验 module key 必须落在 short 范围内，避免后续位运算溢出。
        foreach (var kv in lockData.Modules)
        {
            if (!short.TryParse(kv.Key, out _))
            {
                throw new InvalidDataException(string.Format(Loc.Err_LockModuleKeyInvalid, path, kv.Key));
            }

            if (kv.Value == null)
            {
                throw new InvalidDataException(string.Format(Loc.Err_LockNullField, path, $"modules/{kv.Key}"));
            }

            if (kv.Value.Messages == null)
            {
                throw new InvalidDataException(string.Format(Loc.Err_LockNullField, path, $"modules/{kv.Key}/messages"));
            }

            if (kv.Value.Retired == null)
            {
                throw new InvalidDataException(string.Format(Loc.Err_LockNullField, path, $"modules/{kv.Key}/retired"));
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

        // 临时文件名带随机段：多个进程/线程同时 Save 同一目标时互不覆盖对方的临时文件。
        // 收尾统一用带覆盖的原子 Move（POSIX rename / Win32 MoveFileEx），消除 Exists 检查的 TOCTOU 窗口。
        var tmp = path + "." + Path.GetRandomFileName() + ".tmp";
        try
        {
            File.WriteAllText(tmp, Serialize(lockData));
            File.Move(tmp, path, true);
        }
        catch
        {
            try
            {
                File.Delete(tmp);
            }
            catch
            {
                // 清理失败不掩盖原始异常
            }

            throw;
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