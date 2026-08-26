/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

#if ENABLE_UI_FAIRYGUI
using FairyGUI;
using Cysharp.Threading.Tasks;
using FairyGUI.Utils;
using GameFrameX.Entity.Runtime;
using GameFrameX.UI.Runtime;
using GameFrameX.UI.FairyGUI.Runtime;
using GameFrameX.Runtime;
using UnityEngine;

namespace Hotfix.UI
{
    public sealed partial class UIAnnouncementContent : FUI
    {
        public const string UIPackageName = "UILogin";
        public const string UIResName = "UIAnnouncementContent";
        public const string URL = "ui://f011l0h9aneks9i";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GRichTextField m_LabelContent { get; private set; }


        public static UIAnnouncementContent Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIAnnouncementContent>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIAnnouncementContent GetFormPool(GObject go)
        {
            var fui = go.Get<UIAnnouncementContent>();
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
				m_LabelContent = (GRichTextField)com.GetChild("LabelContent");
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
			m_LabelContent = null;
            self = null;            
        }

        private UIAnnouncementContent(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif