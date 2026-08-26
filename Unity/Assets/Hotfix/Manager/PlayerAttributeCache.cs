// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Collections.Generic;
using Hotfix.Events;

namespace Hotfix.Manager
{
    /// <summary>
    /// 玩家属性缓存。纯逻辑对象，不依赖网络/事件框架，便于单元测试覆盖快照覆盖、增量去重等语义。
    /// 核心属性值统一使用 long 存储。
    /// </summary>
    public sealed class PlayerAttributeCache
    {
        private readonly Dictionary<int, long> _values = new Dictionary<int, long>();

        /// <summary>
        /// 当前缓存属性数量
        /// </summary>
        public int Count
        {
            get { return _values.Count; }
        }

        /// <summary>
        /// 尝试读取属性值
        /// </summary>
        /// <param name="attributeId">属性 id</param>
        /// <param name="value">属性值</param>
        /// <returns>是否命中</returns>
        public bool TryGetValue(int attributeId, out long value)
        {
            return _values.TryGetValue(attributeId, out value);
        }

        /// <summary>
        /// 将当前缓存快照写入目标集合（不清空目标，追加）。用于 UI 刷新等只读遍历。
        /// </summary>
        /// <param name="target">目标集合</param>
        public void CopyTo(IDictionary<int, long> target)
        {
            if (target == null)
            {
                return;
            }

            foreach (var pair in _values)
            {
                target[pair.Key] = pair.Value;
            }
        }

        /// <summary>
        /// 用全量快照覆盖缓存：先清空再填充，避免旧属性残留。
        /// </summary>
        /// <param name="snapshot">全量属性快照</param>
        /// <param name="oldSnapshot">可选：覆盖前的旧值快照，用于生成变化项（同步给 UI 刷新）</param>
        /// <returns>本次实际变化的属性集合（oldSnapshot 缺失时返回全部新值）</returns>
        public List<PlayerAttributeChangeItem> ReplaceAll(IDictionary<int, long> snapshot, IDictionary<int, long> oldSnapshot = null)
        {
            var changed = new List<PlayerAttributeChangeItem>();
            _values.Clear();
            if (snapshot != null)
            {
                foreach (var pair in snapshot)
                {
                    _values[pair.Key] = pair.Value;
                    long old = 0;
                    if (oldSnapshot == null || !oldSnapshot.TryGetValue(pair.Key, out old))
                    {
                        old = 0;
                    }

                    changed.Add(new PlayerAttributeChangeItem { AttributeId = pair.Key, OldValue = old, NewValue = pair.Value });
                }
            }

            return changed;
        }

        /// <summary>
        /// 应用增量变化：只更新携带的键，未携带的键不动；逐键比较，跳过相等键。
        /// </summary>
        /// <param name="changes">本次变化的属性（key:属性id，value:最终值）</param>
        /// <returns>本次实际发生变化的属性集合（已排除相同值）</returns>
        public List<PlayerAttributeChangeItem> ApplyChanges(IDictionary<int, long> changes)
        {
            var changed = new List<PlayerAttributeChangeItem>();
            if (changes == null)
            {
                return changed;
            }

            foreach (var pair in changes)
            {
                long old;
                if (!_values.TryGetValue(pair.Key, out old))
                {
                    old = 0;
                }

                if (old == pair.Value)
                {
                    // 相同值不视为变化，跳过
                    continue;
                }

                _values[pair.Key] = pair.Value;
                changed.Add(new PlayerAttributeChangeItem { AttributeId = pair.Key, OldValue = old, NewValue = pair.Value });
            }

            return changed;
        }

        /// <summary>
        /// 清空缓存（登出/切换账号时调用）
        /// </summary>
        public void Clear()
        {
            _values.Clear();
        }

        /// <summary>
        /// 获取当前缓存快照副本
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, long> GetSnapshot()
        {
            return new Dictionary<int, long>(_values);
        }
    }
}
