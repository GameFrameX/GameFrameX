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
    public sealed partial class UIConnectFourButton : FUI
    {
        public const string UIPackageName = "UIConnectFour";
        public const string UIResName = "UIConnectFourButton";
        public const string URL = "ui://cfour001btn001";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GButton self { get; private set; }

		public GGraph m_bg { get; private set; }


        public static UIConnectFourButton Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIConnectFourButton>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIConnectFourButton GetFormPool(GObject go)
        {
            var fui = go.Get<UIConnectFourButton>();
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
				m_bg = (GGraph)com.GetChild("bg");
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
            self = null;
        }

        private UIConnectFourButton(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif