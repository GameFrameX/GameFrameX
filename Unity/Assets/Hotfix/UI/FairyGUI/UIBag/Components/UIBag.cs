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
    public sealed partial class UIBag : FUI
    {
        public const string UIPackageName = "UIBag";
        public const string UIResName = "UIBag";
        public const string URL = "ui://a3awyna7l50q1";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GGraph m_bg { get; private set; }
		public UIBagContent m_content { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UIBag CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UniTask<UIBag> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UIBag> tcs = new UniTaskCompletionSource<UIBag>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go));
            });
            return tcs.Task;
        }

        public static UIBag Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIBag>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIBag GetFormPool(GObject go)
        {
            var fui = go.Get<UIBag>();
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
				m_bg = (GGraph)com.GetChild("bg");
				m_content = UIBagContent.Create(com.GetChild("content"));
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
			m_content = null;
            self = null;            
        }

        private UIBag(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif