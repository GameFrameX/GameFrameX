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
    public sealed partial class UIRoomListItem : FUI
    {
        public const string UIPackageName = "UIRoom";
        public const string UIResName = "UIRoomListItem";
        public const string URL = "ui://rpsroom1roomi1";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GButton self { get; private set; }

		public GGraph m_bg { get; private set; }
		public GTextField m_room_name { get; private set; }
		public GTextField m_room_status { get; private set; }
		public UIRoomButton m_join_button { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UIRoomListItem CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UniTask<UIRoomListItem> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UIRoomListItem> tcs = new UniTaskCompletionSource<UIRoomListItem>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go));
            });
            return tcs.Task;
        }

        public static UIRoomListItem Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIRoomListItem>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIRoomListItem GetFormPool(GObject go)
        {
            var fui = go.Get<UIRoomListItem>();
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

            self = (GButton)GObject;
            self.Add(this);
            
            var com = GObject.asCom;
            if (com != null)
            {
				m_bg = (GGraph)com.GetChild("bg");
				m_room_name = (GTextField)com.GetChild("room_name");
				m_room_status = (GTextField)com.GetChild("room_status");
				m_join_button = UIRoomButton.Create(com.GetChild("join_button"));
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
			m_room_name = null;
			m_room_status = null;
			m_join_button = null;
            self = null;            
        }

        private UIRoomListItem(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif