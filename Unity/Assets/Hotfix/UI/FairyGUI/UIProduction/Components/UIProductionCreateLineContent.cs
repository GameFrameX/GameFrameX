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
    public sealed partial class UIProductionCreateLineContent : FUI
    {
        public const string UIPackageName = "UIProduction";
        public const string UIResName = "UIProductionCreateLineContent";
        public const string URL = "ui://prod1001prodq";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public UIProductionProductCatalog m_product_catalog { get; private set; }
		public GTextField m_production_line_instructions { get; private set; }
		public GTextField m_production_recipe_instructions { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIProductionCreateLineContent CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIProductionCreateLineContent Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIProductionCreateLineContent>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIProductionCreateLineContent GetFormPool(GObject go)
        {
            var fui = go.Get<UIProductionCreateLineContent>();
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
				m_product_catalog = UIProductionProductCatalog.Create(com.GetChild("product_catalog"));
				m_production_line_instructions = (GTextField)com.GetChild("production_line_instructions");
				m_production_recipe_instructions = (GTextField)com.GetChild("production_recipe_instructions");
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
			m_product_catalog = null;
			m_production_line_instructions = null;
			m_production_recipe_instructions = null;
            self = null;
        }

        private UIProductionCreateLineContent(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif