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
    public sealed partial class UIPlayerCreate : FUI
    {
        public const string UIPackageName = "UILogin";
        public const string UIResName = "UIPlayerCreate";
        public const string URL = "ui://f011l0h9i3dbs9p";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GTextInput m_UserName { get; private set; }
		public GButton m_enter { get; private set; }
		public GTextField m_ErrorText { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UIPlayerCreate CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UniTask<UIPlayerCreate> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UIPlayerCreate> tcs = new UniTaskCompletionSource<UIPlayerCreate>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go));
            });
            return tcs.Task;
        }

        public static UIPlayerCreate Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIPlayerCreate>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIPlayerCreate GetFormPool(GObject go)
        {
            var fui = go.Get<UIPlayerCreate>();
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
				m_UserName = (GTextInput)com.GetChild("UserName");
				m_enter = (GButton)com.GetChild("enter");
				m_ErrorText = (GTextField)com.GetChild("ErrorText");
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
			m_UserName = null;
			m_enter = null;
			m_ErrorText = null;
            self = null;            
        }

        private UIPlayerCreate(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif