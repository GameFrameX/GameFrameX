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
    public sealed partial class UICommonGoodItem : FUI
    {
        public const string UIPackageName = "UICommon";
        public const string UIResName = "UICommonGoodItem";
        public const string URL = "ui://ats3vms372ce2u";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GButton self { get; private set; }

		public GLoader m_bg { get; private set; }
		public GLoader m_gift { get; private set; }
		public GTextField m_number { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UICommonGoodItem CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UICommonGoodItem Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UICommonGoodItem>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UICommonGoodItem GetFormPool(GObject go)
        {
            var fui = go.Get<UICommonGoodItem>();
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
				m_bg = (GLoader)com.GetChild("bg");
				m_gift = (GLoader)com.GetChild("gift");
				m_number = (GTextField)com.GetChild("number");
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
			m_bg = null;
			m_gift = null;
			m_number = null;
            self = null;
        }

        private UICommonGoodItem(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif