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
    public sealed partial class UIGoodItem : FUI
    {
        public const string UIPackageName = "UICommon";
        public const string UIResName = "UIGoodItem";
        public const string URL = "ui://ats3vms372ce2u";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GButton self { get; private set; }

		public GLoader m_bg { get; private set; }
		public GLoader m_gift { get; private set; }
		public GTextField m_number { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UIGoodItem CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UniTask<UIGoodItem> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UIGoodItem> tcs = new UniTaskCompletionSource<UIGoodItem>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go));
            });
            return tcs.Task;
        }

        public static UIGoodItem Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIGoodItem>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIGoodItem GetFormPool(GObject go)
        {
            var fui = go.Get<UIGoodItem>();
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
				m_bg = (GLoader)com.GetChild("bg");
				m_gift = (GLoader)com.GetChild("gift");
				m_number = (GTextField)com.GetChild("number");
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
			m_bg = null;
			m_gift = null;
			m_number = null;
            self = null;            
        }

        private UIGoodItem(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif