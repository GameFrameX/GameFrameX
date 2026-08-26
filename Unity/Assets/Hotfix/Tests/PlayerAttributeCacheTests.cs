// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Collections.Generic;
using Hotfix.Manager;
using Hotfix.Proto;
using NUnit.Framework;

namespace Hotfix.Tests
{
    [TestFixture]
    public class PlayerAttributeCacheTests
    {
        [Test]
        public void ReplaceAll_FillsCache()
        {
            var cache = new PlayerAttributeCache();
            var snapshot = new Dictionary<int, long>
            {
                { (int)PlayerAttributeType.Hp, 1000 },
                { (int)PlayerAttributeType.PhysAtk, 120 },
            };

            cache.ReplaceAll(snapshot);

            long hp;
            Assert.IsTrue(cache.TryGetValue((int)PlayerAttributeType.Hp, out hp));
            Assert.AreEqual(1000, hp);
            Assert.AreEqual(2, cache.Count);
        }

        [Test]
        public void ReplaceAll_OverwritesAndRemovesStaleKeys()
        {
            // 第一次快照：A、B 两个属性
            var cache = new PlayerAttributeCache();
            cache.ReplaceAll(new Dictionary<int, long>
            {
                { (int)PlayerAttributeType.Hp, 1 },
                { (int)PlayerAttributeType.PhysDef, 2 },
            });

            // 第二次快照只含 A，必须覆盖旧数据，B 不残留
            cache.ReplaceAll(new Dictionary<int, long>
            {
                { (int)PlayerAttributeType.Hp, 5 },
            });

            long hp;
            Assert.IsTrue(cache.TryGetValue((int)PlayerAttributeType.Hp, out hp));
            Assert.AreEqual(5, hp);

            long physDef;
            Assert.IsFalse(cache.TryGetValue((int)PlayerAttributeType.PhysDef, out physDef), "重连/重新登录快照必须覆盖旧数据，旧 key 不应残留");
            Assert.AreEqual(1, cache.Count);
        }

        [Test]
        public void ApplyChanges_OnlyTouchesCarriedKeys()
        {
            var cache = new PlayerAttributeCache();
            cache.ReplaceAll(new Dictionary<int, long>
            {
                { (int)PlayerAttributeType.Hp, 100 },
                { (int)PlayerAttributeType.PhysAtk, 50 },
            });

            // 增量只携带 Hp，未携带的 PhysAtk 必须保持不变
            cache.ApplyChanges(new Dictionary<int, long>
            {
                { (int)PlayerAttributeType.Hp, 200 },
            });

            long hp;
            Assert.IsTrue(cache.TryGetValue((int)PlayerAttributeType.Hp, out hp));
            Assert.AreEqual(200, hp);

            long physAtk;
            Assert.IsTrue(cache.TryGetValue((int)PlayerAttributeType.PhysAtk, out physAtk));
            Assert.AreEqual(50, physAtk, "增量消息不应影响未变化属性");
        }

        [Test]
        public void ApplyChanges_SameValueDoesNotReportChange()
        {
            var cache = new PlayerAttributeCache();
            cache.ReplaceAll(new Dictionary<int, long>
            {
                { (int)PlayerAttributeType.Hp, 100 },
            });

            var changed = cache.ApplyChanges(new Dictionary<int, long>
            {
                { (int)PlayerAttributeType.Hp, 100 }, // 相同值
                { (int)PlayerAttributeType.PhysAtk, 80 }, // 新增
            });

            // 相同值（Hp=100）不应出现在变化集合中
            Assert.AreEqual(1, changed.Count, "相同值更新不应视为变化");
            Assert.AreEqual((int)PlayerAttributeType.PhysAtk, changed[0].AttributeId);
            Assert.AreEqual(80, changed[0].NewValue);
        }

        [Test]
        public void ApplyChanges_EmptyOrNullReturnsNoChanges()
        {
            var cache = new PlayerAttributeCache();
            cache.ReplaceAll(new Dictionary<int, long> { { (int)PlayerAttributeType.Hp, 1 } });

            Assert.IsEmpty(cache.ApplyChanges(new Dictionary<int, long>()));
            Assert.IsEmpty(cache.ApplyChanges(null));
        }

        [Test]
        public void Clear_RemovesAllEntries()
        {
            var cache = new PlayerAttributeCache();
            cache.ReplaceAll(new Dictionary<int, long>
            {
                { (int)PlayerAttributeType.Hp, 1 },
                { (int)PlayerAttributeType.Crit, 2 },
            });

            cache.Clear();

            Assert.AreEqual(0, cache.Count);
            long v;
            Assert.IsFalse(cache.TryGetValue((int)PlayerAttributeType.Hp, out v));
        }

        [Test]
        public void ApplyChanges_NewKeyReportsOldZero()
        {
            var cache = new PlayerAttributeCache();
            var changed = cache.ApplyChanges(new Dictionary<int, long>
            {
                { (int)PlayerAttributeType.Block, 42 },
            });

            Assert.AreEqual(1, changed.Count);
            Assert.AreEqual(0, changed[0].OldValue, "新属性旧值应为 0");
            Assert.AreEqual(42, changed[0].NewValue);
        }
    }
}
