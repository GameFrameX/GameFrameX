using System.Collections.Generic;
using System.IO;

namespace GameFrameX.ProtoExport.Persistence;

/// <summary>
/// 把当前解析出的 <see cref="MessageInfoList"/> 序列化为「冻结态」<see cref="MessageIdLock"/>：
/// 每个消息的当前 <c>Opcode</c> 直接落进 <see cref="ModuleEntry.Messages"/>，作为后续稳定分配的起点。
/// <para>
/// 用法：迁移脚本一次调用。语义等同 <c>--regenerate-lock</c>。
/// </para>
/// <para>
/// 注意：旧版本（无 lock）下 Opcode 来自行序自增 —— 此举不是「修复历史」，只是「冻结当前」。
/// 历史里已经发生的重排/插入所产生的协议漂移不会因此回滚。
/// </para>
/// </summary>
public static class LockSeedGenerator
{
    /// <summary>
    /// 解析所有 proto，关闭自增，按当前 Opcode 序列化为 lock 写入 <paramref name="lockPath"/>。
    /// </summary>
    public static CoordinatorResult SeedFromCurrentOpcodes(
        string lockPath,
        IEnumerable<MessageInfoList> lists)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lockPath);
        ArgumentNullException.ThrowIfNull(lists);

        var lockData = MessageIdLock.CreateEmpty();

        // 同 Coordinator：按 Module 分组。
        var byModule = new SortedDictionary<short, List<MessageInfo>>();
        foreach (var list in lists)
        {
            if (!byModule.TryGetValue(list.Module, out var bucket))
            {
                bucket = new List<MessageInfo>();
                byModule[list.Module] = bucket;
            }

            foreach (var info in list.Infos)
            {
                if (info.IsEnum || !info.IsMessage)
                {
                    continue;
                }

                bucket.Add(info);
            }
        }

        var moduleKey = new List<string>();
        var assigned = new List<string>();

        foreach (var (module, messages) in byModule)
        {
            var key = module.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var entry = new ModuleEntry
            {
                ModuleName = FindModuleName(lists, module),
            };
            lockData.Modules[key] = entry;

            foreach (var info in messages)
            {
                if (info.Opcode <= 0)
                {
                    throw new InvalidDataException(
                        $"seed 模式下 module={key} 消息 '{info.Name}' 的当前 Opcode={info.Opcode} 不合法，"
                        + "请先跑一次普通导出（不使用 --messageIdLockPath）让行序自增跑完，再做 seed。");
                }

                if (info.Opcode > MessageIdAllocator.MaxSubId)
                {
                    throw new InvalidDataException(
                        $"seed 模式下 module={key} 消息 '{info.Name}' 的 Opcode={info.Opcode} 超过 SubId 上限 {MessageIdAllocator.MaxSubId}");
                }

                entry.Messages[info.Name] = info.Opcode;
                assigned.Add($"{key}.{info.Name}");
            }

            moduleKey.Add(key);
        }

        MessageIdLockStore.Save(lockPath, lockData);

        return new CoordinatorResult(moduleKey.Count, assigned.Count, assigned);
    }

    private static string FindModuleName(IEnumerable<MessageInfoList> lists, short module)
    {
        foreach (var list in lists)
        {
            if (list.Module == module)
            {
                return list.ModuleName;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 把 lock 文件以可读形式打印到 stdout。供 <c>--print-lock</c> 使用。
    /// </summary>
    public static string FormatLockForDisplay(MessageIdLock lockData)
    {
        ArgumentNullException.ThrowIfNull(lockData);
        return MessageIdLockStore.SaveToString(lockData);
    }
}