/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using Cysharp.Threading.Tasks;
using FairyGUI.Utils;
using GameFrameX.Runtime;

namespace Game.Hotfix
{

    public sealed partial class UIAnnouncement : FUI
    {
        public const string UIPackageName = "UILogin";
        public const string UIResName = "UIAnnouncement";
        public const string URL = "ui://f011l0h9aneks9g";
        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self;

		public GGraph m_MaskLayer;  
		public GComponent m_ConfirmBtn;  
		public UIAnnouncementContent m_TextContent;  
		public GTextField m_TextTitle;  


        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }
    
        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }
    
        public static UIAnnouncement CreateInstance()
        {
            return new UIAnnouncement(CreateGObject());
        }
    
        public static UniTask<UIAnnouncement> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UIAnnouncement> tcs = new UniTaskCompletionSource<UIAnnouncement>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(new UIAnnouncement(go));
            });
            return tcs.Task;
        }

        public static UIAnnouncement Create(GObject go, FUI parent = null)
        {
            return new UIAnnouncement(go, parent);
        }
        /*
        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIAnnouncement GetFormPool(GObject go)
        {
            var fui =  go.Get<UIAnnouncement>();
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
				m_MaskLayer = (GGraph)com.GetChild("MaskLayer"); 
				m_ConfirmBtn = (GComponent)com.GetChild("ConfirmBtn"); 
				m_TextContent = UIAnnouncementContent.Create(com.GetChild("TextContent"), this);  
				m_TextTitle = (GTextField)com.GetChild("TextTitle"); 

            }
        }

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

			m_MaskLayer = null; 
			m_ConfirmBtn = null; 
			m_TextContent = null; 
			m_TextTitle = null; 

            
            self = null;
            base.Dispose();
        }
        private UIAnnouncement(GObject gObject, FUI parent = null) : base(gObject, parent)
        {
            Awake(gObject);
        }
    }
}