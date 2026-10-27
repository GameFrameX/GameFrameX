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
    public sealed partial class UIProductionCreateLineWindowFrame : FUI
    {
        public const string UIPackageName = "UIProduction";
        public const string UIResName = "UIProductionCreateLineWindowFrame";
        public const string URL = "ui://prod1001prodo";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GImage m_window_center_decoration { get; private set; }
		public GImage m_window_outer_panel { get; private set; }
		public GImage m_window_inner_panel { get; private set; }
		public GImage m_window_top_highlight { get; private set; }
		public GImage m_window_top_highlight_dot { get; private set; }
		public GImage m_window_bottom_highlight { get; private set; }
		public GImage m_window_bottom_highlight_dot { get; private set; }
		public GImage m_title_small_star { get; private set; }
		public GImage m_title_left_star { get; private set; }
		public GImage m_title_right_star { get; private set; }
		public GImage m_title_top_star { get; private set; }
		public GImage m_title_bottom_star { get; private set; }
		public UIProductionCloseLineWindowButton m_close_button { get; private set; }
		public UIProductionCreateLineWindowTitle m_title { get; private set; }
		public GImage m_window_content_divider { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIProductionCreateLineWindowFrame CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIProductionCreateLineWindowFrame Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIProductionCreateLineWindowFrame>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIProductionCreateLineWindowFrame GetFormPool(GObject go)
        {
            var fui = go.Get<UIProductionCreateLineWindowFrame>();
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
				m_window_center_decoration = (GImage)com.GetChild("window_center_decoration");
				m_window_outer_panel = (GImage)com.GetChild("window_outer_panel");
				m_window_inner_panel = (GImage)com.GetChild("window_inner_panel");
				m_window_top_highlight = (GImage)com.GetChild("window_top_highlight");
				m_window_top_highlight_dot = (GImage)com.GetChild("window_top_highlight_dot");
				m_window_bottom_highlight = (GImage)com.GetChild("window_bottom_highlight");
				m_window_bottom_highlight_dot = (GImage)com.GetChild("window_bottom_highlight_dot");
				m_title_small_star = (GImage)com.GetChild("title_small_star");
				m_title_left_star = (GImage)com.GetChild("title_left_star");
				m_title_right_star = (GImage)com.GetChild("title_right_star");
				m_title_top_star = (GImage)com.GetChild("title_top_star");
				m_title_bottom_star = (GImage)com.GetChild("title_bottom_star");
				m_close_button = UIProductionCloseLineWindowButton.Create(com.GetChild("close_button"));
				m_title = UIProductionCreateLineWindowTitle.Create(com.GetChild("title"));
				m_window_content_divider = (GImage)com.GetChild("window_content_divider");
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
			m_window_center_decoration = null;
			m_window_outer_panel = null;
			m_window_inner_panel = null;
			m_window_top_highlight = null;
			m_window_top_highlight_dot = null;
			m_window_bottom_highlight = null;
			m_window_bottom_highlight_dot = null;
			m_title_small_star = null;
			m_title_left_star = null;
			m_title_right_star = null;
			m_title_top_star = null;
			m_title_bottom_star = null;
			m_close_button = null;
			m_title = null;
			m_window_content_divider = null;
            self = null;
        }

        private UIProductionCreateLineWindowFrame(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif