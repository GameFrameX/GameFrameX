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
    public sealed partial class UIMain : FUI
    {
        public const string UIPackageName = "UIMain";
        public const string UIResName = "UIMain";
        public const string URL = "ui://q9u97yzfxws70";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GLoader m_bg { get; private set; }
		public GButton m_bag_button { get; private set; }
		public GButton m_room_button { get; private set; }
		public GLoader m_player_icon { get; private set; }
		public GTextField m_player_name { get; private set; }
		public GTextField m_player_level { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UIMain CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UniTask<UIMain> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UIMain> tcs = new UniTaskCompletionSource<UIMain>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go));
            });
            return tcs.Task;
        }

        public static UIMain Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIMain>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIMain GetFormPool(GObject go)
        {
            var fui = go.Get<UIMain>();
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
				m_bg = (GLoader)com.GetChild("bg");
				m_bag_button = (GButton)com.GetChild("bag_button");
				m_room_button = (GButton)com.GetChild("room_button");
				m_player_icon = (GLoader)com.GetChild("player_icon");
				m_player_name = (GTextField)com.GetChild("player_name");
				m_player_level = (GTextField)com.GetChild("player_level");
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
			m_bag_button = null;
			m_room_button = null;
			m_player_icon = null;
			m_player_name = null;
			m_player_level = null;
            self = null;            
        }

        private UIMain(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif