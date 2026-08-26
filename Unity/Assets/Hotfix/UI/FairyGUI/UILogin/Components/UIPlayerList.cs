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
    public sealed partial class UIPlayerList : FUI
    {
        public const string UIPackageName = "UILogin";
        public const string UIResName = "UIPlayerList";
        public const string URL = "ui://f011l0h9i3dbs9m";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public Controller m_IsSelected { get; private set; }
		public GList m_player_list { get; private set; }
		public GLoader m_selected_icon { get; private set; }
		public GRichTextField m_selected_name { get; private set; }
		public GRichTextField m_selected_level { get; private set; }
		public GButton m_login_button { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UIPlayerList CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UniTask<UIPlayerList> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UIPlayerList> tcs = new UniTaskCompletionSource<UIPlayerList>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go));
            });
            return tcs.Task;
        }

        public static UIPlayerList Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIPlayerList>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIPlayerList GetFormPool(GObject go)
        {
            var fui = go.Get<UIPlayerList>();
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
				m_IsSelected = com.GetController("IsSelected");
				m_player_list = (GList)com.GetChild("player_list");
				m_selected_icon = (GLoader)com.GetChild("selected_icon");
				m_selected_name = (GRichTextField)com.GetChild("selected_name");
				m_selected_level = (GRichTextField)com.GetChild("selected_level");
				m_login_button = (GButton)com.GetChild("login_button");
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
			m_IsSelected = null;
			m_player_list = null;
			m_selected_icon = null;
			m_selected_name = null;
			m_selected_level = null;
			m_login_button = null;
            self = null;            
        }

        private UIPlayerList(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif