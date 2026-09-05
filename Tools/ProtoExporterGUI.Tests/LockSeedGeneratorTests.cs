using System.Collections.Generic;
using System.IO;
using GameFrameX.ProtoExport;
using GameFrameX.ProtoExport.Persistence;
using Xunit;

namespace ProtoExporterGUI.Tests;

/// <summary>
/// LockSeedGenerator 的契约：
/// 1) SeedFromCurrentOpcodes 必须把当前 Opcode 1:1 落到 Messages，不重新分配；
/// 2) 调用一次后，再走 Coordinator.AssignAndPersist，老号纹丝不动；
/// 3) Opcode <= 0 必须报错（防止误传空 proto 集合）。
/// </summary>
public class LockSeedGeneratorTests
{
    private static MessageInfoList ListWithOpcodes(short module, params (string name, int opcode)[] entries)
    {
        var list = new MessageInfoList { Module = module, ModuleName = "M" };
        foreach (var (name, opcode) in entries)
        {
            list.Infos.Add(new MessageInfo { Name = name, Opcode = opcode });
        }

        return list;
    }

    /// <summary>
    /// Seed 冻结当前 Opcode 作为起点，不重新分配。
    /// </summary>
    [Fact]
    public void Seed_FreezesCurrentOpcodesAsBaseline()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");
        try
        {
            var lists = new[]
            {
                ListWithOpcodes(1, ("ReqLogin", 10), ("RespLogin", 11)),
                ListWithOpcodes(2, ("ReqOpen", 10)),
            };

            var result = LockSeedGenerator.SeedFromCurrentOpcodes(path, lists);
            Assert.Equal(2, result.ModuleCount);
            Assert.Equal(3, result.NewlyAssignedCount);

            var lockData = MessageIdLockStore.Load(path);
            Assert.Equal(10, lockData.Modules["1"].Messages["ReqLogin"]);
            Assert.Equal(11, lockData.Modules["1"].Messages["RespLogin"]);
            Assert.Equal(10, lockData.Modules["2"].Messages["ReqOpen"]);
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
    /// Seed 后的 lock 喂给 Coordinator —— 老号必须保持不变；新增消息续号。
    /// </summary>
    [Fact]
    public void SeedThenCoordinator_OldSubIdsUnchanged()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");
        try
        {
            // Seed：模拟迁移
            LockSeedGenerator.SeedFromCurrentOpcodes(path, new[]
            {
                ListWithOpcodes(1, ("ReqLogin", 10), ("RespLogin", 11)),
            });

            // 再走 Coordinator，新消息续号
            var lists = new[] { ListWithOpcodes(1, ("ReqLogin", 0), ("RespLogin", 0), ("ReqLogout", 0)) };
            MessageIdCoordinator.AssignAndPersist(path, lists);

            Assert.Equal(10, lists[0].Infos[0].Opcode); // 沿用
            Assert.Equal(11, lists[0].Infos[1].Opcode); // 沿用
            Assert.Equal(12, lists[0].Infos[2].Opcode); // 新增
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
    /// Seed 时 Opcode 非法（&lt;= 0）报错，防止误传空 proto 集合。
    /// </summary>
    [Fact]
    public void Seed_InvalidOpcode_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");
        try
        {
            var lists = new[] { ListWithOpcodes(1, ("ReqA", 0)) };
            Assert.Throws<InvalidDataException>(() => LockSeedGenerator.SeedFromCurrentOpcodes(path, lists));
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
    /// Seed 时 Opcode 超出 SubId 上限报错。
    /// </summary>
    [Fact]
    public void Seed_OpcodeExceedsRange_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");
        try
        {
            var lists = new[] { ListWithOpcodes(1, ("ReqA", 0x10000)) };
            Assert.Throws<InvalidDataException>(() => LockSeedGenerator.SeedFromCurrentOpcodes(path, lists));
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
    /// FormatLockForDisplay 输出等价于序列化输出。
    /// </summary>
    [Fact]
    public void FormatLockForDisplay_EqualsSerializedOutput()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");
        try
        {
            LockSeedGenerator.SeedFromCurrentOpcodes(path, new[]
            {
                ListWithOpcodes(1, ("ReqA", 10), ("ReqB", 11)),
            });

            var fromFile = MessageIdLockStore.Load(path);
            var formatted = LockSeedGenerator.FormatLockForDisplay(fromFile);
            var raw = MessageIdLockStore.SaveToString(fromFile);

            Assert.Equal(raw, formatted);
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
    /// 参数守卫_null或空白路径与null列表抛异常。
    /// </summary>
    [Fact]
    public void NullArguments_Throw()
    {
        var lists = new[] { ListWithOpcodes(1, ("ReqA", 10)) };
        var unusedPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        // ThrowIfNullOrWhiteSpace 对 null 抛 ArgumentNullException（ArgumentException 派生类），用 ThrowsAny 覆盖同族
        Assert.ThrowsAny<ArgumentException>(() => LockSeedGenerator.SeedFromCurrentOpcodes(null, lists));
        Assert.ThrowsAny<ArgumentException>(() => LockSeedGenerator.SeedFromCurrentOpcodes("   ", lists));
        Assert.Throws<ArgumentNullException>(() => LockSeedGenerator.SeedFromCurrentOpcodes(unusedPath, null));
    }

    /// <summary>
    /// Seed 时 Opcode 为负数报错（与 0 同走非法分支）。
    /// </summary>
    [Fact]
    public void Seed_NegativeOpcode_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");
        try
        {
            var lists = new[] { ListWithOpcodes(1, ("ReqA", -5)) };
            Assert.Throws<InvalidDataException>(() => LockSeedGenerator.SeedFromCurrentOpcodes(path, lists));
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
    /// 重复Seed_整体覆盖且丢弃历史:对已有 lock 再次 Seed 时从空 lock 起步，
    /// 原有消息与 Retired 历史被丢弃（语义等同 --regenerate-lock，不做增量合并）。
    /// </summary>
    [Fact]
    public void SecondSeed_OverwritesAndDiscardsHistory()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");
        try
        {
            LockSeedGenerator.SeedFromCurrentOpcodes(path, new[]
            {
                ListWithOpcodes(1, ("ReqA", 10), ("ReqB", 11)),
            });

            // 第二次 Seed 只带 ReqA=20：ReqB 与潜在的 Retired 历史必须消失
            LockSeedGenerator.SeedFromCurrentOpcodes(path, new[]
            {
                ListWithOpcodes(1, ("ReqA", 20)),
            });

            var reloaded = MessageIdLockStore.Load(path);
            Assert.Equal(20, reloaded.Modules["1"].Messages["ReqA"]);
            Assert.False(reloaded.Modules["1"].Messages.ContainsKey("ReqB"));
            Assert.Empty(reloaded.Modules["1"].Retired);
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
    /// 非消息条目_被过滤:IsEnum 与非 Req/Resp/Notify 命名的 info 不进入 lock，其非法 Opcode 也不触发校验。
    /// </summary>
    [Fact]
    public void NonMessageEntries_FilteredFromSeed()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");
        try
        {
            var list = new MessageInfoList { Module = 1, ModuleName = "M" };
            list.Infos.Add(new MessageInfo(true) { Name = "ItemKind", Opcode = 0 });  // enum：跳过，Opcode=0 不报错
            list.Infos.Add(new MessageInfo { Name = "Foo", Opcode = -1 });           // 非消息：跳过
            list.Infos.Add(new MessageInfo { Name = "ReqA", Opcode = 10 });

            var result = LockSeedGenerator.SeedFromCurrentOpcodes(path, new[] { list });

            Assert.Equal(1, result.NewlyAssignedCount);
            var reloaded = MessageIdLockStore.Load(path);
            Assert.Single(reloaded.Modules["1"].Messages);
            Assert.Equal(10, reloaded.Modules["1"].Messages["ReqA"]);
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
    /// FormatLockForDisplay_null参数抛ArgumentNullException。
    /// </summary>
    [Fact]
    public void FormatLockForDisplay_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => LockSeedGenerator.FormatLockForDisplay(null));
    }

    /// <summary>
    /// Seed 时同模块重复 Opcode 报错：冻结含同号双登记的 lock 会在后续分配中撞号，必须在 seed 阶段拦截。
    /// </summary>
    [Fact]
    public void Seed_DuplicateOpcode_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");
        try
        {
            var lists = new[] { ListWithOpcodes(1, ("ReqA", 10), ("ReqB", 10)) };

            var ex = Assert.Throws<InvalidDataException>(() => LockSeedGenerator.SeedFromCurrentOpcodes(path, lists));

            Assert.Contains("ReqA", ex.Message);
            Assert.Contains("ReqB", ex.Message);
            // 报错时不落盘 lock 文件
            Assert.False(File.Exists(path));
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