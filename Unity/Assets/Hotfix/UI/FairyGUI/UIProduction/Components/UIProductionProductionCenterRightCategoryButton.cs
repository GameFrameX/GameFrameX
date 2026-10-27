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
    public sealed partial class UIProductionProductionCenterRightCategoryButton : FUI
    {
        public const string UIPackageName = "UIProduction";
        public const string UIResName = "UIProductionProductionCenterRightCategoryButton";
        public const string URL = "ui://prod1001prodz";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GButton self { get; private set; }

		public GImage m_production_center_right_category_border { get; private set; }
		public GImage m_up { get; private set; }
		public GImage m_production_center_right_category_decoration { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIProductionProductionCenterRightCategoryButton CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIProductionProductionCenterRightCategoryButton Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIProductionProductionCenterRightCategoryButton>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIProductionProductionCenterRightCategoryButton GetFormPool(GObject go)
        {
            var fui = go.Get<UIProductionProductionCenterRightCategoryButton>();
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

            self = (GButton)GObject;
            self.Add(this);
            
            var com = GObject.asCom;
            if (com != null)
            {
				m_production_center_right_category_border = (GImage)com.GetChild("production_center_right_category_border");
				m_up = (GImage)com.GetChild("up");
				m_production_center_right_category_decoration = (GImage)com.GetChild("production_center_right_category_decoration");
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
			m_production_center_right_category_border = null;
			m_up = null;
			m_production_center_right_category_decoration = null;
            self = null;
        }

        private UIProductionProductionCenterRightCategoryButton(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif