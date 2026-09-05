using System.Collections.Generic;
using System.IO;
using GameFrameX.ProtoExport;
using GameFrameX.ProtoExport.Persistence;
using Xunit;

namespace ProtoExporterGUI.Tests;

/// <summary>
/// 集成测试：跨多 proto 文件（多个 MessageInfoList）+ 同一 Module 聚合，
/// 验证 MessageIdCoordinator 把 SubId 分配 + 落盘联起来的行为。
/// </summary>
public class MessageIdCoordinatorTests
{
    private static MessageInfoList NewList(short module, string moduleName, params string[] messageNames)
    {
        var list = new MessageInfoList
        {
            Module = module,
            ModuleName = moduleName,
            FileName = moduleName,
        };

        foreach (var n in messageNames)
        {
            list.Infos.Add(new MessageInfo { Name = n, Opcode = 0 });
        }

        return list;
    }

    /// <summary>
    /// 首次落锁_按模块顺序分配：首次落 lock 时每个模块从 10 起，按 MessageInfo 列表出现顺序递增。
    /// </summary>
    [Fact]
    public void FirstLockAssignment_AllocatesInModuleOrder()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");

        try
        {
            var player = NewList(1, "Player", "ReqLogin", "RespLogin");
            var bag = NewList(2, "Bag", "ReqOpen");
            var coordinatorResult = MessageIdCoordinator.AssignAndPersist(path, new[] { player, bag });

            Assert.Equal(2, coordinatorResult.ModuleCount);
            Assert.Equal(3, coordinatorResult.NewlyAssignedCount);

            Assert.Equal(10, player.Infos[0].Opcode);
            Assert.Equal(11, player.Infos[1].Opcode);
            Assert.Equal(10, bag.Infos[0].Opcode);

            var reloaded = MessageIdLockStore.Load(path);
            Assert.Equal(10, reloaded.Modules["1"].Messages["ReqLogin"]);
            Assert.Equal(11, reloaded.Modules["1"].Messages["RespLogin"]);
            Assert.Equal(10, reloaded.Modules["2"].Messages["ReqOpen"]);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    /// <summary>
    /// 同一模块跨多列表_续号：同模块跨多个 MessageInfoList 时，新号必须接着历史最大号续，不按文件重启。
    /// </summary>
    [Fact]
    public void SameModuleAcrossLists_ContinuesSubIds()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");

        try
        {
            // 提前落一份带历史的 lock：Messages 表达「当前 proto 包含 + 已分配号」，
            // Retired 表达「历史保留号但当前 proto 不再使用」。这样新设计下 Messages 与
            // Retired 不会出现同号双登记。
            var seed = MessageIdLock.CreateEmpty();
            var playerEntry = new ModuleEntry { ModuleName = "Player" };
            // 已废弃的 ReqEnter 占号 12，永不回收
            playerEntry.Retired.Add("ReqEnter", 12);
            seed.Modules["1"] = playerEntry;
            MessageIdLockStore.Save(path, seed);

            // 第一个文件带 ReqLogin / RespLogin（首次为模块分配，max=Retired.Max=12 → 拿到 13、14）
            var listA = NewList(1, "Player", "ReqLogin", "RespLogin");
            MessageIdCoordinator.AssignAndPersist(path, new[] { listA });
            // 第二个文件新增 ReqLogout —— 必须拿到 15（max(Messages=14)+1，跳过 Retired 里的 12）
            var listB = NewList(1, "Player", "ReqLogin", "RespLogin", "ReqLogout");
            var coordinatorResult = MessageIdCoordinator.AssignAndPersist(path, new[] { listB });

            Assert.Equal(1, coordinatorResult.ModuleCount);
            Assert.Single(coordinatorResult.NewlyAssigned);
            Assert.Equal("1.ReqLogout", coordinatorResult.NewlyAssigned[0]);
            Assert.Equal(15, listB.Infos[2].Opcode);

            // 老号纹丝不动
            var reloaded = MessageIdLockStore.Load(path);
            Assert.Equal(13, reloaded.Modules["1"].Messages["ReqLogin"]);
            Assert.Equal(14, reloaded.Modules["1"].Messages["RespLogin"]);
            Assert.Equal(15, reloaded.Modules["1"].Messages["ReqLogout"]);
            Assert.Equal(12, reloaded.Modules["1"].Retired["ReqEnter"]);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    /// <summary>
    /// 跨文件重排与插入_锁稳定：跨「行序变化 + 同模块多文件」综合场景，
    /// 模拟「文件 A 末尾插一条新消息 + 文件 B 重排」，走两轮后必须锁内稳定。
    /// </summary>
    [Fact]
    public void CrossFileReorderAndInsert_LockStaysStable()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");

        try
        {
            // 第一轮：文件 A 两个消息，文件 C 一个
            var a1 = NewList(1, "PlayerA", "ReqA1", "ReqA2");
            var c1 = NewList(1, "PlayerC", "ReqC1");
            MessageIdCoordinator.AssignAndPersist(path, new[] { a1, c1 });

            // ReqA1=10, ReqA2=11, ReqC1=12
            Assert.Equal(10, a1.Infos[0].Opcode);
            Assert.Equal(11, a1.Infos[1].Opcode);
            Assert.Equal(12, c1.Infos[0].Opcode);

            // 第二轮：A 文件末尾插入 ReqA3；C 文件整体前置
            var a2 = NewList(1, "PlayerA", "ReqA1", "ReqA2", "ReqA3");
            var c2 = NewList(1, "PlayerC", "ReqC1");
            MessageIdCoordinator.AssignAndPersist(path, new[] { c2, a2 });

            // 老号不变，新增 ReqA3 = 13
            Assert.Equal(10, a2.Infos[0].Opcode);
            Assert.Equal(11, a2.Infos[1].Opcode);
            Assert.Equal(12, c2.Infos[0].Opcode);
            Assert.Equal(13, a2.Infos[2].Opcode);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    /// <summary>
    /// 删除消息_进入Retired_永不回收：同一模块下某文件整条消息被删，应进 Retired，永不回收。
    /// </summary>
    [Fact]
    public void DeleteMessage_MovesToRetired_NeverReused()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");

        try
        {
            var a = NewList(1, "Player", "ReqA", "ReqB", "ReqC");
            MessageIdCoordinator.AssignAndPersist(path, new[] { a });
            // ReqA=10, ReqB=11, ReqC=12

            // 第二轮删掉 ReqB，加新消息 ReqD
            var a2 = NewList(1, "Player", "ReqA", "ReqC", "ReqD");
            MessageIdCoordinator.AssignAndPersist(path, new[] { a2 });

            Assert.Equal(13, a2.Infos[2].Opcode); // ReqD 跳过 11

            var reloaded = MessageIdLockStore.Load(path);
            Assert.True(reloaded.Modules["1"].Retired.ContainsKey("ReqB"));
            Assert.Equal(11, reloaded.Modules["1"].Retired["ReqB"]);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    /// <summary>
    /// 多模块_互不影响：多模块独立计数，互不影响。
    /// </summary>
    [Fact]
    public void MultipleModules_DoNotAffectEachOther()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");

        try
        {
            var a = NewList(1, "Player", "ReqA");
            var b = NewList(2, "Bag", "ReqOpen");
            MessageIdCoordinator.AssignAndPersist(path, new[] { a, b });

            Assert.Equal(10, a.Infos[0].Opcode);
            Assert.Equal(10, b.Infos[0].Opcode);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    /// <summary>
    /// Lock损坏_报错而非重新分配：文件被外部改成损坏 JSON 时，Coordinator 必须抛错（绝不静默走首次分配）。
    /// </summary>
    [Fact]
    public void CorruptedLock_ThrowsInsteadOfReassigning()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");
        File.WriteAllText(path, "{ this is not valid json");

        try
        {
            var a = NewList(1, "Player", "ReqA");
            Assert.Throws<InvalidDataException>(() => MessageIdCoordinator.AssignAndPersist(path, new[] { a }));

            // Opcode 必须保持 0 —— 不要把损坏的 lock 当成「不存在」，否则导出器会按旧自增方式静默走完。
            Assert.Equal(0, a.Infos[0].Opcode);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    /// <summary>
    /// 参数守卫_null或空白路径即抛异常:lockPath 为 null / 空串 / 纯空白抛 ArgumentException；lists 为 null 抛 ArgumentNullException。
    /// </summary>
    [Fact]
    public void NullOrWhiteSpaceLockPath_Throws()
    {
        var lists = new[] { NewList(1, "Player", "ReqA") };
        var unusedPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        // ThrowIfNullOrWhiteSpace 对 null/空串抛 ArgumentNullException（ArgumentException 派生类），用 ThrowsAny 覆盖同族
        Assert.ThrowsAny<ArgumentException>(() => MessageIdCoordinator.AssignAndPersist(null, lists));
        Assert.ThrowsAny<ArgumentException>(() => MessageIdCoordinator.AssignAndPersist(string.Empty, lists));
        Assert.ThrowsAny<ArgumentException>(() => MessageIdCoordinator.AssignAndPersist("   ", lists));
        Assert.Throws<ArgumentNullException>(() => MessageIdCoordinator.AssignAndPersist(unusedPath, null));
    }

    /// <summary>
    /// 空列表_锁内容不变:lists 为空集合时不新增分配，现有 lock 字节级不变，统计全为零。
    /// </summary>
    [Fact]
    public void EmptyLists_LockUnchanged()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");

        try
        {
            var a = NewList(1, "Player", "ReqA", "ReqB");
            MessageIdCoordinator.AssignAndPersist(path, new[] { a });
            var before = File.ReadAllText(path);

            var result = MessageIdCoordinator.AssignAndPersist(path, new MessageInfoList[0]);

            Assert.Equal(0, result.ModuleCount);
            Assert.Equal(0, result.NewlyAssignedCount);
            Assert.Equal(before, File.ReadAllText(path));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    /// <summary>
    /// 非消息条目_被过滤不占号:IsEnum 与非 Req/Resp/Notify 命名的 info 不参与分配。
    /// 模块桶仍会落一条空 ModuleEntry（分组先于过滤建桶），此处一并固化该语义。
    /// </summary>
    [Fact]
    public void NonMessageEntries_FilteredFromAllocation()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");

        try
        {
            var list = NewList(1, "Player");
            list.Infos.Add(new MessageInfo(true) { Name = "ItemKind" }); // enum：跳过
            list.Infos.Add(new MessageInfo { Name = "Foo" });            // 非 Req/Resp/Notify：跳过

            var result = MessageIdCoordinator.AssignAndPersist(path, new[] { list });

            Assert.Equal(0, result.NewlyAssignedCount);
            var reloaded = MessageIdLockStore.Load(path);
            Assert.True(reloaded.Modules.ContainsKey("1"));
            Assert.Empty(reloaded.Modules["1"].Messages);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    /// <summary>
    /// 模块号边界值_照常分配:0、负数与 short 极值的模块号均生成合法 key 并正常分配（负模块号是内部协议约定）。
    /// </summary>
    [Theory]
    [InlineData((short)0)]
    [InlineData((short)-1)]
    [InlineData(short.MaxValue)]
    [InlineData(short.MinValue)]
    public void BoundaryModuleNumbers_AllocateNormally(short module)
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");

        try
        {
            var list = NewList(module, "M", "ReqA");
            MessageIdCoordinator.AssignAndPersist(path, new[] { list });

            var reloaded = MessageIdLockStore.Load(path);
            var key = module.ToString(System.Globalization.CultureInfo.InvariantCulture);
            Assert.Equal(10, reloaded.Modules[key].Messages["ReqA"]);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}