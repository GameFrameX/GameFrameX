using System.Collections.Generic;
using System.IO;
using GameFrameX.ProtoExport;
using GameFrameX.ProtoExport.Persistence;
using Xunit;

namespace ProtoExporterGUI.Tests;

/// <summary>
/// MessageIdAllocator + MessageIdLockStore 的单测。
/// 覆盖「分配稳定性」的不变式：插入中间、重排、删除、重命名、墓碑号复用，
/// 在同一份 lock 上来回跑，结果必须字节级一致。
/// </summary>
public class MessageIdAllocatorTests
{
    private const string ModuleKey = "1";
    private const string ModuleName = "Player";

    private static MessageInfo Msg(string name, int opcode = 0)
    {
        return new MessageInfo
        {
            Name = name,
            Opcode = opcode,
        };
    }

    private static MessageIdLock NewLock()
    {
        return MessageIdLock.CreateEmpty();
    }

    private static IReadOnlyList<string> Apply(MessageIdLock lockData, params string[] names)
    {
        var msgs = new List<MessageInfo>();
        foreach (var n in names)
        {
            msgs.Add(Msg(n));
        }

        return MessageIdAllocator.Mutate(lockData, ModuleKey, ModuleName, msgs);
    }

    /// <summary>
    /// 首次分配_从FirstSubId开始：首次为模块分配 SubId 必须从 <see cref="MessageIdAllocator.FirstSubId"/> = 10 起。
    /// </summary>
    [Fact]
    public void FirstAllocation_StartsAtFirstSubId()
    {
        var lockData = NewLock();
        var msgs = new List<MessageInfo>
        {
            Msg("ReqLogin"),
            Msg("RespLogin"),
            Msg("NotifyHeartbeat"),
        };

        MessageIdAllocator.Mutate(lockData, ModuleKey, ModuleName, msgs);

        Assert.Equal(10, lockData.Modules[ModuleKey].Messages["ReqLogin"]);
        Assert.Equal(11, lockData.Modules[ModuleKey].Messages["RespLogin"]);
        Assert.Equal(12, lockData.Modules[ModuleKey].Messages["NotifyHeartbeat"]);

        // SubId 必须回写到 MessageInfo.Opcode，否则下游 helper 消费不到。
        Assert.Equal(10, msgs[0].Opcode);
        Assert.Equal(11, msgs[1].Opcode);
        Assert.Equal(12, msgs[2].Opcode);
    }

    /// <summary>
    /// 重排消息_沿用历史SubId：已分配的消息即使在输入列表中被重排，SubId 必须保持不变。
    /// </summary>
    [Fact]
    public void ReorderMessages_ReusesHistoricalSubIds()
    {
        var lockData = NewLock();
        Apply(lockData, "ReqA", "ReqB", "ReqC");

        // 第二次输入顺序完全反过来
        Apply(lockData, "ReqC", "ReqB", "ReqA");

        Assert.Equal(10, lockData.Modules[ModuleKey].Messages["ReqA"]);
        Assert.Equal(11, lockData.Modules[ModuleKey].Messages["ReqB"]);
        Assert.Equal(12, lockData.Modules[ModuleKey].Messages["ReqC"]);
    }

    /// <summary>
    /// 中间插入_老消息SubId不变：在已有消息中间插入新消息，老消息 SubId 必须保持不变。
    /// </summary>
    [Fact]
    public void InsertInMiddle_OldMessagesKeepSubId()
    {
        var lockData = NewLock();
        Apply(lockData, "ReqA", "ReqC");

        // 第一次：ReqA=10, ReqC=11
        Assert.Equal(10, lockData.Modules[ModuleKey].Messages["ReqA"]);
        Assert.Equal(11, lockData.Modules[ModuleKey].Messages["ReqC"]);

        // 在中间插入 ReqB
        Apply(lockData, "ReqA", "ReqB", "ReqC");

        Assert.Equal(10, lockData.Modules[ModuleKey].Messages["ReqA"]);
        Assert.Equal(11, lockData.Modules[ModuleKey].Messages["ReqC"]);
        Assert.Equal(12, lockData.Modules[ModuleKey].Messages["ReqB"]);
    }

    /// <summary>
    /// 删除消息_进入Retired_永不回收：删除一个消息时它的 SubId 进入 Retired，永不复用。
    /// </summary>
    [Fact]
    public void DeleteMessage_MovesToRetired_NeverReused()
    {
        var lockData = NewLock();
        Apply(lockData, "ReqA", "ReqB", "ReqC");
        // ReqA=10, ReqB=11, ReqC=12

        // 第二次只留 ReqA、ReqC
        Apply(lockData, "ReqA", "ReqC");

        Assert.Equal(10, lockData.Modules[ModuleKey].Messages["ReqA"]);
        Assert.Equal(12, lockData.Modules[ModuleKey].Messages["ReqC"]);

        // ReqB 进入 Retired
        Assert.True(lockData.Modules[ModuleKey].Retired.ContainsKey("ReqB"));
        Assert.Equal(11, lockData.Modules[ModuleKey].Retired["ReqB"]);

        // 再加新消息，SubId 必须跳过 11
        Apply(lockData, "ReqA", "ReqC", "ReqD");

        Assert.Equal(13, lockData.Modules[ModuleKey].Messages["ReqD"]);
    }

    /// <summary>
    /// 重命名_旧号永不回收：重命名 = 删旧 + 加新，新名拿到 max+1，旧名进入 Retired，旧号永不复用。
    /// </summary>
    [Fact]
    public void RenameMessage_OldSubIdNeverReused()
    {
        var lockData = NewLock();
        Apply(lockData, "ReqLogin", "RespLogin");
        // ReqLogin=10, RespLogin=11

        // 把 ReqLogin 重命名为 ReqLoginV2
        Apply(lockData, "ReqLoginV2", "RespLogin");

        Assert.True(lockData.Modules[ModuleKey].Retired.ContainsKey("ReqLogin"));
        Assert.Equal(10, lockData.Modules[ModuleKey].Retired["ReqLogin"]);
        Assert.Equal(12, lockData.Modules[ModuleKey].Messages["ReqLoginV2"]);
        Assert.Equal(11, lockData.Modules[ModuleKey].Messages["RespLogin"]);
    }

    /// <summary>
    /// 幂等_同输入反复跑结果一致：同一份 lock + 同一份 proto，无论跑几遍，最终 lock 必须字节级一致。
    /// </summary>
    [Fact]
    public void Idempotent_RepeatedRunsOnSameInputYieldSameResult()
    {
        var lockData = NewLock();
        // 首次落 lock
        Apply(lockData, "ReqA", "ReqB", "ReqC");
        var first = MessageIdLockStore.SaveToString(lockData);

        // 同输入再跑三遍，每遍都换顺序
        Apply(lockData, "ReqC", "ReqB", "ReqA");
        var second = MessageIdLockStore.SaveToString(lockData);
        Apply(lockData, "ReqA", "ReqC", "ReqB");
        var third = MessageIdLockStore.SaveToString(lockData);
        Apply(lockData, "ReqB", "ReqA", "ReqC");
        var fourth = MessageIdLockStore.SaveToString(lockData);

        Assert.Equal(first, second);
        Assert.Equal(first, third);
        Assert.Equal(first, fourth);
    }

    /// <summary>
    /// 多模块_独立计数：模块间独立计数，一个模块的 SubId 变化不影响另一个。
    /// </summary>
    [Fact]
    public void MultipleModules_IndependentCounting()
    {
        var lockData = NewLock();

        var player = new List<MessageInfo> { Msg("ReqA"), Msg("ReqB") };
        var bag = new List<MessageInfo> { Msg("ReqOpen") };

        MessageIdAllocator.Mutate(lockData, "1", "Player", player);
        MessageIdAllocator.Mutate(lockData, "2", "Bag", bag);

        Assert.Equal(10, lockData.Modules["1"].Messages["ReqA"]);
        Assert.Equal(11, lockData.Modules["1"].Messages["ReqB"]);
        Assert.Equal(10, lockData.Modules["2"].Messages["ReqOpen"]);
    }

    /// <summary>
    /// ModuleKey越界_Load报错：ModuleID 越界（&gt; short.MaxValue）写入 lock 必须报错，不能静默写入。
    /// </summary>
    [Fact]
    public void ModuleKeyOutOfRange_LoadThrows()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path,
                "{ \"schemaVersion\": 1, \"modules\": { \"99999\": { \"moduleName\": \"Bad\", \"messages\": { \"ReqA\": 10 } } } }");
            Assert.Throws<InvalidDataException>(() => MessageIdLockStore.Load(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// SchemaVersion不兼容_Load报错：schemaVersion 不一致必须报错，避免「读了一份过期的 lock，导出器静默重排」。
    /// </summary>
    [Fact]
    public void SchemaVersionIncompatible_LoadThrows()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path,
                "{ \"schemaVersion\": 99, \"modules\": {} }");
            Assert.Throws<InvalidDataException>(() => MessageIdLockStore.Load(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// 文件不存在_Load返回空lock：lock 文件不存在时 Load 必须返回空 lock，不报错（首次运行场景）。
    /// </summary>
    [Fact]
    public void FileMissing_LoadReturnsEmptyLock()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            var lockData = MessageIdLockStore.Load(path);
            Assert.NotNull(lockData);
            Assert.Equal(1, lockData.SchemaVersion);
            Assert.Empty(lockData.Modules);
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
    /// SaveLoad_往返等价：Save → Load 往返必须保持等价，便于 PR review 时看 diff。
    /// </summary>
    [Fact]
    public void SaveLoad_RoundTripEquivalent()
    {
        var path = Path.GetTempFileName();
        try
        {
            var lockData = NewLock();
            Apply(lockData, "ReqA", "ReqB", "ReqC");

            MessageIdLockStore.Save(path, lockData);

            var reloaded = MessageIdLockStore.Load(path);
            Assert.Equal(10, reloaded.Modules[ModuleKey].Messages["ReqA"]);
            Assert.Equal(11, reloaded.Modules[ModuleKey].Messages["ReqB"]);
            Assert.Equal(12, reloaded.Modules[ModuleKey].Messages["ReqC"]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// 参数守卫_null即抛ArgumentNullException:lockData / moduleKey / messages 任一为 null 必须立即抛 ArgumentNullException。
    /// </summary>
    [Fact]
    public void NullArguments_ThrowArgumentNullException()
    {
        var lockData = NewLock();
        var msgs = new List<MessageInfo> { Msg("ReqA") };

        Assert.Throws<ArgumentNullException>(() => MessageIdAllocator.Mutate(null, ModuleKey, ModuleName, msgs));
        Assert.Throws<ArgumentNullException>(() => MessageIdAllocator.Mutate(lockData, null, ModuleName, msgs));
        Assert.Throws<ArgumentNullException>(() => MessageIdAllocator.Mutate(lockData, ModuleKey, ModuleName, null));
    }

    /// <summary>
    /// 历史SubId越界_报错:lock 中仍在 proto 里的消息，历史 SubId 为 0 / 负数 / 超上限时必须报错，不能静默沿用。
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0x10000)]
    public void HistoricalSubIdOutOfRange_Throws(int badSubId)
    {
        var lockData = NewLock();
        var entry = new ModuleEntry { ModuleName = ModuleName };
        entry.Messages["ReqA"] = badSubId;
        lockData.Modules[ModuleKey] = entry;

        var msgs = new List<MessageInfo> { Msg("ReqA") };

        Assert.Throws<InvalidDataException>(() => MessageIdAllocator.Mutate(lockData, ModuleKey, ModuleName, msgs));
    }

    /// <summary>
    /// SubId上限边界值_合法沿用:历史 SubId = MaxSubId(65535)是合法值，必须原样沿用不报错。
    /// </summary>
    [Fact]
    public void SubIdAtMaxBoundary_IsReusedAsIs()
    {
        var lockData = NewLock();
        var entry = new ModuleEntry { ModuleName = ModuleName };
        entry.Messages["ReqA"] = MessageIdAllocator.MaxSubId;
        lockData.Modules[ModuleKey] = entry;

        var msg = Msg("ReqA");
        MessageIdAllocator.Mutate(lockData, ModuleKey, ModuleName, new List<MessageInfo> { msg });

        Assert.Equal(MessageIdAllocator.MaxSubId, msg.Opcode);
    }

    /// <summary>
    /// SubId耗尽_报错而非回绕:模块 max 已达 65535 时再新增消息，必须报错，不能回绕或复用旧号。
    /// </summary>
    [Fact]
    public void SubIdExhausted_ThrowsInsteadOfWrapping()
    {
        var lockData = NewLock();
        var entry = new ModuleEntry { ModuleName = ModuleName };
        entry.Messages["ReqA"] = MessageIdAllocator.MaxSubId;
        lockData.Modules[ModuleKey] = entry;

        var msgs = new List<MessageInfo> { Msg("ReqA"), Msg("ReqB") };

        Assert.Throws<InvalidDataException>(() => MessageIdAllocator.Mutate(lockData, ModuleKey, ModuleName, msgs));
    }

    /// <summary>
    /// 自定义起点_从指定值起分:显式传 firstSubId 时首次分配从该值起递增。
    /// </summary>
    [Fact]
    public void CustomFirstSubId_AllocatesFromGivenValue()
    {
        var lockData = NewLock();
        var msgs = new List<MessageInfo> { Msg("ReqA"), Msg("ReqB") };

        MessageIdAllocator.Mutate(lockData, ModuleKey, ModuleName, msgs, 100);

        Assert.Equal(100, lockData.Modules[ModuleKey].Messages["ReqA"]);
        Assert.Equal(101, lockData.Modules[ModuleKey].Messages["ReqB"]);
    }

    /// <summary>
    /// 自定义起点超上限_首个分配即报错:firstSubId 大于 MaxSubId 时直接抛耗尽异常。
    /// </summary>
    [Fact]
    public void CustomFirstSubIdBeyondMax_ThrowsImmediately()
    {
        var lockData = NewLock();
        var msgs = new List<MessageInfo> { Msg("ReqA") };

        Assert.Throws<InvalidDataException>(() =>
            MessageIdAllocator.Mutate(lockData, ModuleKey, ModuleName, msgs, MessageIdAllocator.MaxSubId + 1));
    }

    /// <summary>
    /// 模块名同步_非空覆盖空值保留:非空 moduleName 覆盖 lock 内旧名；null / 空串不覆盖已有内容。
    /// </summary>
    [Fact]
    public void ModuleNameSync_NonEmptyOverwrites_EmptyKeeps()
    {
        var lockData = NewLock();
        Apply(lockData, "ReqA");
        Assert.Equal(ModuleName, lockData.Modules[ModuleKey].ModuleName);

        // 非空新名覆盖
        MessageIdAllocator.Mutate(lockData, ModuleKey, "PlayerV2", new List<MessageInfo> { Msg("ReqA") });
        Assert.Equal("PlayerV2", lockData.Modules[ModuleKey].ModuleName);

        // 空串 / null 不覆盖
        MessageIdAllocator.Mutate(lockData, ModuleKey, string.Empty, new List<MessageInfo> { Msg("ReqA") });
        Assert.Equal("PlayerV2", lockData.Modules[ModuleKey].ModuleName);

        MessageIdAllocator.Mutate(lockData, ModuleKey, null, new List<MessageInfo> { Msg("ReqA") });
        Assert.Equal("PlayerV2", lockData.Modules[ModuleKey].ModuleName);
    }

    /// <summary>
    /// 空消息列表_存量整体进入Retired:直接给 Mutate 传空列表时，该模块现有消息全部按删除处理。
    /// </summary>
    [Fact]
    public void EmptyMessageList_RetiresEverything()
    {
        var lockData = NewLock();
        Apply(lockData, "ReqA", "ReqB");
        // ReqA=10, ReqB=11

        MessageIdAllocator.Mutate(lockData, ModuleKey, ModuleName, new List<MessageInfo>());

        Assert.Empty(lockData.Modules[ModuleKey].Messages);
        Assert.Equal(2, lockData.Modules[ModuleKey].Retired.Count);
        Assert.Equal(10, lockData.Modules[ModuleKey].Retired["ReqA"]);
        Assert.Equal(11, lockData.Modules[ModuleKey].Retired["ReqB"]);
    }

    /// <summary>
    /// 枚举条目_不参与分配:IsEnum = true 的 info 被跳过，不占号也不写入 lock。
    /// </summary>
    [Fact]
    public void EnumEntries_SkippedFromAllocation()
    {
        var lockData = NewLock();
        var msgs = new List<MessageInfo>
        {
            Msg("ReqA"),
            new MessageInfo(true) { Name = "ItemKind" },
        };

        var assigned = MessageIdAllocator.Mutate(lockData, ModuleKey, ModuleName, msgs);

        Assert.Single(assigned);
        Assert.Equal(10, lockData.Modules[ModuleKey].Messages["ReqA"]);
        Assert.False(lockData.Modules[ModuleKey].Messages.ContainsKey("ItemKind"));
    }

    /// <summary>
    /// 大小写敏感_视为改名:消息名按 Ordinal 比较，"ReqA" 改为 "reqa" 等同删旧加新（旧名进 Retired，新名拿 max+1）。
    /// </summary>
    [Fact]
    public void CaseSensitiveRename_TreatedAsDeleteAndAdd()
    {
        var lockData = NewLock();
        Apply(lockData, "ReqA", "ReqB");
        // ReqA=10, ReqB=11

        Apply(lockData, "reqa", "ReqB");

        Assert.Equal(12, lockData.Modules[ModuleKey].Messages["reqa"]);
        Assert.Equal(10, lockData.Modules[ModuleKey].Retired["ReqA"]);
        Assert.Equal(11, lockData.Modules[ModuleKey].Messages["ReqB"]);
    }

    /// <summary>
    /// 同名双登记_Retired优先:手改 lock 造成同名同时出现在 Messages 与 Retired 时，以 Retired 的号为准并复活回 Messages。
    /// </summary>
    [Fact]
    public void SameNameInMessagesAndRetired_RetiredWins()
    {
        var lockData = NewLock();
        var entry = new ModuleEntry { ModuleName = ModuleName };
        entry.Messages["ReqA"] = 10;
        entry.Retired["ReqA"] = 20;
        lockData.Modules[ModuleKey] = entry;

        var msg = Msg("ReqA");
        MessageIdAllocator.Mutate(lockData, ModuleKey, ModuleName, new List<MessageInfo> { msg });

        Assert.Equal(20, msg.Opcode);
        Assert.Equal(20, lockData.Modules[ModuleKey].Messages["ReqA"]);
        Assert.False(lockData.Modules[ModuleKey].Retired.ContainsKey("ReqA"));
    }

    /// <summary>
    /// 空白文件内容_Load返回空lock:文件存在但内容为空白时按空 lock 处理，不报错。
    /// </summary>
    [Fact]
    public void WhitespaceFileContent_LoadReturnsEmptyLock()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "   \r\n  ");
            var lockData = MessageIdLockStore.Load(path);

            Assert.Equal(1, lockData.SchemaVersion);
            Assert.Empty(lockData.Modules);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// JSON字面量null_Load报错:内容为 "null" 反序列化得 null 实例，必须报错而非当空 lock 返回。
    /// </summary>
    [Fact]
    public void JsonNullLiteral_LoadThrows()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "null");
            Assert.Throws<InvalidDataException>(() => MessageIdLockStore.Load(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// ModuleKey非数字_Load报错:module key 不是整数（如 "abc" / "1.5"）必须报错，不能进入分配链路。
    /// </summary>
    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    public void NonNumericModuleKey_LoadThrows(string badKey)
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path,
                "{ \"schemaVersion\": 1, \"modules\": { \"" + badKey + "\": { \"moduleName\": \"Bad\", \"messages\": {} } } }");
            Assert.Throws<InvalidDataException>(() => MessageIdLockStore.Load(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Save到不存在的目录_自动创建:目标目录缺失时自动创建后再原子写入。
    /// </summary>
    [Fact]
    public void SaveToMissingDirectory_CreatesDirectoryAndWrites()
    {
        var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var path = Path.Combine(root, "nested", "a.lock.json");
        try
        {
            var lockData = NewLock();
            Apply(lockData, "ReqA");

            MessageIdLockStore.Save(path, lockData);

            Assert.True(File.Exists(path));
            var reloaded = MessageIdLockStore.Load(path);
            Assert.Equal(10, reloaded.Modules[ModuleKey].Messages["ReqA"]);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    /// <summary>
    /// Save与SaveToString_null参数抛ArgumentNullException。
    /// </summary>
    [Fact]
    public void SaveNullLock_ThrowsArgumentNullException()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Assert.Throws<ArgumentNullException>(() => MessageIdLockStore.Save(path, null));
        Assert.Throws<ArgumentNullException>(() => MessageIdLockStore.SaveToString(null));
    }

    /// <summary>
    /// 同批同名消息_只登记一次：重复名防御性去重——分配与 lock 登记只做一次，
    /// 回写循环仍给每条同名 info 写同一个号（调用方不会拿到半配置状态）。
    /// </summary>
    [Fact]
    public void DuplicateNamesInSameBatch_RegisteredOnlyOnce()
    {
        var lockData = NewLock();
        var first = Msg("ReqA");
        var second = Msg("ReqA");

        var assigned = MessageIdAllocator.Mutate(lockData, ModuleKey, ModuleName, new List<MessageInfo> { first, second });

        Assert.Single(assigned);
        Assert.Equal(10, first.Opcode);
        Assert.Equal(10, second.Opcode);
        Assert.Single(lockData.Modules[ModuleKey].Messages);
        Assert.Equal(10, lockData.Modules[ModuleKey].Messages["ReqA"]);
    }

    /// <summary>
    /// 模块字段为null_Load报错:modules / 模块条目 / messages / retired 显式写 null 的手改 lock
    /// 必须报受控 InvalidDataException，而非让后续分配链路 NRE。
    /// </summary>
    [Theory]
    [InlineData("{ \"schemaVersion\": 1, \"modules\": null }")]
    [InlineData("{ \"schemaVersion\": 1, \"modules\": { \"1\": null } }")]
    [InlineData("{ \"schemaVersion\": 1, \"modules\": { \"1\": { \"moduleName\": \"M\", \"messages\": null } } }")]
    [InlineData("{ \"schemaVersion\": 1, \"modules\": { \"1\": { \"moduleName\": \"M\", \"messages\": {}, \"retired\": null } } }")]
    public void NullModuleFields_LoadThrows(string json)
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, json);
            Assert.Throws<InvalidDataException>(() => MessageIdLockStore.Load(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// 并发Save_互不覆盖临时文件:多线程同时 Save 同一目标时各用随机临时文件，
    /// 不因固定 tmp 名撞车抛 IOException；最终文件总是完整可解析的 lock，且无临时文件残留。
    /// </summary>
    [Fact]
    public void ConcurrentSaves_DoNotCollideOnTempFiles()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");
        try
        {
            var failures = new List<Exception>();
            var tasks = new System.Threading.Tasks.Task[16];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        var lockData = NewLock();
                        Apply(lockData, "ReqA", "ReqB");
                        MessageIdLockStore.Save(path, lockData);
                    }
                    catch (Exception ex)
                    {
                        lock (failures)
                        {
                            failures.Add(ex);
                        }
                    }
                });
            }

            System.Threading.Tasks.Task.WaitAll(tasks);

            Assert.Empty(failures);
            var reloaded = MessageIdLockStore.Load(path);
            Assert.Equal(10, reloaded.Modules[ModuleKey].Messages["ReqA"]);
            Assert.Equal(11, reloaded.Modules[ModuleKey].Messages["ReqB"]);
            Assert.Empty(Directory.GetFiles(Path.GetDirectoryName(path), Path.GetFileName(path) + "*.tmp"));
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