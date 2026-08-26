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
    public sealed partial class UIDialogWithoutButton : FUI
    {
        public const string UIPackageName = "UICommon";
        public const string UIResName = "UIDialogWithoutButton";
        public const string URL = "ui://ats3vms3srah1v";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GButton m_close_icon { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UIDialogWithoutButton CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UniTask<UIDialogWithoutButton> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UIDialogWithoutButton> tcs = new UniTaskCompletionSource<UIDialogWithoutButton>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go));
            });
            return tcs.Task;
        }

        public static UIDialogWithoutButton Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIDialogWithoutButton>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIDialogWithoutButton GetFormPool(GObject go)
        {
            var fui = go.Get<UIDialogWithoutButton>();
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
				m_close_icon = (GButton)com.GetChild("close_icon");
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
			m_close_icon = null;
            self = null;            
        }

        private UIDialogWithoutButton(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif