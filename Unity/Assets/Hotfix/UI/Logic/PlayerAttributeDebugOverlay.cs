// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Collections.Generic;
using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;
using Hotfix.Events;
using Hotfix.Manager;
using Hotfix.Proto;
using UnityEngine;

namespace Hotfix.UI
{
    /// <summary>
    /// 玩家属性最小调试展示入口（IMGUI OnGUI 覆盖层）。
    /// 纯代码实现，不依赖 UI prefab / Binder / Canvas 资源，用于 Server -> Unity 端到端验证：
    /// 登录后展示九项基础属性，收到属性变化事件时刷新。
    /// 切换显示：F8。
    /// </summary>
    public class PlayerAttributeDebugOverlay : MonoBehaviour
    {
        private static PlayerAttributeDebugOverlay _instance;

        private readonly Dictionary<int, long> _snapshot = new Dictionary<int, long>();
        private readonly int[] _displayAttributes =
        {
            (int)PlayerAttributeType.Hp,
            (int)PlayerAttributeType.PhysAtk,
            (int)PlayerAttributeType.MagicAtk,
            (int)PlayerAttributeType.PhysDef,
            (int)PlayerAttributeType.MagicDef,
            (int)PlayerAttributeType.Crit,
            (int)PlayerAttributeType.CritDamage,
            (int)PlayerAttributeType.Accuracy,
            (int)PlayerAttributeType.Block,
        };

        private bool _visible = true;

        private void OnEnable()
        {
            RefreshSnapshot();
            GameApp.Event.CheckSubscribe(PlayerAttributeChangedEventArgs.EventId, OnAttributeChanged);
        }

        private void OnDisable()
        {
            GameApp.Event.Unsubscribe(PlayerAttributeChangedEventArgs.EventId, OnAttributeChanged);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                _visible = !_visible;
            }
        }

        private void OnAttributeChanged(object sender, GameEventArgs e)
        {
            RefreshSnapshot();
        }

        private void RefreshSnapshot()
        {
            _snapshot.Clear();
            PlayerAttributeManager.Instance.CopyTo(_snapshot);
        }

        private void OnGUI()
        {
            if (!_visible)
            {
                return;
            }

            const float width = 280f;
            const float lineHeight = 22f;
            float height = 30f + _displayAttributes.Length * lineHeight;

            var rect = new Rect(16f, 16f, width, height);
            GUI.Box(rect, "玩家属性调试 (F8 隐藏)");

            var inner = new Rect(rect.x + 10f, rect.y + 24f, rect.width - 20f, lineHeight);
            for (int i = 0; i < _displayAttributes.Length; i++)
            {
                var attributeId = _displayAttributes[i];
                long value;
                if (!_snapshot.TryGetValue(attributeId, out value))
                {
                    value = 0;
                }

                var label = GetAttributeName(attributeId);
                GUI.Label(inner, label + " : " + value);
                inner.y += lineHeight;
            }
        }

        private static string GetAttributeName(int attributeId)
        {
            switch ((PlayerAttributeType)attributeId)
            {
                case PlayerAttributeType.Hp:
                    return "生命";
                case PlayerAttributeType.PhysAtk:
                    return "物理攻击";
                case PlayerAttributeType.MagicAtk:
                    return "魔法攻击";
                case PlayerAttributeType.PhysDef:
                    return "物理防御";
                case PlayerAttributeType.MagicDef:
                    return "魔法防御";
                case PlayerAttributeType.Crit:
                    return "暴击";
                case PlayerAttributeType.CritDamage:
                    return "爆伤";
                case PlayerAttributeType.Accuracy:
                    return "精准";
                case PlayerAttributeType.Block:
                    return "格挡";
                default:
                    return "未知(" + attributeId + ")";
            }
        }

        /// <summary>
        /// 确保调试覆盖层存在（幂等）。登录成功后调用。
        /// </summary>
        public static void EnsureExists()
        {
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject(nameof(PlayerAttributeDebugOverlay));
            _instance = go.AddComponent<PlayerAttributeDebugOverlay>();
            Object.DontDestroyOnLoad(go);
            Log.Info("[PlayerAttribute] 调试展示入口已创建。游戏中按 F8 切换显示。");
        }
    }
}
