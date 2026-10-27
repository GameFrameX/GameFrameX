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
    public sealed partial class UIProductionAdventureNavigationButton : FUI
    {
        public const string UIPackageName = "UIProduction";
        public const string UIResName = "UIProductionAdventureNavigationButton";
        public const string URL = "ui://prod1001prod6";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GButton self { get; private set; }

		public GImage m_up { get; private set; }
		public GTextField m_adventure_title_shadow { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIProductionAdventureNavigationButton CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIProductionAdventureNavigationButton Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIProductionAdventureNavigationButton>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIProductionAdventureNavigationButton GetFormPool(GObject go)
        {
            var fui = go.Get<UIProductionAdventureNavigationButton>();
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
				m_up = (GImage)com.GetChild("up");
				m_adventure_title_shadow = (GTextField)com.GetChild("adventure_title_shadow");
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
			m_up = null;
			m_adventure_title_shadow = null;
            self = null;
        }

        private UIProductionAdventureNavigationButton(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif