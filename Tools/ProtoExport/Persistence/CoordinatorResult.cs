using System.Collections.Generic;

namespace GameFrameX.ProtoExport.Persistence;

/// <summary>
/// <see cref="MessageIdCoordinator.AssignAndPersist"/> 与 <see cref="LockSeedGenerator.SeedFromCurrentOpcodes"/> 的统计结果。
/// </summary>
public sealed class CoordinatorResult
{
    /// <summary>
    /// 本次涉及的模块数。
    /// </summary>
    public int ModuleCount { get; }

    /// <summary>
    /// 本次新增 SubId 的消息总数。
    /// </summary>
    public int NewlyAssignedCount { get; }

    /// <summary>
    /// 形如 <c>"&lt;ModuleKey&gt;.&lt;MessageName&gt;"</c> 的新增条目列表。
    /// </summary>
    public IReadOnlyList<string> NewlyAssigned { get; }

    public CoordinatorResult(int moduleCount, int newlyAssignedCount, IReadOnlyList<string> newlyAssigned)
    {
        ModuleCount = moduleCount;
        NewlyAssignedCount = newlyAssignedCount;
        NewlyAssigned = newlyAssigned;
    }
}