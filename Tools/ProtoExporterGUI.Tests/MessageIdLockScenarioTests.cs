using System.Collections.Generic;
using System.IO;
using GameFrameX.ProtoExport;
using GameFrameX.ProtoExport.Persistence;
using Xunit;

namespace ProtoExporterGUI.Tests
{
    /// <summary>
    /// 端到端场景测试（对照 P0 设计矩阵）：直接构造 <see cref="MessageInfoList"/> 走
    /// <see cref="MessageIdCoordinator.AssignAndPersist"/>，不跑完整导出。
    /// <para>
    /// 场景：基线 S1（3 消息模块首次导出）→ 变异 A（中间插入）/ B（整体重排）/
    /// C（删除）/ D（重命名）→ 回归（各变异后重跑原基线输入）。
    /// 核心不变式：任何变异后老消息 SubId 字节级不变；新号只从 max+1 续；删除与重命名的旧号永不回收。
    /// </para>
    /// </summary>
    public class MessageIdLockScenarioTests
    {
        private const short Module = 1;
        private const string ModuleName = "Player";
        private const string ModuleKey = "1";

        private static MessageInfoList NewList(params string[] names)
        {
            var list = new MessageInfoList
            {
                Module = Module,
                ModuleName = ModuleName,
                FileName = ModuleName,
            };

            foreach (var name in names)
            {
                list.Infos.Add(new MessageInfo { Name = name, Opcode = 0 });
            }

            return list;
        }

        /// <summary>
        /// 以一份单模块 proto 输入跑一轮「导出」（分配 + 落盘），返回带 Opcode 回写的列表。
        /// </summary>
        private static MessageInfoList RunExport(string lockPath, params string[] names)
        {
            var list = NewList(names);
            MessageIdCoordinator.AssignAndPersist(lockPath, new[] { list });
            return list;
        }

        private static string LockJson(string lockPath)
        {
            return MessageIdLockStore.SaveToString(MessageIdLockStore.Load(lockPath));
        }

        private static string NewTempLockPath()
        {
            return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".lock.json");
        }

        private static void DeleteLock(string lockPath)
        {
            if (File.Exists(lockPath))
            {
                File.Delete(lockPath);
            }

            var tmp = lockPath + ".tmp";
            if (File.Exists(tmp))
            {
                File.Delete(tmp);
            }
        }

        private static SortedDictionary<string, int> Dict(params (string Name, int SubId)[] entries)
        {
            var dict = new SortedDictionary<string, int>(StringComparer.Ordinal);
            foreach (var entry in entries)
            {
                dict[entry.Name] = entry.SubId;
            }

            return dict;
        }

        /// <summary>
        /// 显式构造期望 lock，配合 <see cref="MessageIdLockStore.SaveToString"/> 做字节级断言。
        /// </summary>
        private static string ExpectedJson(SortedDictionary<string, int> messages, SortedDictionary<string, int> retired)
        {
            var entry = new ModuleEntry { ModuleName = ModuleName };
            foreach (var kv in messages)
            {
                entry.Messages[kv.Key] = kv.Value;
            }

            foreach (var kv in retired)
            {
                entry.Retired[kv.Key] = kv.Value;
            }

            var lockData = MessageIdLock.CreateEmpty();
            lockData.Modules[ModuleKey] = entry;
            return MessageIdLockStore.SaveToString(lockData);
        }

        /// <summary>
        /// 基线_三消息首次导出_生成快照S1：场景 1（基线），一份 3 消息的模块首次导出 → lock 快照 S1。
        /// </summary>
        [Fact]
        public void Baseline_ThreeMessageFirstExport_GeneratesSnapshotS1()
        {
            var path = NewTempLockPath();

            try
            {
                var baseline = RunExport(path, "ReqA", "ReqB", "ReqC");

                // 首次分配：起点 10，按输入顺序递增
                Assert.Equal(10, baseline.Infos[0].Opcode);
                Assert.Equal(11, baseline.Infos[1].Opcode);
                Assert.Equal(12, baseline.Infos[2].Opcode);

                // S1 与显式构造的期望 lock 字节级一致
                var expected = ExpectedJson(
                    Dict(("ReqA", 10), ("ReqB", 11), ("ReqC", 12)),
                    Dict());
                Assert.Equal(expected, LockJson(path));
            }
            finally
            {
                DeleteLock(path);
            }
        }

        /// <summary>
        /// 变异A_中间插入_老消息Opcode不变_新消息Max加一：场景 2（变异 A 中间插入），在 S1 基础上于中间插一条新消息。
        /// 老消息 Opcode 与 S1 完全一致；新消息 = max+1，而不是插入位置的行序号。
        /// </summary>
        [Fact]
        public void MutationA_InsertInMiddle_OldOpcodesUnchanged_NewMessageGetsMaxPlusOne()
        {
            var path = NewTempLockPath();

            try
            {
                RunExport(path, "ReqA", "ReqB", "ReqC"); // S1: ReqA=10, ReqB=11, ReqC=12

                var mutated = RunExport(path, "ReqA", "ReqNew", "ReqB", "ReqC");

                // 老消息三个 Opcode 与 S1 完全一致
                Assert.Equal(10, mutated.Infos[0].Opcode);
                Assert.Equal(11, mutated.Infos[2].Opcode);
                Assert.Equal(12, mutated.Infos[3].Opcode);

                // 新消息 = max+1 = 13，而不是插入位置 2 上的 11/12 平移
                Assert.Equal(13, mutated.Infos[1].Opcode);

                var expected = ExpectedJson(
                    Dict(("ReqA", 10), ("ReqB", 11), ("ReqC", 12), ("ReqNew", 13)),
                    Dict());
                Assert.Equal(expected, LockJson(path));
            }
            finally
            {
                DeleteLock(path);
            }
        }

        /// <summary>
        /// 变异B_整体重排_所有Opcode与基线一致：场景 3（变异 B 重排），消息顺序整体倒过来，
        /// 所有 Opcode 与 S1 一致，lock 字节级不变。
        /// </summary>
        [Fact]
        public void MutationB_FullReorder_AllOpcodesMatchBaseline()
        {
            var path = NewTempLockPath();

            try
            {
                RunExport(path, "ReqA", "ReqB", "ReqC"); // S1
                var s1 = LockJson(path);

                var mutated = RunExport(path, "ReqC", "ReqB", "ReqA");

                // Opcode 跟着消息名走，不跟着行序走
                Assert.Equal(12, mutated.Infos[0].Opcode);
                Assert.Equal(11, mutated.Infos[1].Opcode);
                Assert.Equal(10, mutated.Infos[2].Opcode);

                // 重排不产生任何新增条目，lock 与 S1 字节级一致
                Assert.Equal(s1, LockJson(path));
            }
            finally
            {
                DeleteLock(path);
            }
        }

        /// <summary>
        /// 变异C_删除中间消息_进Retired_号永不回收：场景 4（变异 C 删除），删掉中间一条 → 它进 Retired，
        /// 号永不回收（后续新消息跳过该号）。
        /// </summary>
        [Fact]
        public void MutationC_DeleteMiddleMessage_MovedToRetired_NeverReused()
        {
            var path = NewTempLockPath();

            try
            {
                RunExport(path, "ReqA", "ReqB", "ReqC"); // S1: ReqA=10, ReqB=11, ReqC=12

                var mutated = RunExport(path, "ReqA", "ReqC");

                // 老消息沿用，删掉的 ReqB 不得引起 ReqC 平移到 11
                Assert.Equal(10, mutated.Infos[0].Opcode);
                Assert.Equal(12, mutated.Infos[1].Opcode);

                // ReqB 进 Retired，保留 11
                var afterDelete = MessageIdLockStore.Load(path);
                Assert.Equal(11, afterDelete.Modules[ModuleKey].Retired["ReqB"]);

                // 后续新消息跳过 11：ReqD = max(12)+1 = 13
                var afterAdd = RunExport(path, "ReqA", "ReqC", "ReqD");
                Assert.Equal(13, afterAdd.Infos[2].Opcode);

                // 已占用号集合：11 仍被 Retired 占着，新消息拿不到
                var occupied = MessageIdAllocator.OccupiedSubIds(MessageIdLockStore.Load(path), ModuleKey);
                Assert.Equal(new HashSet<int> { 10, 11, 12, 13 }, occupied);
            }
            finally
            {
                DeleteLock(path);
            }
        }

        /// <summary>
        /// 变异D_重命名_新名拿新号_旧名进Retired：场景 5（变异 D 重命名），ReqX → ReqY 语义（此处 ReqB → ReqBNew）：
        /// 新名拿新号（max+1），旧名进 Retired，旧号永不回收。
        /// </summary>
        [Fact]
        public void MutationD_Rename_NewNameGetsNewSubId_OldNameToRetired()
        {
            var path = NewTempLockPath();

            try
            {
                RunExport(path, "ReqA", "ReqB", "ReqC"); // S1: ReqA=10, ReqB=11, ReqC=12

                var mutated = RunExport(path, "ReqA", "ReqBNew", "ReqC");

                // 新名 = max+1 = 13；老消息不动
                Assert.Equal(13, mutated.Infos[1].Opcode);
                Assert.Equal(10, mutated.Infos[0].Opcode);
                Assert.Equal(12, mutated.Infos[2].Opcode);

                var reloaded = MessageIdLockStore.Load(path);
                Assert.Equal(13, reloaded.Modules[ModuleKey].Messages["ReqBNew"]);

                // 旧名进 Retired，保留 11，永不回收
                Assert.Equal(11, reloaded.Modules[ModuleKey].Retired["ReqB"]);

                // 后续新消息跳过 11：ReqD = max(13)+1 = 14
                var afterAdd = RunExport(path, "ReqA", "ReqBNew", "ReqC", "ReqD");
                Assert.Equal(14, afterAdd.Infos[3].Opcode);
            }
            finally
            {
                DeleteLock(path);
            }
        }

        /// <summary>
        /// 回归_变异后重跑基线_老条目字节级不变：场景 6（回归），对 A/B/C/D 每个变异（各自从 S1 重新开始），
        /// 变异后再跑一遍原基线输入。
        /// 断言：S1 的每条老条目 SubId 保持不变（字节级）；lock 与「S1 + 变异新增条目」的期望值一致。
        /// </summary>
        [Fact]
        public void Regression_RerunBaselineAfterMutation_OldEntriesByteIdentical()
        {
            // 变异 A：插入 ReqNew（=13）后重跑基线。ReqNew 不在基线输入中 → 标记进 Retired；
            // 老条目 ReqA/ReqB/ReqC 全部沿用，S1 三条记录原值不动。
            AssertMutationRegression(
                p => { RunExport(p, "ReqA", "ReqNew", "ReqB", "ReqC"); },
                ExpectedJson(
                    Dict(("ReqA", 10), ("ReqB", 11), ("ReqC", 12)),
                    Dict(("ReqNew", 13))));

            // 变异 B：重排后重跑基线 → lock 与 S1 完全一致（无新增条目）
            AssertMutationRegression(
                p => { RunExport(p, "ReqC", "ReqB", "ReqA"); },
                ExpectedJson(
                    Dict(("ReqA", 10), ("ReqB", 11), ("ReqC", 12)),
                    Dict()));

            // 变异 C：删 ReqB（Retired 11）→ 加 ReqD（=13 跳过 11）→ 重跑基线。
            // ReqB 回到基线输入 → 沿用回 11（恢复为活跃态，从 Retired 移到 Messages）。
            // ReqD 不在基线输入中 → 留在 Retired。
            AssertMutationRegression(
                p =>
                {
                    RunExport(p, "ReqA", "ReqC");
                    RunExport(p, "ReqA", "ReqC", "ReqD");
                },
                ExpectedJson(
                    Dict(("ReqA", 10), ("ReqB", 11), ("ReqC", 12)),
                    Dict(("ReqD", 13))));

            // 变异 D：ReqB → ReqBNew（=13，ReqB 进 Retired 11）→ 重跑基线。
            // ReqB 回到基线输入 → 沿用回 11（恢复为活跃态）；ReqBNew 不在基线输入中 → 留在 Retired。
            AssertMutationRegression(
                p => { RunExport(p, "ReqA", "ReqBNew", "ReqC"); },
                ExpectedJson(
                    Dict(("ReqA", 10), ("ReqB", 11), ("ReqC", 12)),
                    Dict(("ReqBNew", 13))));
        }

        /// <summary>
        /// 回归场景的公共流程：重建 S1 → 应用变异 → 重跑原基线输入 → 三层断言。
        /// </summary>
        private static void AssertMutationRegression(Action<string> applyMutation, string expectedJsonAfterRebaseline)
        {
            var path = NewTempLockPath();

            try
            {
                // 重建 S1（基线导出是确定性的，与首次生成字节级一致）
                RunExport(path, "ReqA", "ReqB", "ReqC");
                var s1 = LockJson(path);
                Assert.Equal(
                    ExpectedJson(Dict(("ReqA", 10), ("ReqB", 11), ("ReqC", 12)), Dict()),
                    s1);

                // 应用变异
                applyMutation(path);

                // 重跑原基线输入
                var rebaseline = RunExport(path, "ReqA", "ReqB", "ReqC");

                // 断言 1：三条基线消息的 Opcode 与 S1 完全一致
                Assert.Equal(10, rebaseline.Infos[0].Opcode);
                Assert.Equal(11, rebaseline.Infos[1].Opcode);
                Assert.Equal(12, rebaseline.Infos[2].Opcode);

                // 断言 2：lock 与「S1 + 变异新增条目」字节级一致
                Assert.Equal(expectedJsonAfterRebaseline, LockJson(path));

                // 断言 3：S1 的每条老条目 SubId 在最终 lock 中原值保留（Retired 也算保留）
                var final = MessageIdLockStore.Load(path);
                var entry = final.Modules[ModuleKey];
                Assert.Equal(10, entry.Messages["ReqA"]);
                Assert.Equal(11, entry.Messages.ContainsKey("ReqB") ? entry.Messages["ReqB"] : entry.Retired["ReqB"]);
                Assert.Equal(12, entry.Messages["ReqC"]);
            }
            finally
            {
                DeleteLock(path);
            }
        }
    }
}
