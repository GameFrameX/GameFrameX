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
    public sealed partial class UIProduction : FUI
    {
        public const string UIPackageName = "UIProduction";
        public const string UIResName = "UIProduction";
        public const string URL = "ui://prod1001prod1";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public Controller m_tab { get; private set; }
		public Controller m_window { get; private set; }
		public GGraph m_screen_base_image { get; private set; }
		public UIProductionScreenChrome m_screen_chrome { get; private set; }
		public UIProductionBuildingsPanel m_buildings_panel { get; private set; }
		public UIProductionProductionLinesPanel m_production_lines_panel { get; private set; }
		public GGraph m_modal_dim_overlay { get; private set; }
		public UIProductionCreateLineWindowFrame m_create_line_window_frame { get; private set; }
		public UIProductionCreateLineContent m_create_line_content { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIProduction CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIProduction Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIProduction>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIProduction GetFormPool(GObject go)
        {
            var fui = go.Get<UIProduction>();
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
				m_tab = com.GetController("tab");
				m_window = com.GetController("window");
				m_screen_base_image = (GGraph)com.GetChild("screen_base_image");
				m_screen_chrome = UIProductionScreenChrome.Create(com.GetChild("screen_chrome"));
				m_buildings_panel = UIProductionBuildingsPanel.Create(com.GetChild("buildings_panel"));
				m_production_lines_panel = UIProductionProductionLinesPanel.Create(com.GetChild("production_lines_panel"));
				m_modal_dim_overlay = (GGraph)com.GetChild("modal_dim_overlay");
				m_create_line_window_frame = UIProductionCreateLineWindowFrame.Create(com.GetChild("create_line_window_frame"));
				m_create_line_content = UIProductionCreateLineContent.Create(com.GetChild("create_line_content"));
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
			m_tab = null;
			m_window = null;
			m_screen_base_image = null;
			m_screen_chrome = null;
			m_buildings_panel = null;
			m_production_lines_panel = null;
			m_modal_dim_overlay = null;
			m_create_line_window_frame = null;
			m_create_line_content = null;
            self = null;
        }

        private UIProduction(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif