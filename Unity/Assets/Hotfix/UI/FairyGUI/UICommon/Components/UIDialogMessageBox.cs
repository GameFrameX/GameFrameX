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
    public sealed partial class UIDialogMessageBox : FUI
    {
        public const string UIPackageName = "UICommon";
        public const string UIResName = "UIDialogMessageBox";
        public const string URL = "ui://ats3vms3iopl2l";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GButton m_enter_button { get; private set; }
		public GButton m_cancel_button { get; private set; }
		public GRichTextField m_content { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UIDialogMessageBox CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UniTask<UIDialogMessageBox> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UIDialogMessageBox> tcs = new UniTaskCompletionSource<UIDialogMessageBox>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go));
            });
            return tcs.Task;
        }

        public static UIDialogMessageBox Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIDialogMessageBox>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIDialogMessageBox GetFormPool(GObject go)
        {
            var fui = go.Get<UIDialogMessageBox>();
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
				m_enter_button = (GButton)com.GetChild("enter_button");
				m_cancel_button = (GButton)com.GetChild("cancel_button");
				m_content = (GRichTextField)com.GetChild("content");
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
			m_enter_button = null;
			m_cancel_button = null;
			m_content = null;
            self = null;            
        }

        private UIDialogMessageBox(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif