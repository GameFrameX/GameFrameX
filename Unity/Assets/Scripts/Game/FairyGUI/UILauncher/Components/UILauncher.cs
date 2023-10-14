/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using Cysharp.Threading.Tasks;
using FairyGUI.Utils;
using UnityGameFramework.Runtime;

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
        public GComponent self;

		public Controller m_IsUpgrade;  
		public Controller m_IsDownload;  
		public GLoader m_bg;  
		public GTextField m_TipText;  
		public GProgressBar m_ProgressBar;  
		public UILauncherUpgrade m_upgrade;  


        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }
    
        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }
    
        public static UILauncher CreateInstance()
        {
            return new UILauncher(CreateGObject());
        }
    
        public static UniTask<UILauncher> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UILauncher> tcs = new UniTaskCompletionSource<UILauncher>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(new UILauncher(go));
            });
            return tcs.Task;
        }

        public static UILauncher Create(GObject go, FUI parent = null)
        {
            return new UILauncher(go, parent);
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

        private void Awake(GObject go)
        {
            if(go == null)
            {
                return;
            }

            self = (GComponent)go;
            
            var com = go.asCom;
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

			m_IsUpgrade = null; 
			m_IsDownload = null; 
			m_bg = null; 
			m_TipText = null; 
			m_ProgressBar = null; 
			m_upgrade = null; 

            
            self = null;
            base.Dispose();
        }
        private UILauncher(GObject gObject, FUI parent = null) : base(gObject, parent)
        {
            Awake(gObject);
        }
    }
}