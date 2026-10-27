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
    public sealed partial class UIProductionScreenChrome : FUI
    {
        public const string UIPackageName = "UIProduction";
        public const string UIResName = "UIProductionScreenChrome";
        public const string URL = "ui://prod1001prod2";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GImage m_screen_background_part0 { get; private set; }
		public GImage m_screen_background_part1 { get; private set; }
		public UIProductionBackButton m_back_button { get; private set; }
		public UIProductionProductionNavigationButton m_production_navigation_button { get; private set; }
		public UIProductionAdventureNavigationButton m_adventure_navigation_button { get; private set; }
		public UIProductionTrainingNavigationButton m_training_navigation_button { get; private set; }
		public UIProductionProductionTabButton m_production_tab_button { get; private set; }
		public UIProductionProductionLinesTabButton m_production_lines_tab_button { get; private set; }
		public GTextField m_screen_title_shadow { get; private set; }
		public GTextField m_title { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIProductionScreenChrome CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIProductionScreenChrome Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIProductionScreenChrome>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIProductionScreenChrome GetFormPool(GObject go)
        {
            var fui = go.Get<UIProductionScreenChrome>();
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
				m_screen_background_part0 = (GImage)com.GetChild("screen_background_part0");
				m_screen_background_part1 = (GImage)com.GetChild("screen_background_part1");
				m_back_button = UIProductionBackButton.Create(com.GetChild("back_button"));
				m_production_navigation_button = UIProductionProductionNavigationButton.Create(com.GetChild("production_navigation_button"));
				m_adventure_navigation_button = UIProductionAdventureNavigationButton.Create(com.GetChild("adventure_navigation_button"));
				m_training_navigation_button = UIProductionTrainingNavigationButton.Create(com.GetChild("training_navigation_button"));
				m_production_tab_button = UIProductionProductionTabButton.Create(com.GetChild("production_tab_button"));
				m_production_lines_tab_button = UIProductionProductionLinesTabButton.Create(com.GetChild("production_lines_tab_button"));
				m_screen_title_shadow = (GTextField)com.GetChild("screen_title_shadow");
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
			m_screen_background_part0 = null;
			m_screen_background_part1 = null;
			m_back_button = null;
			m_production_navigation_button = null;
			m_adventure_navigation_button = null;
			m_training_navigation_button = null;
			m_production_tab_button = null;
			m_production_lines_tab_button = null;
			m_screen_title_shadow = null;
			m_title = null;
            self = null;
        }

        private UIProductionScreenChrome(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif