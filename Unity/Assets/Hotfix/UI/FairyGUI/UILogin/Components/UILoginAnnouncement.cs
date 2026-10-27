/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

#if ENABLE_UI_FAIRYGUI
using FairyGUI;
using FairyGUI.Utils;
using GameFrameX.UI.Runtime;
using GameFrameX.UI.FairyGUI.Runtime;
using GameFrameX.Runtime;
using UnityEngine;

namespace Hotfix.UI
{
    [OptionUIConfig(UIPackageName)]
    public sealed partial class UILoginAnnouncement : FUI
    {
        public const string UIPackageName = "UILogin";
        public const string UIResName = "UILoginAnnouncement";
        public const string URL = "ui://f011l0h9aneks9g";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GGraph m_mask_layer { get; private set; }
		public UILoginAnnouncementContent m_text_content { get; private set; }
		public GTextField m_text_title { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UILoginAnnouncement CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UILoginAnnouncement Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UILoginAnnouncement>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UILoginAnnouncement GetFormPool(GObject go)
        {
            var fui = go.Get<UILoginAnnouncement>();
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
				m_mask_layer = (GGraph)com.GetChild("mask_layer");
				m_text_content = UILoginAnnouncementContent.Create(com.GetChild("text_content"));
				m_text_title = (GTextField)com.GetChild("text_title");
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
			m_mask_layer = null;
			m_text_content = null;
			m_text_title = null;
            self = null;
        }

        private UILoginAnnouncement(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif