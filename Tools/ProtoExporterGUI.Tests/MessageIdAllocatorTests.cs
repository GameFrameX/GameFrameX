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
    /// 首次为模块分配 SubId 必须从 <see cref="MessageIdAllocator.FirstSubId"/> = 10 起。
    /// </summary>
    [Fact]
    public void 首次分配_从FirstSubId开始()
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
    /// 已分配的消息即使在输入列表中被重排，SubId 必须保持不变。
    /// </summary>
    [Fact]
    public void 重排消息_沿用历史SubId()
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
    /// 在已有消息中间插入新消息，老消息 SubId 必须保持不变。
    /// </summary>
    [Fact]
    public void 中间插入_老消息SubId不变()
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
    /// 删除一个消息：它的 SubId 进入 Retired，永不复用。
    /// </summary>
    [Fact]
    public void 删除消息_进入Retired_永不回收()
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
    /// 重命名 = 删旧 + 加新：新名拿到 max+1，旧名进入 Retired，旧号永不复用。
    /// </summary>
    [Fact]
    public void 重命名_旧号永不回收()
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
    /// 同一份 lock + 同一份 proto，无论跑几遍，最终 lock 必须字节级一致。
    /// </summary>
    [Fact]
    public void 幂等_同输入反复跑结果一致()
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
    /// 模块间独立计数：一个模块的 SubId 变化不影响另一个。
    /// </summary>
    [Fact]
    public void 多模块_独立计数()
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
    /// ModuleID 越界（&gt; short.MaxValue）写入 lock 必须报错，不能静默写入。
    /// </summary>
    [Fact]
    public void ModuleKey越界_Load报错()
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
    /// schemaVersion 不一致必须报错，避免「读了一份过期的 lock，导出器静默重排」。
    /// </summary>
    [Fact]
    public void SchemaVersion不兼容_Load报错()
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
    /// lock 文件不存在时 Load 必须返回空 lock，不报错（首次运行场景）。
    /// </summary>
    [Fact]
    public void 文件不存在_Load返回空lock()
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
    /// Save → Load 往返必须保持等价，便于 PR review 时看 diff。
    /// </summary>
    [Fact]
    public void SaveLoad_往返等价()
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
}