using System;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 管理所有顶层UI, 顶层UI都是GRoot的孩子
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/FUI")]
    public sealed class FUIComponent : GameFrameworkComponent
    {
        private FUI _root;
        FUI HiddenRoot;
        FUI FloorRoot;
        FUI NormalRoot;
        FUI FixedRoot;
        FUI WindowRoot;
        FUI TipRoot;
        FUI BlackBoardRoot;
        FUI DialogueRoot;
        FUI GuideRoot;
        FUI LoadingRoot;
        FUI NotifyRoot;

        FUI SystemRoot;
        // public FUI UIRoot;

        private Dictionary<UILayer, Dictionary<string, FUI>> _dictionary = new Dictionary<UILayer, Dictionary<string, FUI>>(16);
        private Dictionary<string, FUI> _fuis = new Dictionary<string, FUI>(64);

        public void OnDestroy()
        {
            // UIRoot.Dispose();
            _root.Dispose();
            _root = null;
        }

        public void Add<T>(FUI ui, UILayer layer) where T : FUI
        {
            ui.Name = nameof(T);
            Add(ui, layer);
        }

        public void RemoveAll()
        {
            foreach (var keyValuePair in _fuis)
            {
                keyValuePair.Value.Dispose();
            }

            _fuis.Clear();
            foreach (var kv in _dictionary)
            {
                foreach (var fui in kv.Value)
                {
                    Remove(fui.Key);
                }

                kv.Value.Clear();
            }
        }

        public void Add(FUI ui, UILayer layer)
        {
            if (!_fuis.ContainsKey(ui.Name))
            {
                _fuis[ui.Name] = ui;
            }

            _dictionary[layer][ui.Name] = ui;
            switch (layer)
            {
                case UILayer.Hidden:
                    HiddenRoot.Add(ui);
                    break;
                case UILayer.Floor:
                    FloorRoot.Add(ui);
                    break;
                case UILayer.Normal:
                    NormalRoot.Add(ui);
                    break;
                case UILayer.Fixed:
                    FixedRoot.Add(ui);
                    break;
                case UILayer.Window:
                    WindowRoot.Add(ui);
                    break;
                case UILayer.Tip:
                    TipRoot.Add(ui);
                    break;
                case UILayer.BlackBoard:
                    BlackBoardRoot.Add(ui);
                    break;
                case UILayer.Dialogue:
                    DialogueRoot.Add(ui);
                    break;
                case UILayer.Guide:
                    GuideRoot.Add(ui);
                    break;
                case UILayer.Loading:
                    LoadingRoot.Add(ui);
                    break;
                case UILayer.Notify:
                    NotifyRoot.Add(ui);
                    break;
                case UILayer.System:
                    SystemRoot.Add(ui);
                    break;
            }
        }

        public bool Remove(string name)
        {
            if (SystemRoot.Remove(name))
            {
                return true;
            }

            if (NotifyRoot.Remove(name))
            {
                return true;
            }

            if (HiddenRoot.Remove(name))
            {
                return true;
            }

            if (FloorRoot.Remove(name))
            {
                return true;
            }

            if (NormalRoot.Remove(name))
            {
                return true;
            }

            if (FixedRoot.Remove(name))
            {
                return true;
            }

            if (WindowRoot.Remove(name))
            {
                return true;
            }

            if (TipRoot.Remove(name))
            {
                return true;
            }

            if (BlackBoardRoot.Remove(name))
            {
                return true;
            }

            if (DialogueRoot.Remove(name))
            {
                return true;
            }

            if (GuideRoot.Remove(name))
            {
                return true;
            }

            if (LoadingRoot.Remove(name))
            {
                return true;
            }

            return false;
        }

        public void Remove(string name, UILayer layer)
        {
            switch (layer)
            {
                case UILayer.Hidden:
                    HiddenRoot.Remove(name);
                    break;
                case UILayer.Floor:
                    FloorRoot.Remove(name);
                    break;
                case UILayer.Normal:
                    NormalRoot.Remove(name);
                    break;
                case UILayer.Fixed:
                    FixedRoot.Remove(name);
                    break;
                case UILayer.Window:
                    WindowRoot.Remove(name);
                    break;
                case UILayer.Tip:
                    TipRoot.Remove(name);
                    break;
                case UILayer.BlackBoard:
                    BlackBoardRoot.Remove(name);
                    break;
                case UILayer.Dialogue:
                    DialogueRoot.Remove(name);
                    break;
                case UILayer.Guide:
                    GuideRoot.Remove(name);
                    break;
                case UILayer.Loading:
                    LoadingRoot.Remove(name);
                    break;
                case UILayer.Notify:
                    NotifyRoot.Remove(name);
                    break;
                case UILayer.System:
                    SystemRoot.Remove(name);
                    break;
            }
        }

        public bool Has(string name)
        {
            return Get(name) != null;
        }

        /// <summary>
        /// 判断UI是否已创建。如果创建则。返回UI对象
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fui"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Has<T>(string name, out T fui) where T : FUI
        {
            var ui = Get(name);
            fui = ui as T;
            return fui != null;
        }

        public T Get<T>(string name) where T : FUI
        {
            if (_fuis.TryGetValue(name, out var ui))
            {
                return ui as T;
            }

            return null;
        }

        public FUI Get(string name)
        {
            if (_fuis.TryGetValue(name, out var ui))
            {
                return ui;
            }

            return null;
        }

        protected override void Awake()
        {
            base.Awake();
            _root = new FUI(GRoot.inst);

            // _screenOrientation = Screen.orientation;


            // UIRoot = new FUI(uiRoot);
            // uiRoot.name = nameof(UIRoot);
            // _root.Add(UIRoot);
            // uiRoot.MakeFullScreen();

            HiddenRoot = CreateNode(GRoot.inst, UILayer.Hidden);
            FloorRoot = CreateNode(GRoot.inst, UILayer.Floor);
            NormalRoot = CreateNode(GRoot.inst, UILayer.Normal);
            FixedRoot = CreateNode(GRoot.inst, UILayer.Fixed);
            WindowRoot = CreateNode(GRoot.inst, UILayer.Window);
            TipRoot = CreateNode(GRoot.inst, UILayer.Tip);
            BlackBoardRoot = CreateNode(GRoot.inst, UILayer.BlackBoard);
            DialogueRoot = CreateNode(GRoot.inst, UILayer.Dialogue);
            GuideRoot = CreateNode(GRoot.inst, UILayer.Guide);
            LoadingRoot = CreateNode(GRoot.inst, UILayer.Loading);
            NotifyRoot = CreateNode(GRoot.inst, UILayer.Notify);
            SystemRoot = CreateNode(GRoot.inst, UILayer.System);


            _dictionary[UILayer.Hidden] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Floor] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Normal] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Fixed] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Window] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Tip] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.BlackBoard] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Dialogue] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Guide] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Loading] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.Notify] = new Dictionary<string, FUI>(64);
            _dictionary[UILayer.System] = new Dictionary<string, FUI>(64);
        }

        FUI CreateNode(GComponent root, UILayer layer)
        {
            GComponent component = new GComponent();
            root.AddChild(component);
            component.z = (int) layer * 100;
            component.gameObjectName = layer.ToString();
            component.name = layer.ToString();
            component.MakeFullScreen();
            component.AddRelation(root, RelationType.Width);
            component.AddRelation(root, RelationType.Height);

            return new FUI(component);
        }

        // private ScreenOrientation _screenOrientation;

        void IsChanged(bool isLeft)
        {
            // GameApp.Event.Fire("ScreenOrientationChanged", isLeft);
        }

        public void Update()
        {
            // var orientation = Screen.orientation;
            // if (orientation == ScreenOrientation.LandscapeLeft || orientation == ScreenOrientation.LandscapeRight)
            // {
            //     if (_screenOrientation != orientation)
            //     {
            //         IsChanged(orientation == ScreenOrientation.LandscapeLeft);
            //         _screenOrientation = orientation;
            //     }
            // }
        }
    }
}