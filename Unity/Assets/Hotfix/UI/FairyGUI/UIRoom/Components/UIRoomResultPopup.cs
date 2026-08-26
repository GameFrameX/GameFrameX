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
    public sealed partial class UIRoomResultPopup : FUI
    {
        public const string UIPackageName = "UIRoom";
        public const string UIResName = "UIRoomResultPopup";
        public const string URL = "ui://rpsroom1reslt1";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GGraph m_mask { get; private set; }
		public GGraph m_window_bg { get; private set; }
		public GTextField m_title_text { get; private set; }
		public GTextField m_result_text { get; private set; }
		public GTextField m_round_text { get; private set; }
		public GGraph m_self_bg { get; private set; }
		public GGraph m_opponent_bg { get; private set; }
		public GTextField m_self_name_text { get; private set; }
		public GTextField m_opponent_name_text { get; private set; }
		public GTextField m_self_gesture_text { get; private set; }
		public GTextField m_opponent_gesture_text { get; private set; }
		public UIRoomButton m_restart_button { get; private set; }
		public UIRoomButton m_close_button { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UIRoomResultPopup CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UniTask<UIRoomResultPopup> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UIRoomResultPopup> tcs = new UniTaskCompletionSource<UIRoomResultPopup>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go));
            });
            return tcs.Task;
        }

        public static UIRoomResultPopup Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIRoomResultPopup>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIRoomResultPopup GetFormPool(GObject go)
        {
            var fui = go.Get<UIRoomResultPopup>();
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
				m_mask = (GGraph)com.GetChild("mask");
				m_window_bg = (GGraph)com.GetChild("window_bg");
				m_title_text = (GTextField)com.GetChild("title_text");
				m_result_text = (GTextField)com.GetChild("result_text");
				m_round_text = (GTextField)com.GetChild("round_text");
				m_self_bg = (GGraph)com.GetChild("self_bg");
				m_opponent_bg = (GGraph)com.GetChild("opponent_bg");
				m_self_name_text = (GTextField)com.GetChild("self_name_text");
				m_opponent_name_text = (GTextField)com.GetChild("opponent_name_text");
				m_self_gesture_text = (GTextField)com.GetChild("self_gesture_text");
				m_opponent_gesture_text = (GTextField)com.GetChild("opponent_gesture_text");
				m_restart_button = UIRoomButton.Create(com.GetChild("restart_button"));
				m_close_button = UIRoomButton.Create(com.GetChild("close_button"));
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
			m_mask = null;
			m_window_bg = null;
			m_title_text = null;
			m_result_text = null;
			m_round_text = null;
			m_self_bg = null;
			m_opponent_bg = null;
			m_self_name_text = null;
			m_opponent_name_text = null;
			m_self_gesture_text = null;
			m_opponent_gesture_text = null;
			m_restart_button = null;
			m_close_button = null;
            self = null;            
        }

        private UIRoomResultPopup(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif