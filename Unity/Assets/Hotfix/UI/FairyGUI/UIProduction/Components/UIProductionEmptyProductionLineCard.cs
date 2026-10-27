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
    public sealed partial class UIProductionEmptyProductionLineCard : FUI
    {
        public const string UIPackageName = "UIProduction";
        public const string UIResName = "UIProductionEmptyProductionLineCard";
        public const string URL = "ui://prod1001prodk";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GImage m_deco_cardpanel2 { get; private set; }
		public GImage m_deco_cardcornerdecoration2 { get; private set; }
		public GTextField m_production_line_name { get; private set; }
		public GImage m_deco_editlinebutton3 { get; private set; }
		public GImage m_deco_deletelinebutton3 { get; private set; }
		public GTextField m_create_line_hint { get; private set; }
		public UIProductionCreateLineButton m_create_line_button { get; private set; }
		public GTextField m_create_line_hint_2 { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIProductionEmptyProductionLineCard CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIProductionEmptyProductionLineCard Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIProductionEmptyProductionLineCard>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIProductionEmptyProductionLineCard GetFormPool(GObject go)
        {
            var fui = go.Get<UIProductionEmptyProductionLineCard>();
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
				m_deco_cardpanel2 = (GImage)com.GetChild("deco_cardpanel2");
				m_deco_cardcornerdecoration2 = (GImage)com.GetChild("deco_cardcornerdecoration2");
				m_production_line_name = (GTextField)com.GetChild("production_line_name");
				m_deco_editlinebutton3 = (GImage)com.GetChild("deco_editlinebutton3");
				m_deco_deletelinebutton3 = (GImage)com.GetChild("deco_deletelinebutton3");
				m_create_line_hint = (GTextField)com.GetChild("create_line_hint");
				m_create_line_button = UIProductionCreateLineButton.Create(com.GetChild("create_line_button"));
				m_create_line_hint_2 = (GTextField)com.GetChild("create_line_hint");
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
			m_deco_cardpanel2 = null;
			m_deco_cardcornerdecoration2 = null;
			m_production_line_name = null;
			m_deco_editlinebutton3 = null;
			m_deco_deletelinebutton3 = null;
			m_create_line_hint = null;
			m_create_line_button = null;
			m_create_line_hint_2 = null;
            self = null;
        }

        private UIProductionEmptyProductionLineCard(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif