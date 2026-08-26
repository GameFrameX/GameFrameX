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
    public sealed partial class UILogin : FUI
    {
        public const string UIPackageName = "UILogin";
        public const string UIResName = "UILogin";
        public const string URL = "ui://f011l0h9nmd0c";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GTextField m_ErrorText { get; private set; }
		public GButton m_enter { get; private set; }
		public GTextInput m_UserName { get; private set; }
		public GTextInput m_Password { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UILogin CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UniTask<UILogin> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UILogin> tcs = new UniTaskCompletionSource<UILogin>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go));
            });
            return tcs.Task;
        }

        public static UILogin Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UILogin>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UILogin GetFormPool(GObject go)
        {
            var fui = go.Get<UILogin>();
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
				m_ErrorText = (GTextField)com.GetChild("ErrorText");
				m_enter = (GButton)com.GetChild("enter");
				m_UserName = (GTextInput)com.GetChild("UserName");
				m_Password = (GTextInput)com.GetChild("Password");
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
			m_ErrorText = null;
			m_enter = null;
			m_UserName = null;
			m_Password = null;
            self = null;            
        }

        private UILogin(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif