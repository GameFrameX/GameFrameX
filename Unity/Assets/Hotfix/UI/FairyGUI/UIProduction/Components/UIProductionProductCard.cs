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
    public sealed partial class UIProductionProductCard : FUI
    {
        public const string UIPackageName = "UIProduction";
        public const string UIResName = "UIProductionProductCard";
        public const string URL = "ui://prod1001prods";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public Controller m_state { get; private set; }
		public GImage m_product_card_shadow { get; private set; }
		public GImage m_product_card_background { get; private set; }
		public GImage m_product_card_highlight { get; private set; }
		public GImage m_product_upper_gradient { get; private set; }
		public GImage m_product_lower_gradient { get; private set; }
		public GImage m_product_caption_background { get; private set; }
		public GTextField m_title { get; private set; }
		public GLoader m_product_image { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIProductionProductCard CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIProductionProductCard Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIProductionProductCard>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIProductionProductCard GetFormPool(GObject go)
        {
            var fui = go.Get<UIProductionProductCard>();
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
				m_state = com.GetController("state");
				m_product_card_shadow = (GImage)com.GetChild("product_card_shadow");
				m_product_card_background = (GImage)com.GetChild("product_card_background");
				m_product_card_highlight = (GImage)com.GetChild("product_card_highlight");
				m_product_upper_gradient = (GImage)com.GetChild("product_upper_gradient");
				m_product_lower_gradient = (GImage)com.GetChild("product_lower_gradient");
				m_product_caption_background = (GImage)com.GetChild("product_caption_background");
				m_title = (GTextField)com.GetChild("title");
				m_product_image = (GLoader)com.GetChild("product_image");
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
			m_state = null;
			m_product_card_shadow = null;
			m_product_card_background = null;
			m_product_card_highlight = null;
			m_product_upper_gradient = null;
			m_product_lower_gradient = null;
			m_product_caption_background = null;
			m_title = null;
			m_product_image = null;
            self = null;
        }

        private UIProductionProductCard(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif