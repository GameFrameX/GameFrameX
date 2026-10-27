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
    public sealed partial class UIProductionBuildingCard : FUI
    {
        public const string UIPackageName = "UIProduction";
        public const string UIResName = "UIProductionBuildingCard";
        public const string URL = "ui://prod1001proda";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GImage m_card_panel { get; private set; }
		public GImage m_card_top_left_decoration { get; private set; }
		public GImage m_card_top_divider { get; private set; }
		public GImage m_card_bottom_divider { get; private set; }
		public GTextField m_title { get; private set; }
		public UIProductionWorkerQuantityControl m_quantity { get; private set; }
		public GLoader m_building_image { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIProductionBuildingCard CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIProductionBuildingCard Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIProductionBuildingCard>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIProductionBuildingCard GetFormPool(GObject go)
        {
            var fui = go.Get<UIProductionBuildingCard>();
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
				m_card_panel = (GImage)com.GetChild("card_panel");
				m_card_top_left_decoration = (GImage)com.GetChild("card_top_left_decoration");
				m_card_top_divider = (GImage)com.GetChild("card_top_divider");
				m_card_bottom_divider = (GImage)com.GetChild("card_bottom_divider");
				m_title = (GTextField)com.GetChild("title");
				m_quantity = UIProductionWorkerQuantityControl.Create(com.GetChild("quantity"));
				m_building_image = (GLoader)com.GetChild("building_image");
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
			m_card_panel = null;
			m_card_top_left_decoration = null;
			m_card_top_divider = null;
			m_card_bottom_divider = null;
			m_title = null;
			m_quantity = null;
			m_building_image = null;
            self = null;
        }

        private UIProductionBuildingCard(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif