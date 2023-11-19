using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// Helper for drag and drop.
    /// 这是一个提供特殊拖放功能的功能类。与GObject.draggable不同，拖动开始后，他使用一个替代的图标作为拖动对象。
    /// 当玩家释放鼠标/手指，目标组件会发出一个onDrop事件。
    /// </summary>
    public sealed class DragDropManager
    {
        private GLoader _agent;
        private GObject _customAgent;
        private bool _useCustomAgent;
        private object _sourceData;
        private GObject _source;

        private static DragDropManager _inst;

        public static DragDropManager inst
        {
            get
            {
                if (_inst == null)
                    _inst = new DragDropManager();
                return _inst;
            }
        }

        public DragDropManager()
        {
            _agent = (GLoader)UIObjectFactory.NewObject(ObjectType.Loader);
            _agent.gameObjectName = "DragDropAgent";
            _agent.SetHome(GRoot.inst);
            _agent.touchable = false; //important
            _agent.draggable = true;
            _agent.SetSize(100, 100);
            _agent.SetPivot(0.5f, 0.5f, true);
            _agent.align = AlignType.Center;
            _agent.verticalAlign = VertAlignType.Middle;
            _agent.sortingOrder = int.MaxValue;
            _agent.onDragEnd.Add(__dragEnd);

            _useCustomAgent = false;
        }

        /// <summary>
        /// 用于实际拖动的对象。你可以根据实际情况设置对象的大小，对齐等。
        /// </summary>
        public GObject DragAgent
        {
            get { return _useCustomAgent ? _customAgent : _agent; }
        }

        /// <summary>
        /// Is dragging?
        /// 返回当前是否正在拖动。
        /// </summary>
        public bool dragging
        {
            get { return DragAgent.parent != null; }
        }

        /// <summary>
        /// Start dragging.
        /// 开始拖动。
        /// </summary>
        /// <param name="source">Source object. This is the object which initiated the dragging.</param>
        /// <param name="icon">Icon to be used as the dragging sign.</param>
        /// <param name="sourceData">Custom data. You can get it in the onDrop event data.</param>
        /// <param name="touchPointID">Copy the touchId from InputEvent to here, if has one.</param>
        public void StartDrag(GObject source, string icon, object sourceData, int touchPointID = -1)
        {
            if (DragAgent.parent != null)
                return;

            _sourceData = sourceData;
            _source = source;
            _agent.url = icon;
            _useCustomAgent = false;
            GRoot.inst.AddChild(_agent);
            _agent.xy = GRoot.inst.GlobalToLocal(Stage.inst.GetTouchPosition(touchPointID));
            _agent.StartDrag(touchPointID);
        }

        /// <summary>
        /// 开始拖动
        /// </summary>
        /// <param name="source">拖动源对象</param>
        /// <param name="customAgent">自定义拖动中的代理对象</param>
        /// <param name="sourceData">源数据</param>
        /// <param name="touchPointID">触摸ID</param>
        public void StartDrag(GObject source, GObject customAgent, object sourceData, int touchPointID = -1)
        {
            if (DragAgent.parent != null)
                return;

            _sourceData = sourceData;
            _source = source;
            _customAgent = customAgent;
            _customAgent.visible = true;
            _useCustomAgent = true;
            GRoot.inst.AddChild(_customAgent);
            _customAgent.xy = GRoot.inst.GlobalToLocal(Stage.inst.GetTouchPosition(touchPointID));
            _customAgent.StartDrag(touchPointID);
        }

        /// <summary>
        /// Cancel dragging.
        /// 取消拖动。
        /// </summary>
        public void Cancel()
        {
            if (DragAgent.parent == null)
            {
                return;
            }

            DragAgent.StopDrag();
            GRoot.inst.RemoveChild(DragAgent);
            _sourceData = null;
            _customAgent = null;
            _useCustomAgent = false;
        }

        private void __dragEnd(EventContext evt)
        {
            if (DragAgent.parent == null) //cancelled
            {
                return;
            }

            GRoot.inst.RemoveChild(DragAgent);

            object sourceData = _sourceData;
            GObject source = _source;
            _sourceData = null;
            _source = null;
            _customAgent = null;
            _useCustomAgent = false;

            GObject obj = GRoot.inst.touchTarget;
            while (obj != null)
            {
                if (obj.hasEventListeners("onDrop"))
                {
                    obj.RequestFocus();
                    obj.DispatchEvent("onDrop", sourceData, source);
                    return;
                }

                obj = obj.parent;
            }
        }
    }
}