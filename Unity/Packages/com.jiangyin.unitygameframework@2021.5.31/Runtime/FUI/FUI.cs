using FairyGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.ObjectPool;

namespace UnityGameFramework.Runtime
{
    public class FUI : ObjectBase, IDisposable
    {
        public FUI(GObject gObject)
        {
            GObject = gObject;
            Name = GetType().Name;
        }

        public virtual void Show()
        {
            Visible = true;
            Refresh();
        }

        public virtual void Refresh()
        {
        }

        public virtual void Hide()
        {
            Visible = false;
        }

        public GObject GObject { get; }

        public string Name
        {
            get
            {
                if (GObject == null)
                {
                    return string.Empty;
                }

                return GObject.name;
            }

            set
            {
                if (GObject == null)
                {
                    return;
                }

                GObject.name = value;
            }
        }

        protected override void Release(bool isShutdown)
        {
        }

        public bool Visible
        {
            get
            {
                if (GObject == null)
                {
                    return false;
                }

                return GObject.visible;
            }
            set
            {
                if (GObject == null)
                {
                    return;
                }

                GObject.visible = value;
            }
        }

        public bool IsWindow
        {
            get { return GObject is GWindow; }
        }

        public bool IsComponent
        {
            get { return GObject is GComponent; }
        }

        public bool IsRoot
        {
            get { return GObject is GRoot; }
        }

        public bool IsEmpty
        {
            get { return GObject == null; }
        }

        private readonly Dictionary<string, FUI> _children = new Dictionary<string, FUI>();
        protected bool isFromFGUIPool;

        protected bool IsDisposed;

        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            // 从父亲中删除自己
            // Pare?.RemoveNoDispose(Name);

            // 删除所有的孩子
            foreach (FUI ui in _children.Values.ToArray())
            {
                ui.Dispose();
            }

            _children.Clear();

            // 删除自己的UI
            if (!IsRoot)
            {
                GObject.Dispose();
            }

            isFromFGUIPool = false;
        }


        public void Add(FUI ui)
        {
            if (ui == null || ui.IsEmpty)
            {
                throw new Exception($"ui can not be empty");
            }

            if (string.IsNullOrWhiteSpace(ui.Name))
            {
                throw new Exception($"ui.Name can not be empty");
            }

            if (!IsComponent)
            {
                throw new Exception($"this must be GComponent");
            }

            if (_children.ContainsKey(ui.Name))
            {
                throw new Exception($"ui.Name({ui.Name}) already exist");
            }

            _children.Add(ui.Name, ui);

            GObject.asCom.AddChild(ui.GObject);

            ui.Parent = this;
        }

        public FUI Parent { get; protected set; }

        public void MakeFullScreen()
        {
            GObject?.asCom?.MakeFullScreen();
        }

        public bool Remove(string name)
        {
            FUI ui;

            if (_children.TryGetValue(name, out ui))
            {
                _children.Remove(name);

                if (ui != null)
                {
                    if (IsComponent)
                    {
                        GObject.asCom.RemoveChild(ui.GObject, false);
                    }

                    ui.Parent = null;
                    ui.Dispose();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 一般情况不要使用此方法，如需使用，需要自行管理返回值的FUI的释放。
        /// </summary>
        public FUI RemoveNoDispose(string name)
        {
            if (_children.TryGetValue(name, out var ui))
            {
                _children.Remove(name);

                if (ui != null)
                {
                    if (IsComponent)
                    {
                        GObject.asCom.RemoveChild(ui.GObject, false);
                    }

                    ui.Parent = null;
                }
            }

            return ui;
        }

        public void RemoveChildren()
        {
            foreach (var child in _children.Values.ToArray())
            {
                child.Dispose();
            }

            _children.Clear();
        }

        public FUI Get(string name)
        {
            if (_children.TryGetValue(name, out var child))
            {
                return child;
            }

            return null;
        }

        public FUI[] GetAll()
        {
            return _children.Values.ToArray();
        }
    }
}