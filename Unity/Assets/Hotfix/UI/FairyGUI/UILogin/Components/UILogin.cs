/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using Cysharp.Threading.Tasks;
using FairyGUI.Utils;
using GameFrameX.Runtime;

namespace Game.Hotfix
{

    public sealed partial class UILogin : FUI
    {
        public const string UIPackageName = "UILogin";
        public const string UIResName = "UILogin";
        public const string URL = "ui://f011l0h9nmd0c";
        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self;

		public GTextInput m_UserName;  
		public GTextInput m_Password;  
		public GTextField m_ErrorText;  
		public GButton m_enter;  


        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }
    
        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }
    
        public static UILogin CreateInstance(object userData = null)
        {
            return new UILogin(CreateGObject(), userData);
        }
    
        public static UniTask<UILogin> CreateInstanceAsync(Entity domain, object userData = null)
        {
            UniTaskCompletionSource<UILogin> tcs = new UniTaskCompletionSource<UILogin>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(new UILogin(go, userData));
            });
            return tcs.Task;
        }

        public static UILogin Create(GObject go, FUI parent = null, object userData = null)
        {
            return new UILogin(go, userData, parent);
        }
        /*
        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UILogin GetFormPool(GObject go)
        {
            var fui =  go.Get<UILogin>();
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
				m_UserName = (GTextInput)com.GetChild("UserName"); 
				m_Password = (GTextInput)com.GetChild("Password"); 
				m_ErrorText = (GTextField)com.GetChild("ErrorText"); 
				m_enter = (GButton)com.GetChild("enter"); 

            }
        }

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

			m_UserName = null; 
			m_Password = null; 
			m_ErrorText = null; 
			m_enter = null; 

            
            self = null;
            base.Dispose();
        }
        private UILogin(GObject gObject, object userData, FUI parent = null) : base(gObject, parent, userData)
        {
            // Awake(gObject);
        }
    }
}