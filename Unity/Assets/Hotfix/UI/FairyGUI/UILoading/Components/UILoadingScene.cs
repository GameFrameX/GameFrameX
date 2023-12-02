/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using Cysharp.Threading.Tasks;
using FairyGUI.Utils;
using GameFrameX.Runtime;

namespace Hotfix.UI
{
    public sealed partial class UILoadingScene : FUI
    {
        public const string UIPackageName = "UILoading";
        public const string UIResName = "UILoadingScene";
        public const string URL = "ui://qecztwbp5euk0";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }



        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UILoadingScene CreateInstance(object userData = null)
        {
            return new UILoadingScene(CreateGObject(), userData);
        }

        public static UniTask<UILoadingScene> CreateInstanceAsync(Entity domain, object userData = null)
        {
            UniTaskCompletionSource<UILoadingScene> tcs = new UniTaskCompletionSource<UILoadingScene>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(new UILoadingScene(go, userData));
            });
            return tcs.Task;
        }

        public static UILoadingScene Create(GObject go, FUI parent = null, object userData = null)
        {
            return new UILoadingScene(go, userData, parent);
        }
        /*
        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UILoadingScene GetFormPool(GObject go)
        {
            var fui =  go.Get<UILoadingScene>();
            if(fui == null)
            {
                fui = Create(go);
            }
            fui.isFromFGUIPool = true;
            return fui;
        }
        */

        protected override void InitView()
        {
            if(GObject == null)
            {
                return;
            }

            self = (GComponent)GObject;
            
            var com = GObject.asCom;
            if(com != null)
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

            self = null;            
        }

        private UILoadingScene(GObject gObject, object userData, FUI parent = null) : base(gObject, parent, userData)
        {
            // Awake(gObject);
        }
    }
}