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
    public sealed partial class UIProductionProductionLineProductLabel : FUI
    {
        public const string UIPackageName = "UIProduction";
        public const string UIResName = "UIProductionProductionLineProductLabel";
        public const string URL = "ui://prod1001prodi";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GImage m_deco_productlabelbackground { get; private set; }
		public GTextField m_title { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIProductionProductionLineProductLabel CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIProductionProductionLineProductLabel Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIProductionProductionLineProductLabel>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIProductionProductionLineProductLabel GetFormPool(GObject go)
        {
            var fui = go.Get<UIProductionProductionLineProductLabel>();
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
				m_deco_productlabelbackground = (GImage)com.GetChild("deco_productlabelbackground");
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
			m_deco_productlabelbackground = null;
			m_title = null;
            self = null;
        }

        private UIProductionProductionLineProductLabel(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif