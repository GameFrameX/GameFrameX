/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using Cysharp.Threading.Tasks;
using FairyGUI.Utils;
using GameFrameX.Runtime;

namespace Game.Model
{
    public sealed partial class UILauncherUpgrade : FUI
    {
        public const string UIPackageName = "UILauncher";
        public const string UIResName = "UILauncherUpgrade";
        public const string URL = "ui://u7deosq0qew11e";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GGraph m_bg { get; private set; }
		public UILauncherEnterButton m_EnterButton { get; private set; }
		public UILauncherUpgradeContent m_TextContent { get; private set; }


        public static UILauncherUpgrade Create(GObject go, FUI parent = null, object userData = null)
        {
            return new UILauncherUpgrade(go, userData, parent);
        }
        /*
        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UILauncherUpgrade GetFormPool(GObject go)
        {
            var fui =  go.Get<UILauncherUpgrade>();
            if(fui == null)
            {
                fui = Create(go);
            }
            fui.isFromFGUIPool = true;
            return fui;
        }
        */

        protected override void InitView()
        {
            if(GObject == null)
            {
                return;
            }

            self = (GComponent)GObject;
            
            var com = GObject.asCom;
            if(com != null)
            {
				m_bg = (GGraph)com.GetChild("bg");
				m_EnterButton = UILauncherEnterButton.Create(com.GetChild("EnterButton"), this);
				m_TextContent = UILauncherUpgradeContent.Create(com.GetChild("TextContent"), this);
            }
        }

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            base.Dispose();
			m_bg = null;
			m_EnterButton = null;
			m_TextContent = null;
            self = null;            
        }

        private UILauncherUpgrade(GObject gObject, object userData, FUI parent = null) : base(gObject, parent, userData)
        {
            // Awake(gObject);
        }
    }
}