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
    public sealed partial class UIBagTypeItem : FUI
    {
        public const string UIPackageName = "UIBag";
        public const string UIResName = "UIBagTypeItem";
        public const string URL = "ui://a3awyna7l50q4";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GButton self { get; private set; }

		public GImage m_normal { get; private set; }
		public GImage m_select { get; private set; }


        public static UIBagTypeItem Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIBagTypeItem>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIBagTypeItem GetFormPool(GObject go)
        {
            var fui = go.Get<UIBagTypeItem>();
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
				m_normal = (GImage)com.GetChild("normal");
				m_select = (GImage)com.GetChild("select");
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
			m_normal = null;
			m_select = null;
            self = null;            
        }

        private UIBagTypeItem(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif