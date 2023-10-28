using FairyGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using GameFrameX.ObjectPool;

namespace GameFrameX.Runtime
{
    public class FUI : ObjectBase, IDisposable
    {
        /// <summary>
        /// 界面显示之后触发
        /// </summary>
        public Action<FUI> OnShowAction { get; set; }

        /// <summary>
        /// 界面隐藏之前触发
        /// </summary>
        public Action<FUI> OnHideAction { get; set; }

        public FUI(GObject gObject, FUI parent = null)
        {
            GObject = gObject;
            Parent = parent;
            // 在初始化的时候先隐藏UI。后续由声明周期控制
            // if (parent == null)
            // {
            SetVisibleWithNoNotify(false);
            // }

            parent?.Add(this);

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
        /// 界面添加到UI系统之前执行
        /// </summary>
        public virtual void Init()
        {
            Log.Info("Init " + Name);
        }

        /// <summary>
        /// 界面显示后执行
        /// </summary>
        protected virtual void OnShow()
        {
            Log.Info("OnShow " + Name);
        }


        /// <summary>
        /// 界面显示之后执行，设置数据和多语言建议在这里设置
        /// </summary>
        public virtual void Refresh()
        {
            Log.Info("Refresh " + Name);
        }

        /// <summary>
        /// 界面隐藏之前执行
        /// </summary>
        protected virtual void OnHide()
        {
            Log.Info("OnHide " + Name);
        }

        /// <summary>
        /// UI 对象销毁之前执行
        /// </summary>
        protected virtual void OnDispose()
        {
            Log.Info("OnDispose " + Name);
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

            Log.Info("Show " + Name);
            Visible = true;
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

            Log.Info("Hide " + Name);
            Visible = false;
        }

        /// <summary>
        /// UI 对象
        /// </summary>
        public GObject GObject { get; }

        /// <summary>
        /// UI 名称
        /// </summary>
        public sealed override string Name
        {
            get
            {
                if (GObject == null)
                {
                    return string.Empty;
                }

                return GObject.name;
            }

            protected set
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

        protected internal override void Release(bool isShutdown)
        {
        }

        /// <summary>
        /// 设置UI的显示状态，不发出事件
        /// </summary>
        /// <param name="value"></param>
        private void SetVisibleWithNoNotify(bool value)
        {
            GObject.visible = value;
        }

        /// <summary>
        /// 获取UI是否显示
        /// </summary>
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
                    OnHide();
                    OnHideAction?.Invoke(this);
                }

                GObject.visible = value;
                if (value)
                {
                    OnShowAction?.Invoke(this);
                    OnShow();
                    Refresh();
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
        // protected bool isFromFGUIPool;


        protected bool IsDisposed;

        /// <summary>
        /// 销毁UI对象
        /// </summary>
        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            // 删除自己的UI
            if (!IsRoot)
            {
                RemoveFromParent();
            }

            // 删除所有的孩子
            DisposeChildren();
            // 释放UI
            OnDispose();
            // 删除自己的UI
            if (!IsRoot)
            {
                GObject.Dispose();
            }

            OnShowAction = null;
            OnHideAction = null;
            Parent = null;
            // isFromFGUIPool = false;
        }

        /// <summary>
        /// 添加UI对象到子级列表
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="index">添加到的目标UI层级索引位置</param>
        /// <exception cref="Exception"></exception>
        public void Add(FUI ui, int index = -1)
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

            ui.Init();
            _children.Add(ui.Name, ui);
            if (index < 0 || index > _children.Count)
            {
                GObject.asCom.AddChild(ui.GObject);
            }
            else
            {
                GObject.asCom.AddChildAt(ui.GObject, index);
            }

            ui.Parent = this;
            // 显示UI
            ui.Show();
        }

        /// <summary>
        /// UI 父级对象
        /// </summary>
        public FUI Parent { get; protected set; }

        /// <summary>
        /// 设置当前UI对象为全屏
        /// </summary>
        public void MakeFullScreen()
        {
            GObject?.asCom?.MakeFullScreen();
        }

        /// <summary>
        /// 将自己从父级UI对象删除
        /// </summary>
        public void RemoveFromParent()
        {
            Parent?.Remove(Name);
        }

        /// <summary>
        /// 删除指定UI名称的UI对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Remove(string name)
        {
            if (_children.TryGetValue(name, out var ui))
            {
                _children.Remove(name);

                if (ui != null)
                {
                    ui.RemoveChildren();

                    ui.Hide();

                    if (IsComponent)
                    {
                        GObject.asCom.RemoveChild(ui.GObject);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 销毁所有自己对象
        /// </summary>
        public void DisposeChildren()
        {
            if (_children.Count > 0)
            {
                var children = GetAll();
                foreach (var child in children)
                {
                    child.Dispose();
                }

                _children.Clear();
            }
        }

        /// <summary>
        /// 删除所有子级UI对象
        /// </summary>
        public void RemoveChildren()
        {
            if (_children.Count > 0)
            {
                var children = GetAll();

                foreach (var child in children)
                {
                    child.RemoveFromParent();
                }

                _children.Clear();
            }
        }

        /// <summary>
        /// 根据 UI名称 获取子级UI对象
        /// </summary>
        /// <param name="name">UI名称</param>
        /// <returns></returns>
        public FUI Get(string name)
        {
            if (_children.TryGetValue(name, out var child))
            {
                return child;
            }

            return null;
        }

        /// <summary>
        /// 获取所有的子级UI，非递归
        /// </summary>
        /// <returns></returns>
        public FUI[] GetAll()
        {
            return _children.Values.ToArray();
        }
    }
}