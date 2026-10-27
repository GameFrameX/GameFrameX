/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

#if ENABLE_UI_FAIRYGUI
using FairyGUI;
using FairyGUI.Utils;
using GameFrameX.UI.Runtime;
using GameFrameX.UI.FairyGUI.Runtime;
using GameFrameX.Runtime;
using UnityEngine;

namespace Hotfix.UI
{
    [OptionUIConfig(UIPackageName)]
    public sealed partial class UILoginPlayerCreate : FUI
    {
        public const string UIPackageName = "UILogin";
        public const string UIResName = "UILoginPlayerCreate";
        public const string URL = "ui://f011l0h9i3dbs9p";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GTextInput m_user_name { get; private set; }
		public GButton m_enter { get; private set; }
		public GTextField m_error_text { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UILoginPlayerCreate CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UILoginPlayerCreate Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UILoginPlayerCreate>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UILoginPlayerCreate GetFormPool(GObject go)
        {
            var fui = go.Get<UILoginPlayerCreate>();
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
				m_user_name = (GTextInput)com.GetChild("user_name");
				m_enter = (GButton)com.GetChild("enter");
				m_error_text = (GTextField)com.GetChild("error_text");
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
			m_user_name = null;
			m_enter = null;
			m_error_text = null;
            self = null;
        }

        private UILoginPlayerCreate(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif