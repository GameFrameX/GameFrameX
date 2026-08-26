// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFrameX.Network.Runtime;
using GameFrameX.Runtime;
using Hotfix.Events;
using Hotfix.Proto;

namespace Hotfix.Manager
{
    /// <summary>
    /// 客户端玩家属性编号映射。数值必须与服务端 GameFrameX.Server 的 AttributeType 最终值槽保持一致。
    /// </summary>
    public enum PlayerAttributeType
    {
        Hp = 1,
        PhysAtk = 2,
        MagicAtk = 3,
        PhysDef = 4,
        MagicDef = 5,
        Crit = 6,
        CritDamage = 7,
        Accuracy = 8,
        Block = 9,
    }

    /// <summary>
    /// 玩家属性管理器。消费服务端属性快照与增量变化，维护客户端缓存并派发变化事件。
    /// 权威在服务端（GameFrameX.Server#88），客户端不拥有权威属性计算、不写服务端属性。
    /// </summary>
    public sealed class PlayerAttributeManager : GameFrameworkSingleton<PlayerAttributeManager>, IMessageHandler
    {
        private readonly PlayerAttributeCache _cache = new PlayerAttributeCache();

        /// <summary>
        /// 缓存属性数量
        /// </summary>
        public int Count
        {
            get { return _cache.Count; }
        }

        /// <summary>
        /// 尝试读取属性值
        /// </summary>
        /// <param name="attributeId">属性 id</param>
        /// <param name="value">属性值</param>
        /// <returns>是否命中</returns>
        public bool TryGetValue(int attributeId, out long value)
        {
            return _cache.TryGetValue(attributeId, out value);
        }

        /// <summary>
        /// 读取枚举类型属性值，未命中返回 0
        /// </summary>
        /// <param name="attributeType">属性类型</param>
        /// <returns>属性值</returns>
        public long GetValue(PlayerAttributeType attributeType)
        {
            long value;
            if (_cache.TryGetValue((int)attributeType, out value))
            {
                return value;
            }

            return 0;
        }

        /// <summary>
        /// 监听服务端属性完整快照通知。登录后或重连时用快照覆盖本地旧缓存。
        /// </summary>
        /// <param name="msg">属性快照通知</param>
        [MessageHandler(typeof(NotifyPlayerAttributeSync), nameof(NotifyPlayerAttributeSync))]
        private void NotifyPlayerAttributeSync(NotifyPlayerAttributeSync msg)
        {
            var snapshot = new Dictionary<int, long>();
            if (msg != null && msg.Attributes != null)
            {
                foreach (var attribute in msg.Attributes)
                {
                    snapshot[attribute.Type] = attribute.Value;
                }
            }

            var oldSnapshot = _cache.GetSnapshot();
            var changed = _cache.ReplaceAll(snapshot, oldSnapshot);
            GameApp.Event.Fire(this, PlayerAttributeChangedEventArgs.Create(changed));
        }

        /// <summary>
        /// 将当前缓存快照写入目标集合
        /// </summary>
        /// <param name="target">目标集合</param>
        public void CopyTo(IDictionary<int, long> target)
        {
            _cache.CopyTo(target);
        }

        /// <summary>
        /// 监听服务端属性增量变化通知
        /// </summary>
        /// <param name="msg">增量通知</param>
        [MessageHandler(typeof(NotifyPlayerAttributeChanged), nameof(NotifyPlayerAttributeChanged))]
        private void NotifyPlayerAttributeChanged(NotifyPlayerAttributeChanged msg)
        {
            if (msg == null)
            {
                return;
            }

            var changed = _cache.ApplyChanges(new Dictionary<int, long> { { msg.Type, msg.Value } });
            if (changed.Count <= 0)
            {
                // 无实际变化，不重复派发
                return;
            }

            GameApp.Event.Fire(this, PlayerAttributeChangedEventArgs.Create(changed));
        }

        /// <summary>
        /// 请求玩家属性快照。服务端返回后清空旧缓存再填充（重连/重新登录快照覆盖）。
        /// </summary>
        public UniTask RequestGetPlayerAttribute()
        {
            // 服务端在登录成功后主动推送 NotifyPlayerAttributeSync；这里保留 awaitable 入口兼容登录流程。
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 清空缓存。登出/切换账号时调用，保证事件订阅不会引用旧玩家数据。
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }

        /// <summary>
        /// 由于是单例对象。所以在初始化的时候自动调用一次注册消息
        /// </summary>
        public PlayerAttributeManager()
        {
            Register();
        }

        /// <summary>
        /// 注册消息。请勿多次调用
        /// </summary>
        public void Register()
        {
            ProtoMessageHandler.Add(this);
        }
    }
}
