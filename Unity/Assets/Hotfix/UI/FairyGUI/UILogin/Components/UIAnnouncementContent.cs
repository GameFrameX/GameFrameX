/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using Cysharp.Threading.Tasks;
using FairyGUI.Utils;
using UnityGameFramework.Runtime;

namespace Game.Hotfix
{

    public sealed partial class UIAnnouncementContent : FUI
    {
        public const string UIPackageName = "UILogin";
        public const string UIResName = "UIAnnouncementContent";
        public const string URL = "ui://f011l0h9aneks9i";
        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self;

		public GRichTextField m_LabelContent;  



        public static UIAnnouncementContent Create(GObject go)
        {
            return new UIAnnouncementContent(go);
        }
        /*
        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIAnnouncementContent GetFormPool(GObject go)
        {
            var fui =  go.Get<UIAnnouncementContent>();
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

            //GObject = go;

            self = (GComponent)go;
            
            var com = go.asCom;
            if(com != null)
            {
				m_LabelContent = (GRichTextField)com.GetChild("LabelContent"); 

            }
        }

        public override void Dispose()
        {
            if(IsDisposed)
            {
                return;
            }

            base.Dispose();
            
            self = null;

			m_LabelContent = null; 

        }
        private UIAnnouncementContent(GObject gObject) : base(gObject)
        {
            Awake(gObject);
        }
    }
}