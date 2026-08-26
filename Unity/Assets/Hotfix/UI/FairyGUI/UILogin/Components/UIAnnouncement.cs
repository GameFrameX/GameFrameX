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
    public sealed partial class UIAnnouncement : FUI
    {
        public const string UIPackageName = "UILogin";
        public const string UIResName = "UIAnnouncement";
        public const string URL = "ui://f011l0h9aneks9g";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GGraph m_MaskLayer { get; private set; }
		public UIAnnouncementContent m_TextContent { get; private set; }
		public GTextField m_TextTitle { get; private set; }

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
            return Create(CreateGObject());
        }

        public static UniTask<UIAnnouncement> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UIAnnouncement> tcs = new UniTaskCompletionSource<UIAnnouncement>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go));
            });
            return tcs.Task;
        }

        public static UIAnnouncement Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIAnnouncement>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIAnnouncement GetFormPool(GObject go)
        {
            var fui = go.Get<UIAnnouncement>();
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
				m_MaskLayer = (GGraph)com.GetChild("MaskLayer");
				m_TextContent = UIAnnouncementContent.Create(com.GetChild("TextContent"));
				m_TextTitle = (GTextField)com.GetChild("TextTitle");
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
			m_MaskLayer = null;
			m_TextContent = null;
			m_TextTitle = null;
            self = null;            
        }

        private UIAnnouncement(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif