using System.Collections.Generic;
using System.IO;

namespace GameFrameX.ProtoExport.Persistence;

/// <summary>
/// 把多个 <see cref="MessageInfoList"/> 按 Module 分组，喂给 <see cref="MessageIdAllocator"/>，并落盘 lock。
/// <para>
/// 设计点：所有 proto 文件都已解析完（<c>Opcode == 0</c>，因为 <see cref="MessageHelper.SkipAutoAssignOpcode"/> 已置位），
/// 此刻再做统一分配 —— 跨文件、跨模块按 Module 聚合，新号 = max(existing) + 1，确保同模块的 SubId 全局唯一且稳定。
/// </para>
/// </summary>
public static class MessageIdCoordinator
{
    /// <summary>
    /// 给定一组已解析的 proto 信息列表，按 Module 聚合调用 <see cref="MessageIdAllocator.Mutate"/>，然后把更新后的 lock 写回文件。
    /// </summary>
    /// <param name="lockPath">lock 文件路径。文件不存在则视为空 lock。</param>
    /// <param name="lists">已解析的 proto 信息列表（来自 <see cref="MessageHelper.Parse"/>，此时 Opcode 应全部为 0）。</param>
    /// <returns>本次新增/复用统计，便于日志输出。</returns>
    public static CoordinatorResult AssignAndPersist(string lockPath, IEnumerable<MessageInfoList> lists)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lockPath);
        ArgumentNullException.ThrowIfNull(lists);

        var lockData = MessageIdLockStore.Load(lockPath);

        // 按 Module 分组。Module 相同的 message 共享 SubId 计数空间。
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
                if (info.IsEnum)
                {
                    continue;
                }

                if (!info.IsMessage)
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
            var moduleKeyStr = module.ToString(System.Globalization.CultureInfo.InvariantCulture);
            moduleKey.Add(moduleKeyStr);
            var moduleName = byModule[module].Count > 0
                ? FindModuleName(lists, module)
                : string.Empty;

            var newlyAssigned = MessageIdAllocator.Mutate(lockData, moduleKeyStr, moduleName, messages);
            foreach (var name in newlyAssigned)
            {
                assigned.Add($"{moduleKeyStr}.{name}");
            }
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
}