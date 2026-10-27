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
    public sealed partial class UIProductionBuildingsPanel : FUI
    {
        public const string UIPackageName = "UIProduction";
        public const string UIResName = "UIProductionBuildingsPanel";
        public const string URL = "ui://prod1001prod9";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GImage m_description_outer_panel { get; private set; }
		public GImage m_description_header_background { get; private set; }
		public GImage m_description_body_background { get; private set; }
		public GImage m_description_right_corner_decoration { get; private set; }
		public GImage m_description_left_corner_decoration { get; private set; }
		public GTextField m_description_heading { get; private set; }
		public GTextField m_description_body { get; private set; }
		public GImage m_available_workers_background { get; private set; }
		public GTextField m_available_workers { get; private set; }
		public UIProductionAllCategoryButton m_all_category_button { get; private set; }
		public UIProductionProductionLeftCategoryButton m_left_category_button { get; private set; }
		public UIProductionProductionCenterLeftCategoryButton m_center_left_category_button { get; private set; }
		public UIProductionProductionCenterRightCategoryButton m_center_right_category_button { get; private set; }
		public UIProductionProductionRightCategoryButton m_right_category_button { get; private set; }
		public UIProductionProductionLinesCategoryButton m_production_lines_category_button { get; private set; }
		public GList m_building_list { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIProductionBuildingsPanel CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIProductionBuildingsPanel Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIProductionBuildingsPanel>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIProductionBuildingsPanel GetFormPool(GObject go)
        {
            var fui = go.Get<UIProductionBuildingsPanel>();
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
				m_description_outer_panel = (GImage)com.GetChild("description_outer_panel");
				m_description_header_background = (GImage)com.GetChild("description_header_background");
				m_description_body_background = (GImage)com.GetChild("description_body_background");
				m_description_right_corner_decoration = (GImage)com.GetChild("description_right_corner_decoration");
				m_description_left_corner_decoration = (GImage)com.GetChild("description_left_corner_decoration");
				m_description_heading = (GTextField)com.GetChild("description_heading");
				m_description_body = (GTextField)com.GetChild("description_body");
				m_available_workers_background = (GImage)com.GetChild("available_workers_background");
				m_available_workers = (GTextField)com.GetChild("available_workers");
				m_all_category_button = UIProductionAllCategoryButton.Create(com.GetChild("all_category_button"));
				m_left_category_button = UIProductionProductionLeftCategoryButton.Create(com.GetChild("left_category_button"));
				m_center_left_category_button = UIProductionProductionCenterLeftCategoryButton.Create(com.GetChild("center_left_category_button"));
				m_center_right_category_button = UIProductionProductionCenterRightCategoryButton.Create(com.GetChild("center_right_category_button"));
				m_right_category_button = UIProductionProductionRightCategoryButton.Create(com.GetChild("right_category_button"));
				m_production_lines_category_button = UIProductionProductionLinesCategoryButton.Create(com.GetChild("production_lines_category_button"));
				m_building_list = (GList)com.GetChild("building_list");
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
			m_description_outer_panel = null;
			m_description_header_background = null;
			m_description_body_background = null;
			m_description_right_corner_decoration = null;
			m_description_left_corner_decoration = null;
			m_description_heading = null;
			m_description_body = null;
			m_available_workers_background = null;
			m_available_workers = null;
			m_all_category_button = null;
			m_left_category_button = null;
			m_center_left_category_button = null;
			m_center_right_category_button = null;
			m_right_category_button = null;
			m_production_lines_category_button = null;
			m_building_list = null;
            self = null;
        }

        private UIProductionBuildingsPanel(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif