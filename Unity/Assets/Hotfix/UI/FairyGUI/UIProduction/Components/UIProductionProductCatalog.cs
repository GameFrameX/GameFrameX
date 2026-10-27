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
    public sealed partial class UIProductionProductCatalog : FUI
    {
        public const string UIPackageName = "UIProduction";
        public const string UIResName = "UIProductionProductCatalog";
        public const string URL = "ui://prod1001prodr";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GImage m_catalog_outer_panel { get; private set; }
		public GImage m_catalog_header_background { get; private set; }
		public GImage m_catalog_body_background { get; private set; }
		public GImage m_catalog_right_corner_decoration { get; private set; }
		public GImage m_catalog_left_corner_decoration { get; private set; }
		public GImage m_era_selected_background { get; private set; }
		public GButton m_era_button_0 { get; private set; }
		public GButton m_era_button_1 { get; private set; }
		public GButton m_era_button_2 { get; private set; }
		public GButton m_era_button_3 { get; private set; }
		public GButton m_era_button_4 { get; private set; }
		public GButton m_era_button_5 { get; private set; }
		public GList m_product_list { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIProductionProductCatalog CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIProductionProductCatalog Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIProductionProductCatalog>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIProductionProductCatalog GetFormPool(GObject go)
        {
            var fui = go.Get<UIProductionProductCatalog>();
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
				m_catalog_outer_panel = (GImage)com.GetChild("catalog_outer_panel");
				m_catalog_header_background = (GImage)com.GetChild("catalog_header_background");
				m_catalog_body_background = (GImage)com.GetChild("catalog_body_background");
				m_catalog_right_corner_decoration = (GImage)com.GetChild("catalog_right_corner_decoration");
				m_catalog_left_corner_decoration = (GImage)com.GetChild("catalog_left_corner_decoration");
				m_era_selected_background = (GImage)com.GetChild("era_selected_background");
				m_era_button_0 = (GButton)com.GetChild("era_button_0");
				m_era_button_1 = (GButton)com.GetChild("era_button_1");
				m_era_button_2 = (GButton)com.GetChild("era_button_2");
				m_era_button_3 = (GButton)com.GetChild("era_button_3");
				m_era_button_4 = (GButton)com.GetChild("era_button_4");
				m_era_button_5 = (GButton)com.GetChild("era_button_5");
				m_product_list = (GList)com.GetChild("product_list");
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
			m_catalog_outer_panel = null;
			m_catalog_header_background = null;
			m_catalog_body_background = null;
			m_catalog_right_corner_decoration = null;
			m_catalog_left_corner_decoration = null;
			m_era_selected_background = null;
			m_era_button_0 = null;
			m_era_button_1 = null;
			m_era_button_2 = null;
			m_era_button_3 = null;
			m_era_button_4 = null;
			m_era_button_5 = null;
			m_product_list = null;
            self = null;
        }

        private UIProductionProductCatalog(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif