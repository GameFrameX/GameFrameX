//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace GameFrameX
{
    /// <summary>
    /// 事件池。
    /// </summary>
    /// <typeparam name="T">事件类型。</typeparam>
    public sealed partial class EventPool<T> where T : BaseEventArgs
    {
        private readonly object _lock = new object();
        private readonly GameFrameworkMultiDictionary<string, EventHandler<T>> _eventHandlers;
        private readonly Queue<EventNode> _events;
        private readonly Dictionary<object, LinkedListNode<EventHandler<T>>> _cachedNodes;
        private readonly Dictionary<object, LinkedListNode<EventHandler<T>>> _tempNodes;
        private readonly EventPoolMode _eventPoolMode;
        private EventHandler<T> _defaultHandler;

        /// <summary>
        /// 初始化事件池的新实例。
        /// </summary>
        /// <param name="mode">事件池模式。</param>
        public EventPool(EventPoolMode mode)
        {
            _eventHandlers = new GameFrameworkMultiDictionary<string, EventHandler<T>>();
            _events = new Queue<EventNode>();
            _cachedNodes = new Dictionary<object, LinkedListNode<EventHandler<T>>>();
            _tempNodes = new Dictionary<object, LinkedListNode<EventHandler<T>>>();
            _eventPoolMode = mode;
            _defaultHandler = null;
        }

        /// <summary>
        /// 获取事件处理函数的数量。
        /// </summary>
        public int EventHandlerCount
        {
            get
            {
                lock (_lock)
                {
                    return _eventHandlers.Count;
                }
            }
        }

        /// <summary>
        /// 获取事件数量。
        /// </summary>
        public int EventCount
        {
            get
            {
                lock (_lock)
                {
                    return _events.Count;
                }
            }
        }

        /// <summary>
        /// 事件池轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            lock (_lock)
            {
                while (_events.Count > 0)
                {
                    EventNode eventNodeNode = _events.Dequeue();
                    HandleEvent(eventNodeNode.Sender, eventNodeNode.EventArgs);
                    ReferencePool.Release(eventNodeNode);
                }
            }
        }

        /// <summary>
        /// 关闭并清理事件池。
        /// </summary>
        public void Shutdown()
        {
            Clear();
            _eventHandlers.Clear();
            _cachedNodes.Clear();
            _tempNodes.Clear();
            _defaultHandler = null;
        }

        /// <summary>
        /// 清理事件。
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _events.Clear();
            }
        }

        /// <summary>
        /// 获取事件处理函数的数量。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <returns>事件处理函数的数量。</returns>
        public int Count(string id)
        {
            lock (_lock)
            {
                if (_eventHandlers.TryGetValue(id, out var listRange))
                {
                    return listRange.Count;
                }

                return 0;
            }
        }

        /// <summary>
        /// 检查是否存在事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要检查的事件处理函数。</param>
        /// <returns>是否存在事件处理函数。</returns>
        public bool Check(string id, EventHandler<T> handler)
        {
            lock (_lock)
            {
                if (handler == null)
                {
                    throw new GameFrameworkException("Event handler is invalid.");
                }

                return _eventHandlers.Contains(id, handler);
            }
        }

        /// <summary>
        /// 订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        public void Subscribe(string id, EventHandler<T> handler)
        {
            lock (_lock)
            {
                if (handler == null)
                {
                    throw new GameFrameworkException("Event handler is invalid.");
                }

                if (!_eventHandlers.Contains(id))
                {
                    _eventHandlers.Add(id, handler);
                }
                else if ((_eventPoolMode & EventPoolMode.AllowMultiHandler) != EventPoolMode.AllowMultiHandler)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Event '{0}' not allow multi handler.", id));
                }
                else if ((_eventPoolMode & EventPoolMode.AllowDuplicateHandler) != EventPoolMode.AllowDuplicateHandler && Check(id, handler))
                {
                    throw new GameFrameworkException(Utility.Text.Format("Event '{0}' not allow duplicate handler.", id));
                }
                else
                {
                    _eventHandlers.Add(id, handler);
                }
            }
        }

        /// <summary>
        /// 取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        public void Unsubscribe(string id, EventHandler<T> handler)
        {
            lock (_lock)
            {
                if (handler == null)
                {
                    throw new GameFrameworkException("Event handler is invalid.");
                }

                if (_cachedNodes.Count > 0)
                {
                    foreach (KeyValuePair<object, LinkedListNode<EventHandler<T>>> cachedNode in _cachedNodes)
                    {
                        if (cachedNode.Value != null && cachedNode.Value.Value == handler)
                        {
                            _tempNodes.Add(cachedNode.Key, cachedNode.Value.Next);
                        }
                    }

                    if (_tempNodes.Count > 0)
                    {
                        foreach (KeyValuePair<object, LinkedListNode<EventHandler<T>>> cachedNode in _tempNodes)
                        {
                            _cachedNodes[cachedNode.Key] = cachedNode.Value;
                        }

                        _tempNodes.Clear();
                    }
                }

                if (!_eventHandlers.Remove(id, handler))
                {
                    throw new GameFrameworkException(Utility.Text.Format("Event '{0}' not exists specified handler.", id));
                }
            }
        }

        /// <summary>
        /// 设置默认事件处理函数。
        /// </summary>
        /// <param name="handler">要设置的默认事件处理函数。</param>
        public void SetDefaultHandler(EventHandler<T> handler)
        {
            _defaultHandler = handler;
        }

        /// <summary>
        /// 抛出事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        public void Fire(object sender, T e)
        {
            lock (_lock)
            {
                if (e == null)
                {
                    throw new GameFrameworkException("Event is invalid.");
                }

                EventNode eventNodeNode = EventNode.Create(sender, e);
                lock (_events)
                {
                    _events.Enqueue(eventNodeNode);
                }
            }
        }

        /// <summary>
        /// 抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        public void FireNow(object sender, T e)
        {
            lock (_lock)
            {
                if (e == null)
                {
                    throw new GameFrameworkException("Event is invalid.");
                }

                HandleEvent(sender, e);
            }
        }

        /// <summary>
        /// 处理事件结点。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private void HandleEvent(object sender, T e)
        {
            lock (_lock)
            {
                bool noHandlerException = false;
                if (_eventHandlers.TryGetValue(e.Id, out var range))
                {
                    LinkedListNode<EventHandler<T>> current = range.First;
                    while (current != null && current != range.Terminal)
                    {
                        _cachedNodes[e] = current.Next != range.Terminal ? current.Next : null;
                        current.Value?.Invoke(sender, e);
                        current = _cachedNodes[e];
                    }

                    _cachedNodes.Remove(e);
                }
                else if (_defaultHandler != null)
                {
                    _defaultHandler(sender, e);
                }
                else if ((_eventPoolMode & EventPoolMode.AllowNoHandler) == 0)
                {
                    noHandlerException = true;
                }

                ReferencePool.Release(e);

                if (noHandlerException)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Event '{0}' not allow no handler.", e.Id));
                }
            }
        }
    }
}