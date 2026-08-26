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
    public sealed partial class UIBagContent : FUI
    {
        public const string UIPackageName = "UIBag";
        public const string UIResName = "UIBagContent";
        public const string URL = "ui://a3awyna7l50q2";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public Controller m_IsSelectedItem { get; private set; }
		public GList m_list { get; private set; }
		public UIBagItemInfo m_info { get; private set; }
		public GList m_type_list { get; private set; }


        public static UIBagContent Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIBagContent>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIBagContent GetFormPool(GObject go)
        {
            var fui = go.Get<UIBagContent>();
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
				m_IsSelectedItem = com.GetController("IsSelectedItem");
				m_list = (GList)com.GetChild("list");
				m_info = UIBagItemInfo.Create(com.GetChild("info"));
				m_type_list = (GList)com.GetChild("type_list");
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
			m_IsSelectedItem = null;
			m_list = null;
			m_info = null;
			m_type_list = null;
            self = null;            
        }

        private UIBagContent(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif