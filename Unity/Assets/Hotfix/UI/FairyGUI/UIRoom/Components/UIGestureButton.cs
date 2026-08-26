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
    public sealed partial class UIGestureButton : FUI
    {
        public const string UIPackageName = "UIRoom";
        public const string UIResName = "UIGestureButton";
        public const string URL = "ui://rpsroom1gesti1";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GButton self { get; private set; }

		public Controller m_Gesture { get; private set; }
		public GGraph m_bg { get; private set; }
		public GGraph m_rock_icon { get; private set; }
		public GGraph m_scissors_left { get; private set; }
		public GGraph m_scissors_right { get; private set; }
		public GGraph m_paper_back { get; private set; }
		public GGraph m_paper_front { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UIGestureButton CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UniTask<UIGestureButton> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UIGestureButton> tcs = new UniTaskCompletionSource<UIGestureButton>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go));
            });
            return tcs.Task;
        }

        public static UIGestureButton Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIGestureButton>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIGestureButton GetFormPool(GObject go)
        {
            var fui = go.Get<UIGestureButton>();
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
				m_Gesture = com.GetController("Gesture");
				m_bg = (GGraph)com.GetChild("bg");
				m_rock_icon = (GGraph)com.GetChild("rock_icon");
				m_scissors_left = (GGraph)com.GetChild("scissors_left");
				m_scissors_right = (GGraph)com.GetChild("scissors_right");
				m_paper_back = (GGraph)com.GetChild("paper_back");
				m_paper_front = (GGraph)com.GetChild("paper_front");
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
			m_Gesture = null;
			m_bg = null;
			m_rock_icon = null;
			m_scissors_left = null;
			m_scissors_right = null;
			m_paper_back = null;
			m_paper_front = null;
            self = null;            
        }

        private UIGestureButton(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif