/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

#if ENABLE_UI_FAIRYGUI
using FairyGUI;
using FairyGUI.Utils;
using GameFrameX.UI.Runtime;
using GameFrameX.UI.FairyGUI.Runtime;
using GameFrameX.Runtime;
using UnityEngine;

namespace Unity.Startup
{
    [OptionUIConfig(UIPackageName)]
    public sealed partial class UILauncher : FUI
    {
        public const string UIPackageName = "UILauncher";
        public const string UIResName = "UILauncher";
        public const string URL = "ui://u7deosq0mw8e0";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public Controller m_IsUpgrade { get; private set; }
		public Controller m_IsDownload { get; private set; }
		public GLoader m_bg { get; private set; }
		public GTextField m_tip_text { get; private set; }
		public GProgressBar m_progress_bar { get; private set; }
		public UILauncherUpgrade m_upgrade { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UILauncher CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UILauncher Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UILauncher>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UILauncher GetFormPool(GObject go)
        {
            var fui = go.Get<UILauncher>();
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
				m_IsUpgrade = com.GetController("IsUpgrade");
				m_IsDownload = com.GetController("IsDownload");
				m_bg = (GLoader)com.GetChild("bg");
				m_tip_text = (GTextField)com.GetChild("tip_text");
				m_progress_bar = (GProgressBar)com.GetChild("progress_bar");
				m_upgrade = UILauncherUpgrade.Create(com.GetChild("upgrade"));
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
			m_IsUpgrade = null;
			m_IsDownload = null;
			m_bg = null;
			m_tip_text = null;
			m_progress_bar = null;
			m_upgrade = null;
            self = null;
        }

        private UILauncher(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif