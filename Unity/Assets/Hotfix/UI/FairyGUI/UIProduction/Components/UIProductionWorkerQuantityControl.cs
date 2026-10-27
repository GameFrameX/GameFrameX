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
    public sealed partial class UIProductionWorkerQuantityControl : FUI
    {
        public const string UIPackageName = "UIProduction";
        public const string UIResName = "UIProductionWorkerQuantityControl";
        public const string URL = "ui://prod1001prodb";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public UIProductionDecreaseButton m_decrease_button { get; private set; }
		public UIProductionIncreaseButton m_increase_button { get; private set; }
		public GImage m_quantity_background { get; private set; }
		public GTextField m_value { get; private set; }
		public UIProductionMaximumWorkersButton m_maximum_button { get; private set; }
		public UIProductionMinimumWorkersButton m_minimum_button { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIProductionWorkerQuantityControl CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIProductionWorkerQuantityControl Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIProductionWorkerQuantityControl>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIProductionWorkerQuantityControl GetFormPool(GObject go)
        {
            var fui = go.Get<UIProductionWorkerQuantityControl>();
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
				m_decrease_button = UIProductionDecreaseButton.Create(com.GetChild("decrease_button"));
				m_increase_button = UIProductionIncreaseButton.Create(com.GetChild("increase_button"));
				m_quantity_background = (GImage)com.GetChild("quantity_background");
				m_value = (GTextField)com.GetChild("value");
				m_maximum_button = UIProductionMaximumWorkersButton.Create(com.GetChild("maximum_button"));
				m_minimum_button = UIProductionMinimumWorkersButton.Create(com.GetChild("minimum_button"));
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
			m_decrease_button = null;
			m_increase_button = null;
			m_quantity_background = null;
			m_value = null;
			m_maximum_button = null;
			m_minimum_button = null;
            self = null;
        }

        private UIProductionWorkerQuantityControl(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif