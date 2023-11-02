using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// Mono 组件
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Mono")]
    public class MonoComponent : GameFrameworkComponent
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


        private void FixedUpdate()
        {
            QueueInvoking(ref this._invokeFixedUpdate, ref this._fixedUpdate);
        }

        private void LateUpdate()
        {
            QueueInvoking(ref this._invokeLateUpdate, ref this._lateUpdate);
        }

        private void OnDestroy()
        {
            QueueInvoking(ref this._invokeDestroy, ref this._destroy);
        }

        private void OnDrawGizmos()
        {
            QueueInvoking(ref this._invokeOnDrawGizmos, ref this._onDrawGizmos);
        }

        private void OnApplicationFocus(bool focusStatus)
        {
            QueueInvoking(ref this._invokeOnApplicationFocus, ref this._onApplicationFocus, focusStatus);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            QueueInvoking(ref this._invokeOnApplicationPause, ref this._onApplicationPause, pauseStatus);
        }

        public void AddLateUpdateListener(Action fun)
        {
            lock (Lock)
            {
                _lateUpdate.Enqueue(fun);
            }
        }

        public void RemoveLateUpdateListener(Action fun)
        {
            lock (Lock)
            {
                Delete(ref this._lateUpdate, fun);
            }
        }

        public void AddFixedUpdateListener(Action fun)
        {
            lock (Lock)
            {
                _fixedUpdate.Enqueue(fun);
            }
        }

        public void RemoveFixedUpdateListener(Action fun)
        {
            lock (Lock)
            {
                Delete(ref this._fixedUpdate, fun);
            }
        }

        public void AddUpdateListener(Action fun)
        {
            lock (Lock)
            {
                _updateQueue.Enqueue(fun);
            }
        }

        public void RemoveUpdateListener(Action fun)
        {
            lock (Lock)
            {
                Delete(ref this._updateQueue, fun);
            }
        }

        public void AddDestroyListener(Action fun)
        {
            lock (Lock)
            {
                _destroy.Enqueue(fun);
            }
        }

        public void RemoveDestroyListener(Action fun)
        {
            lock (Lock)
            {
                Delete(ref this._destroy, fun);
            }
        }

        public void AddOnDrawGizmosListener(Action fun)
        {
            lock (Lock)
            {
                _onDrawGizmos.Enqueue(fun);
            }
        }

        public void RemoveOnDrawGizmosListener(Action fun)
        {
            lock (Lock)
            {
                Delete(ref this._onDrawGizmos, fun);
            }
        }

        public void AddOnApplicationPauseListener(Action<bool> fun)
        {
            lock (Lock)
            {
                _onApplicationPause.Enqueue(fun);
            }
        }

        public void RemoveOnApplicationPauseListener(Action<bool> fun)
        {
            lock (Lock)
            {
                Delete(ref this._onApplicationPause, fun);
            }
        }

        public void AddOnApplicationFocusListener(Action<bool> fun)
        {
            lock (Lock)
            {
                _onApplicationFocus.Enqueue(fun);
            }
        }

        public void RemoveOnApplicationFocusListener(Action<bool> fun)
        {
            lock (Lock)
            {
                Delete(ref this._onApplicationFocus, fun);
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

        void Update()
        {
            QueueInvoking(ref this._invokeUpdateQueue, ref this._updateQueue);
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
    }
}