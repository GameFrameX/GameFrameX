// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Collections.Generic;
using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;

namespace Hotfix.Events
{
    /// <summary>
    /// 玩家属性变化项
    /// </summary>
    public struct PlayerAttributeChangeItem
    {
        /// <summary>
        /// 属性 id
        /// </summary>
        public int AttributeId;

        /// <summary>
        /// 旧值
        /// </summary>
        public long OldValue;

        /// <summary>
        /// 新值
        /// </summary>
        public long NewValue;
    }

    /// <summary>
    /// 玩家属性变化事件。仅承载本次实际发生变化的属性集合；无变化时不派发。
    /// </summary>
    public sealed class PlayerAttributeChangedEventArgs : GameEventArgs
    {
        public static readonly string EventId = typeof(PlayerAttributeChangedEventArgs).FullName;

        private readonly List<PlayerAttributeChangeItem> _changedItems = new List<PlayerAttributeChangeItem>();

        /// <summary>
        /// 本次实际发生变化的属性集合
        /// </summary>
        public IList<PlayerAttributeChangeItem> ChangedItems
        {
            get { return _changedItems; }
        }

        public override string Id
        {
            get { return EventId; }
        }

        public override void Clear()
        {
            _changedItems.Clear();
        }

        /// <summary>
        /// 创建玩家属性变化事件
        /// </summary>
        /// <param name="changedItems">实际发生变化的属性集合</param>
        /// <returns></returns>
        public static PlayerAttributeChangedEventArgs Create(IList<PlayerAttributeChangeItem> changedItems)
        {
            var eventArgs = ReferencePool.Acquire<PlayerAttributeChangedEventArgs>();
            if (changedItems != null)
            {
                eventArgs._changedItems.AddRange(changedItems);
            }

            return eventArgs;
        }
    }
}
