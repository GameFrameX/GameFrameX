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
    public sealed partial class UIProductionCreateLineWindowTitle : FUI
    {
        public const string UIPackageName = "UIProduction";
        public const string UIResName = "UIProductionCreateLineWindowTitle";
        public const string URL = "ui://prod1001prodp";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GImage m_window_title_background { get; private set; }
		public GTextField m_title { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIProductionCreateLineWindowTitle CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIProductionCreateLineWindowTitle Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIProductionCreateLineWindowTitle>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIProductionCreateLineWindowTitle GetFormPool(GObject go)
        {
            var fui = go.Get<UIProductionCreateLineWindowTitle>();
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
				m_window_title_background = (GImage)com.GetChild("window_title_background");
				m_title = (GTextField)com.GetChild("title");
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
			m_window_title_background = null;
			m_title = null;
            self = null;
        }

        private UIProductionCreateLineWindowTitle(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif