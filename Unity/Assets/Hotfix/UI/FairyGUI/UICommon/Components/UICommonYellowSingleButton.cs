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
    public sealed partial class UICommonYellowSingleButton : FUI
    {
        public const string UIPackageName = "UICommon";
        public const string UIResName = "UICommonYellowSingleButton";
        public const string URL = "ui://ats3vms3oo3b2o";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GButton self { get; private set; }



        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UICommonYellowSingleButton CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UICommonYellowSingleButton Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UICommonYellowSingleButton>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UICommonYellowSingleButton GetFormPool(GObject go)
        {
            var fui = go.Get<UICommonYellowSingleButton>();
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

            self = null;
        }

        private UICommonYellowSingleButton(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif