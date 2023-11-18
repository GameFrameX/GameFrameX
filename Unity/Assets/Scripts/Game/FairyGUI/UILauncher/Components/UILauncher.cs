/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using Cysharp.Threading.Tasks;
using FairyGUI.Utils;
using GameFrameX.Runtime;

namespace Game.Model
{
    public sealed partial class UILauncher : FUI
    {
        public const string UIPackageName = "UILauncher";
        public const string UIResName = "UILauncher";
        public const string URL = "ui://u7deosq0mw8e0";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public Controller m_IsUpgrade { get; private set; }
		public Controller m_IsDownload { get; private set; }
		public GLoader m_bg { get; private set; }
		public GTextField m_TipText { get; private set; }
		public GProgressBar m_ProgressBar { get; private set; }
		public UILauncherUpgrade m_upgrade { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UILauncher CreateInstance(object userData = null)
        {
            return new UILauncher(CreateGObject(), userData);
        }

        public static UniTask<UILauncher> CreateInstanceAsync(Entity domain, object userData = null)
        {
            UniTaskCompletionSource<UILauncher> tcs = new UniTaskCompletionSource<UILauncher>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(new UILauncher(go, userData));
            });
            return tcs.Task;
        }

        public static UILauncher Create(GObject go, FUI parent = null, object userData = null)
        {
            return new UILauncher(go, userData, parent);
        }
        /*
        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UILauncher GetFormPool(GObject go)
        {
            var fui =  go.Get<UILauncher>();
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
				m_IsUpgrade = com.GetController("IsUpgrade");
				m_IsDownload = com.GetController("IsDownload");
				m_bg = (GLoader)com.GetChild("bg");
				m_TipText = (GTextField)com.GetChild("TipText");
				m_ProgressBar = (GProgressBar)com.GetChild("ProgressBar");
				m_upgrade = UILauncherUpgrade.Create(com.GetChild("upgrade"), this);
            }
        }

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            base.Dispose();
			m_IsUpgrade = null;
			m_IsDownload = null;
			m_bg = null;
			m_TipText = null;
			m_ProgressBar = null;
			m_upgrade = null;
            self = null;            
        }

        private UILauncher(GObject gObject, object userData, FUI parent = null) : base(gObject, parent, userData)
        {
            // Awake(gObject);
        }
    }
}