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
    public sealed partial class UIProductionProductionLineCard : FUI
    {
        public const string UIPackageName = "UIProduction";
        public const string UIResName = "UIProductionProductionLineCard";
        public const string URL = "ui://prod1001prodh";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GImage m_deco_cardpanel { get; private set; }
		public GImage m_deco_cardcornerdecoration { get; private set; }
		public GImage m_deco_carddivider { get; private set; }
		public GTextField m_production_line_name { get; private set; }
		public UIProductionProductionLineProductLabel m_product_label { get; private set; }
		public UIProductionEditLineButton m_edit_line_button { get; private set; }
		public UIProductionDeleteLineButton m_delete_line_button { get; private set; }
		public GTextField m_output_label { get; private set; }
		public GTextField m_output_rate { get; private set; }
		public GTextField m_inventory_value { get; private set; }
		public UIProductionOutputProductLabel m_output_product_label { get; private set; }
		public GTextField m_worker_allocation_label { get; private set; }
		public UIProductionWorkerQuantityControl m_quantity { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIProductionProductionLineCard CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIProductionProductionLineCard Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIProductionProductionLineCard>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIProductionProductionLineCard GetFormPool(GObject go)
        {
            var fui = go.Get<UIProductionProductionLineCard>();
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
				m_deco_cardpanel = (GImage)com.GetChild("deco_cardpanel");
				m_deco_cardcornerdecoration = (GImage)com.GetChild("deco_cardcornerdecoration");
				m_deco_carddivider = (GImage)com.GetChild("deco_carddivider");
				m_production_line_name = (GTextField)com.GetChild("production_line_name");
				m_product_label = UIProductionProductionLineProductLabel.Create(com.GetChild("product_label"));
				m_edit_line_button = UIProductionEditLineButton.Create(com.GetChild("edit_line_button"));
				m_delete_line_button = UIProductionDeleteLineButton.Create(com.GetChild("delete_line_button"));
				m_output_label = (GTextField)com.GetChild("output_label");
				m_output_rate = (GTextField)com.GetChild("output_rate");
				m_inventory_value = (GTextField)com.GetChild("inventory_value");
				m_output_product_label = UIProductionOutputProductLabel.Create(com.GetChild("output_product_label"));
				m_worker_allocation_label = (GTextField)com.GetChild("worker_allocation_label");
				m_quantity = UIProductionWorkerQuantityControl.Create(com.GetChild("quantity"));
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
			m_deco_cardpanel = null;
			m_deco_cardcornerdecoration = null;
			m_deco_carddivider = null;
			m_production_line_name = null;
			m_product_label = null;
			m_edit_line_button = null;
			m_delete_line_button = null;
			m_output_label = null;
			m_output_rate = null;
			m_inventory_value = null;
			m_output_product_label = null;
			m_worker_allocation_label = null;
			m_quantity = null;
            self = null;
        }

        private UIProductionProductionLineCard(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif