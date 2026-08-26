/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

#if ENABLE_UI_FAIRYGUI
using FairyGUI;
using Cysharp.Threading.Tasks;
using FairyGUI.Utils;
using GameFrameX.Entity.Runtime;
using GameFrameX.UI.Runtime;
using GameFrameX.UI.FairyGUI.Runtime;
using GameFrameX.Runtime;
using UnityEngine;

namespace Hotfix.UI
{
    public sealed partial class UIRoomPlayerItem : FUI
    {
        public const string UIPackageName = "UIRoom";
        public const string UIResName = "UIRoomPlayerItem";
        public const string URL = "ui://rpsroom1playi1";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GGraph m_bg { get; private set; }
		public GLoader m_avatar { get; private set; }
		public GTextField m_role_text { get; private set; }
		public GTextField m_seat_text { get; private set; }
		public GTextField m_state_text { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UIRoomPlayerItem CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UniTask<UIRoomPlayerItem> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UIRoomPlayerItem> tcs = new UniTaskCompletionSource<UIRoomPlayerItem>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go));
            });
            return tcs.Task;
        }

        public static UIRoomPlayerItem Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIRoomPlayerItem>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIRoomPlayerItem GetFormPool(GObject go)
        {
            var fui = go.Get<UIRoomPlayerItem>();
            if (fui == null)
            {
                fui = Create(go);
            }

            fui.IsFromPool = true;
            return fui;
        }

        protected override void InitView()
        {
            if(GObject == null)
            {
                return;
            }

            self = (GComponent)GObject;
            self.Add(this);
            
            var com = GObject.asCom;
            if (com != null)
            {
				m_bg = (GGraph)com.GetChild("bg");
				m_avatar = (GLoader)com.GetChild("avatar");
				m_role_text = (GTextField)com.GetChild("role_text");
				m_seat_text = (GTextField)com.GetChild("seat_text");
				m_state_text = (GTextField)com.GetChild("state_text");
            }
        }

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            base.Dispose();
            self.Remove();
			m_bg = null;
			m_avatar = null;
			m_role_text = null;
			m_seat_text = null;
			m_state_text = null;
            self = null;            
        }

        private UIRoomPlayerItem(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif