using FairyGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.ObjectPool;

namespace UnityGameFramework.Runtime
{
    public class FUI : ObjectBase, IDisposable
    {
        /// <summary>
        /// 界面显示之后触发
        /// </summary>
        public Action<FUI> OnShow { get; set; }

        /// <summary>
        /// 界面隐藏之前触发
        /// </summary>
        public Action<FUI> OnHide { get; set; }

        public FUI(GObject gObject, FUI parent = null)
        {
            GObject = gObject;
            Parent = parent;
            if (gObject.name.IsNullOrWhiteSpace())
            {
                Name = GetType().Name;
            }
            else
            {
                Name = gObject.name;
            }
        }

        /// <summary>
        /// 显示UI
        /// </summary>
        public void Show()
        {
            if (Visible)
            {
                return;
            }

            Visible = true;
            Log.Info("Show " + Name);
        }

        /// <summary>
        /// 界面添加到UI系统之前执行
        /// </summary>
        public virtual void Init()
        {
            Log.Info("Init " + Name);
        }

        /// <summary>
        /// 界面显示之后执行
        /// </summary>
        public virtual void Refresh()
        {
            Log.Info("Refresh " + Name);
        }

        /// <summary>
        /// 隐藏UI
        /// </summary>
        public void Hide()
        {
            if (!Visible)
            {
                return;
            }

            Visible = false;
            Log.Info("Hide " + Name);
        }

        public GObject GObject { get; }

        public new string Name
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

                if (GObject.name != null && GObject.name == value)
                {
                    return;
                }

                GObject.name = value;
            }
        }

        protected override void Release(bool isShutdown)
        {
        }

        public bool IsVisible => Visible;

        private bool Visible
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

                if (GObject.visible == value)
                {
                    return;
                }

                if (!value)
                {
                    OnHide?.Invoke(this);
                }

                GObject.visible = value;
                if (value)
                {
                    Refresh();
                    OnShow?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// 是否是窗口对象
        /// </summary>
        public bool IsWindow
        {
            get { return GObject is GWindow; }
        }

        /// <summary>
        /// 是否是UI组件对象
        /// </summary>
        public bool IsComponent
        {
            get { return GObject is GComponent; }
        }

        /// <summary>
        /// 是否是UI根
        /// </summary>
        public bool IsRoot
        {
            get { return GObject is GRoot; }
        }

        /// <summary>
        /// 界面对象是否为空
        /// </summary>
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
            // 删除所有的孩子
            RemoveChildren();

            // 删除自己的UI
            if (!IsRoot)
            {
                RemoveFromParent();
                GObject.Dispose();
            }

            Parent = null;
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

        public void RemoveFromParent()
        {
            Parent.Remove(Name);
        }

        public bool Remove(string name)
        {
            Hide();
            if (_children.TryGetValue(name, out var ui))
            {
                _children.Remove(name);

                if (ui != null)
                {
                    if (IsComponent)
                    {
                        GObject.asCom.RemoveChild(ui.GObject, false);
                    }

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
            Hide();
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