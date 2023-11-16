using System;
using System.Collections.Generic;
using GameFrameX.Runtime;

namespace GameFrameX.Mono
{
    public class MonoManager : GameFrameworkModule, IMonoManager
    {
        private static readonly object Lock = new object();

        private Queue<Action> _updateQueue = new Queue<Action>();
        private Queue<Action> _invokeUpdateQueue = new Queue<Action>();

        private Queue<Action> _fixedUpdate = new Queue<Action>();
        private Queue<Action> _invokeFixedUpdate = new Queue<Action>();

        private Queue<Action> _lateUpdate = new Queue<Action>();
        private Queue<Action> _invokeLateUpdate = new Queue<Action>();

        private Queue<Action> _destroy = new Queue<Action>();
        private Queue<Action> _invokeDestroy = new Queue<Action>();

        private Queue<Action> _onDrawGizmos = new Queue<Action>();
        private Queue<Action> _invokeOnDrawGizmos = new Queue<Action>();

        private Queue<Action<bool>> _onApplicationPause = new Queue<Action<bool>>();
        private Queue<Action<bool>> _invokeOnApplicationPause = new Queue<Action<bool>>();

        private Queue<Action<bool>> _onApplicationFocus = new Queue<Action<bool>>();
        private Queue<Action<bool>> _invokeOnApplicationFocus = new Queue<Action<bool>>();


        /// <summary>
        /// 在固定的帧率下调用。
        /// </summary>
        public void FixedUpdate()
        {
            QueueInvoking(ref this._invokeFixedUpdate, ref this._fixedUpdate);
        }

        /// <summary>
        /// 在所有 Update 函数调用后每帧调用。
        /// </summary>
        public void LateUpdate()
        {
            QueueInvoking(ref this._invokeLateUpdate, ref this._lateUpdate);
        }

        /// <summary>
        /// 当 MonoBehaviour 将被销毁时调用。
        /// </summary>
        public void OnDestroy()
        {
            QueueInvoking(ref this._invokeDestroy, ref this._destroy);
        }

        /// <summary>
        /// 在绘制 Gizmos 时调用。
        /// </summary>
        public void OnDrawGizmos()
        {
            QueueInvoking(ref this._invokeOnDrawGizmos, ref this._onDrawGizmos);
        }

        /// <summary>
        /// 当应用程序失去或获得焦点时调用。
        /// </summary>
        /// <param name="focusStatus">应用程序的焦点状态</param>
        public void OnApplicationFocus(bool focusStatus)
        {
            QueueInvoking(ref this._invokeOnApplicationFocus, ref this._onApplicationFocus, focusStatus);
        }

        /// <summary>
        /// 当应用程序暂停或恢复时调用。
        /// </summary>
        /// <param name="pauseStatus">应用程序的暂停状态</param>
        public void OnApplicationPause(bool pauseStatus)
        {
            QueueInvoking(ref this._invokeOnApplicationPause, ref this._onApplicationPause, pauseStatus);
        }

        /// <summary>
        /// 添加一个在 LateUpdate 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void AddLateUpdateListener(Action action)
        {
            lock (Lock)
            {
                _lateUpdate.Enqueue(action);
            }
        }

        /// <summary>
        /// 从 LateUpdate 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void RemoveLateUpdateListener(Action action)
        {
            lock (Lock)
            {
                Delete(ref this._lateUpdate, action);
            }
        }

        /// <summary>
        /// 添加一个在 FixedUpdate 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void AddFixedUpdateListener(Action action)
        {
            lock (Lock)
            {
                _fixedUpdate.Enqueue(action);
            }
        }

        /// <summary>
        /// 从 FixedUpdate 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void RemoveFixedUpdateListener(Action action)
        {
            lock (Lock)
            {
                Delete(ref this._fixedUpdate, action);
            }
        }

        /// <summary>
        /// 添加一个在 Update 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void AddUpdateListener(Action action)
        {
            lock (Lock)
            {
                _updateQueue.Enqueue(action);
            }
        }

        /// <summary>
        /// 从 Update 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void RemoveUpdateListener(Action action)
        {
            lock (Lock)
            {
                Delete(ref this._updateQueue, action);
            }
        }

        /// <summary>
        /// 添加一个在 Destroy 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void AddDestroyListener(Action action)
        {
            lock (Lock)
            {
                _destroy.Enqueue(action);
            }
        }

        /// <summary>
        /// 从 Destroy 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void RemoveDestroyListener(Action action)
        {
            lock (Lock)
            {
                Delete(ref this._destroy, action);
            }
        }

        /// <summary>
        /// 添加一个在 OnDrawGizmos 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void AddOnDrawGizmosListener(Action action)
        {
            lock (Lock)
            {
                _onDrawGizmos.Enqueue(action);
            }
        }

        /// <summary>
        /// 从 OnDrawGizmos 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void RemoveOnDrawGizmosListener(Action action)
        {
            lock (Lock)
            {
                Delete(ref this._onDrawGizmos, action);
            }
        }

        /// <summary>
        /// 添加一个在 OnApplicationPause 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void AddOnApplicationPauseListener(Action<bool> action)
        {
            lock (Lock)
            {
                _onApplicationPause.Enqueue(action);
            }
        }

        /// <summary>
        /// 从 OnApplicationPause 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void RemoveOnApplicationPauseListener(Action<bool> action)
        {
            lock (Lock)
            {
                Delete(ref this._onApplicationPause, action);
            }
        }

        /// <summary>
        /// 添加一个在 OnApplicationFocus 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void AddOnApplicationFocusListener(Action<bool> action)
        {
            lock (Lock)
            {
                _onApplicationFocus.Enqueue(action);
            }
        }

        /// <summary>
        /// 从 OnApplicationFocus 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void RemoveOnApplicationFocusListener(Action<bool> action)
        {
            lock (Lock)
            {
                Delete(ref this._onApplicationFocus, action);
            }
        }

        public void Release()
        {
            this._updateQueue.Clear();
            this._destroy.Clear();
            this._fixedUpdate.Clear();
            this._lateUpdate.Clear();
            this._onApplicationFocus.Clear();
            this._onApplicationPause.Clear();
            this._onDrawGizmos.Clear();
        }

        private static void QueueInvoking(ref Queue<Action> a, ref Queue<Action> b)
        {
            lock (Lock)
            {
                ObjectHelper.Swap(ref a, ref b);

                while (a.Count > 0)
                {
                    var action = a.Dequeue();
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }

        private static void QueueInvoking(ref Queue<Action<bool>> a, ref Queue<Action<bool>> b, bool value)
        {
            lock (Lock)
            {
                ObjectHelper.Swap(ref a, ref b);

                while (a.Count > 0)
                {
                    var action = a.Dequeue();
                    try
                    {
                        action.Invoke(value);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }

        private static void Delete<T>(ref Queue<T> queue, T item)
        {
            lock (Lock)
            {
                // 创建一个临时队列
                Queue<T> tempQueue = new Queue<T>();

                // 将要删除的对象之前的所有对象从原始队列移到临时队列
                while (queue.Peek().Equals(item) == false)
                {
                    tempQueue.Enqueue(queue.Dequeue());
                }

                // 跳过要删除的对象
                queue.Dequeue();

                // 将临时队列中的元素移到原始队列
                while (tempQueue.Count > 0)
                {
                    tempQueue.Enqueue(queue.Dequeue());
                }

                ObjectHelper.Swap(ref queue, ref tempQueue);
                tempQueue.Clear();
            }
        }


        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            QueueInvoking(ref this._invokeUpdateQueue, ref this._updateQueue);
        }

        internal override void Shutdown()
        {
            this._updateQueue.Clear();
            this._destroy.Clear();
            this._fixedUpdate.Clear();
            this._lateUpdate.Clear();
            this._onApplicationFocus.Clear();
            this._onApplicationPause.Clear();
            this._onDrawGizmos.Clear();
        }
    }
}