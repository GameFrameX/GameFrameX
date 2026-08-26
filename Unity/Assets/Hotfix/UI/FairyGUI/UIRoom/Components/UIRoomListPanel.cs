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
    public sealed partial class UIRoomListPanel : FUI
    {
        public const string UIPackageName = "UIRoom";
        public const string UIResName = "UIRoomListPanel";
        public const string URL = "ui://rpsroom1listp1";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GGraph m_mask { get; private set; }
		public GGraph m_window_bg { get; private set; }
		public GTextField m_title_text { get; private set; }
		public GTextField m_summary_text { get; private set; }
		public UIRoomButton m_refresh_button { get; private set; }
		public UIRoomButton m_create_button { get; private set; }
		public UIRoomButton m_close_button { get; private set; }
		public GGraph m_list_bg { get; private set; }
		public GTextField m_list_title { get; private set; }
		public GList m_room_list { get; private set; }
		public GTextField m_tips_text { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UIRoomListPanel CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UniTask<UIRoomListPanel> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UIRoomListPanel> tcs = new UniTaskCompletionSource<UIRoomListPanel>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go));
            });
            return tcs.Task;
        }

        public static UIRoomListPanel Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIRoomListPanel>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIRoomListPanel GetFormPool(GObject go)
        {
            var fui = go.Get<UIRoomListPanel>();
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
				m_summary_text = (GTextField)com.GetChild("summary_text");
				m_refresh_button = UIRoomButton.Create(com.GetChild("refresh_button"));
				m_create_button = UIRoomButton.Create(com.GetChild("create_button"));
				m_close_button = UIRoomButton.Create(com.GetChild("close_button"));
				m_list_bg = (GGraph)com.GetChild("list_bg");
				m_list_title = (GTextField)com.GetChild("list_title");
				m_room_list = (GList)com.GetChild("room_list");
				m_tips_text = (GTextField)com.GetChild("tips_text");
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
			m_summary_text = null;
			m_refresh_button = null;
			m_create_button = null;
			m_close_button = null;
			m_list_bg = null;
			m_list_title = null;
			m_room_list = null;
			m_tips_text = null;
            self = null;            
        }

        private UIRoomListPanel(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif