using System.Collections.Generic;

namespace GameFrameX.ProtoExport.Persistence;

/// <summary>
/// 把解析后的 <see cref="MessageInfoList"/> 与 <see cref="MessageIdLock"/> 合并，产出稳定的 <c>Opcode</c>。
/// <para>
/// 决策表（消息 m 在 lock.Modules[M].Messages 中的存在与否）：
/// <list type="bullet">
///   <item>m 存在 → Opcode = lock[M].Messages[m]，沿用历史值；不递增、不重用空号。</item>
///   <item>m 不存在 + lock[M].Messages 非空 → Opcode = max(Messages.Values) + 1。</item>
///   <item>m 不存在 + lock[M].Messages 为空（首次为该模块分配）→ Opcode = <paramref name="firstSubId"/>。</item>
///   <item>lock 里有但当前 proto 缺失的消息 → 移入 lock[M].Retired，永久占用其 SubId。</item>
/// </list>
/// </para>
/// <para>
/// 调用方负责：
/// <list type="bullet">
///   <item>传入已经按 <see cref="MessageInfoList.Module"/> 分组的列表。</item>
///   <item>调用结束后用 <see cref="MessageIdLockStore"/> 落盘（<see cref="Mutate"/> 已经在内存里改完 lock）。</item>
/// </list>
/// </para>
/// </summary>
public static class MessageIdAllocator
{
    /// <summary>
    /// SubId 取值上界。MessageID = (Module &lt;&lt; 16) | SubId，下行 16 位满。
    /// </summary>
    public const int MaxSubId = 0xFFFF;

    /// <summary>
    /// 首次为模块分配时的起始 SubId。沿用 <see cref="MessageHelper.MessageIdHandler"/> 历史约定 10。
    /// </summary>
    public const int FirstSubId = 10;

    /// <summary>
    /// 把 <paramref name="messages"/> 写回 <paramref name="lockData"/>，并把 Opcode 同步到每条 MessageInfo。
    /// </summary>
    /// <param name="lockData">当前 lock（来自文件）。</param>
    /// <param name="moduleKey">模块的 short 字符串形式。</param>
    /// <param name="moduleName">模块的可读名称。</param>
    /// <param name="messages">该模块下当前解析出的所有消息（已经过滤掉 enum）。</param>
    /// <param name="firstSubId">首次为该模块分配时的起点（默认 <see cref="FirstSubId"/>）。</param>
    /// <returns>本次新增 SubId 的消息名列表（用于日志/审计）。</returns>
    /// <exception cref="InvalidDataException">当历史 SubId 越界、或分配溢出时抛出。</exception>
    public static IReadOnlyList<string> Mutate(
        MessageIdLock lockData,
        string moduleKey,
        string moduleName,
        IEnumerable<MessageInfo> messages,
        int firstSubId = FirstSubId)
    {
        ArgumentNullException.ThrowIfNull(lockData);
        ArgumentNullException.ThrowIfNull(moduleKey);
        ArgumentNullException.ThrowIfNull(messages);

        if (!lockData.Modules.TryGetValue(moduleKey, out var entry))
        {
            entry = new ModuleEntry { ModuleName = moduleName };
            lockData.Modules[moduleKey] = entry;
        }
        else
        {
            // 同步 moduleName，便于人在 lock 里阅读；空值不覆盖已有内容。
            if (!string.IsNullOrEmpty(moduleName))
            {
                entry.ModuleName = moduleName;
            }
        }

        var assigned = new List<string>();
        var presentNames = new HashSet<string>(StringComparer.Ordinal);

        // 先用快照遍历，避免在迭代 entry.Messages 时又往里写入。
        var existing = new SortedDictionary<string, int>(entry.Messages, StringComparer.Ordinal);

        // maxExisting 必须从 existing（= entry.Messages 快照）+ Retired 中取最大，
        // Retired 段里的号也属于「永久占用」，不能被新消息撞上。
        int maxExisting = 0;
        foreach (var subId in existing.Values)
        {
            if (subId > maxExisting)
            {
                maxExisting = subId;
            }
        }

        foreach (var subId in entry.Retired.Values)
        {
            if (subId > maxExisting)
            {
                maxExisting = subId;
            }
        }

        var toAssign = new SortedDictionary<string, int>(StringComparer.Ordinal);

        // 既在 Messages 也在 Retired 里视为「已分配」，都允许沿用老号。
        // Retired 里命中后同步把它「恢复」到 Messages（语义：消息再次出现就重新变活跃）。
        foreach (var info in messages)
        {
            if (info.IsEnum)
            {
                continue;
            }

            presentNames.Add(info.Name);

            // 优先看 Retired（消息曾在锁里被删，现又回到 proto），
            // 再看 Messages（消息持续活跃）。两者都视为「已分配，沿用老号」。
            int existingSubId = 0;
            bool found = false;
            if (entry.Retired.TryGetValue(info.Name, out var retiredId))
            {
                existingSubId = retiredId;
                found = true;
            }
            else if (existing.TryGetValue(info.Name, out var activeId))
            {
                existingSubId = activeId;
                found = true;
            }

            if (found)
            {
                if (existingSubId <= 0 || existingSubId > MaxSubId)
                {
                    throw new InvalidDataException(
                        $"lock 中 module={moduleKey} 消息 '{info.Name}' 的历史 SubId={existingSubId} 越界（合法范围 1..{MaxSubId}）");
                }

                info.Opcode = existingSubId;
                if (existingSubId > maxExisting)
                {
                    maxExisting = existingSubId;
                }

                // 同步从 Retired 移除并放回 Messages（如果原本在 Messages，Remove 返回 false，跳过）。
                if (entry.Retired.Remove(info.Name))
                {
                    entry.Messages[info.Name] = existingSubId;
                }

                continue;
            }

            int next = maxExisting == 0 ? firstSubId : maxExisting + 1;
            if (next > MaxSubId)
            {
                throw new InvalidDataException(
                    $"module={moduleKey} 的 SubId 已用尽（>={MaxSubId}），请为该模块申请新的 ModuleID");
            }

            toAssign[info.Name] = next;
            maxExisting = next;
            assigned.Add(info.Name);
        }

        foreach (var kv in toAssign)
        {
            entry.Messages[kv.Key] = kv.Value;
        }

        // 把 SubId 写回每条 MessageInfo.Opcode。沿用分支已在上面直接赋值。
        foreach (var info in messages)
        {
            if (info.IsEnum)
            {
                continue;
            }

            if (toAssign.TryGetValue(info.Name, out var subId))
            {
                info.Opcode = subId;
            }
        }

        // 把当前 proto 里没出现、但 lock 里还有的消息移入 Retired，永久占用其 SubId。
        // 用快照遍历 entry.Messages，避免迭代时又往里写入。
        // 移入 Retired 时同步从 Messages 移除，避免号在同一模块里出现两次造成歧义。
        var keysToRetire = new List<string>();
        foreach (var name in existing.Keys)
        {
            if (!presentNames.Contains(name))
            {
                keysToRetire.Add(name);
            }
        }

        foreach (var name in keysToRetire)
        {
            entry.Retired.TryAdd(name, existing[name]);
            // 旧 client 的包携带历史 Opcode 时，若不小心被新消息同号占用，会被解析成另一条消息。
            // 从 Messages 移出后，号仍然占着（SubId 在 Retired 里），新消息拿不到；但 lock 文件不再有「同号双登记」。
            entry.Messages.Remove(name);
        }

        // 已进入 Retired 的旧名也视为「永久占用」，不再给新消息分配这些号。
        // 这条规则已经在「max+1」路径里隐式满足：retired 的 SubId 已经被 Move 出 Messages 集合，
        // 新消息拿不到。但 retired 段本身仍要在 lock 里保留供审计。

        return assigned;
    }

    /// <summary>
    /// 从 lock 里取一个模块下的全部「已占用 SubId」，含 Messages 与 Retired。
    /// 主要给单元测试做不变式断言用。
    /// </summary>
    public static HashSet<int> OccupiedSubIds(MessageIdLock lockData, string moduleKey)
    {
        var result = new HashSet<int>();
        if (lockData.Modules.TryGetValue(moduleKey, out var entry))
        {
            foreach (var v in entry.Messages.Values)
            {
                result.Add(v);
            }

            foreach (var v in entry.Retired.Values)
            {
                result.Add(v);
            }
        }

        return result;
    }
}