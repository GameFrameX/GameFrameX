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
    public sealed partial class UIProductionProductionLinesPanel : FUI
    {
        public const string UIPackageName = "UIProduction";
        public const string UIResName = "UIProductionProductionLinesPanel";
        public const string URL = "ui://prod1001prodg";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GImage m_deco_descriptionouterpanel { get; private set; }
		public GImage m_deco_descriptionheaderbackground { get; private set; }
		public GImage m_deco_descriptionbodybackground { get; private set; }
		public GImage m_deco_descriptionrightcornerdecoration { get; private set; }
		public GImage m_deco_descriptionleftcornerdecoration { get; private set; }
		public GTextField m_description_heading { get; private set; }
		public GTextField m_description_body { get; private set; }
		public GImage m_deco_availableworkersbackground { get; private set; }
		public GTextField m_available_workers { get; private set; }
		public GList m_line_list { get; private set; }
		public UIProductionEmptyProductionLineCard m_empty_line_card { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIProductionProductionLinesPanel CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIProductionProductionLinesPanel Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIProductionProductionLinesPanel>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIProductionProductionLinesPanel GetFormPool(GObject go)
        {
            var fui = go.Get<UIProductionProductionLinesPanel>();
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
				m_deco_descriptionouterpanel = (GImage)com.GetChild("deco_descriptionouterpanel");
				m_deco_descriptionheaderbackground = (GImage)com.GetChild("deco_descriptionheaderbackground");
				m_deco_descriptionbodybackground = (GImage)com.GetChild("deco_descriptionbodybackground");
				m_deco_descriptionrightcornerdecoration = (GImage)com.GetChild("deco_descriptionrightcornerdecoration");
				m_deco_descriptionleftcornerdecoration = (GImage)com.GetChild("deco_descriptionleftcornerdecoration");
				m_description_heading = (GTextField)com.GetChild("description_heading");
				m_description_body = (GTextField)com.GetChild("description_body");
				m_deco_availableworkersbackground = (GImage)com.GetChild("deco_availableworkersbackground");
				m_available_workers = (GTextField)com.GetChild("available_workers");
				m_line_list = (GList)com.GetChild("line_list");
				m_empty_line_card = UIProductionEmptyProductionLineCard.Create(com.GetChild("empty_line_card"));
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
			m_deco_descriptionouterpanel = null;
			m_deco_descriptionheaderbackground = null;
			m_deco_descriptionbodybackground = null;
			m_deco_descriptionrightcornerdecoration = null;
			m_deco_descriptionleftcornerdecoration = null;
			m_description_heading = null;
			m_description_body = null;
			m_deco_availableworkersbackground = null;
			m_available_workers = null;
			m_line_list = null;
			m_empty_line_card = null;
            self = null;
        }

        private UIProductionProductionLinesPanel(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif