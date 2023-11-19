using System;
using System.Collections.Generic;
using GameFrameX.Runtime;

namespace GameFrameX.Mono
{
    public class MonoManager : GameFrameworkModule, IMonoManager
    {
        private static readonly object Lock = new object();

        private List<Action> _updateQueue = new List<Action>();
        private List<Action> _invokeUpdateQueue = new List<Action>();

        private List<Action> _fixedUpdate = new List<Action>();
        private List<Action> _invokeFixedUpdate = new List<Action>();

        private List<Action> _lateUpdate = new List<Action>();
        private List<Action> _invokeLateUpdate = new List<Action>();

        private List<Action> _destroy = new List<Action>();
        private List<Action> _invokeDestroy = new List<Action>();

        private List<Action<bool>> _onApplicationPause = new List<Action<bool>>();
        private List<Action<bool>> _invokeOnApplicationPause = new List<Action<bool>>();

        private List<Action<bool>> _onApplicationFocus = new List<Action<bool>>();
        private List<Action<bool>> _invokeOnApplicationFocus = new List<Action<bool>>();


        /// <summary>
        /// 在固定的帧率下调用。
        /// </summary>
        public void FixedUpdate()
        {
            QueueInvoking(this._invokeFixedUpdate, this._fixedUpdate);
        }

        /// <summary>
        /// 在所有 Update 函数调用后每帧调用。
        /// </summary>
        public void LateUpdate()
        {
            QueueInvoking(this._invokeLateUpdate, this._lateUpdate);
        }

        /// <summary>
        /// 当 MonoBehaviour 将被销毁时调用。
        /// </summary>
        public void OnDestroy()
        {
            QueueInvoking(this._invokeDestroy, this._destroy);
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
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (Lock)
            {
                _lateUpdate.Add(action);
            }
        }

        /// <summary>
        /// 从 LateUpdate 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void RemoveLateUpdateListener(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (Lock)
            {
                _lateUpdate.Remove(action);
            }
        }

        /// <summary>
        /// 添加一个在 FixedUpdate 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void AddFixedUpdateListener(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (Lock)
            {
                _fixedUpdate.Add(action);
            }
        }

        /// <summary>
        /// 从 FixedUpdate 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void RemoveFixedUpdateListener(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (Lock)
            {
                this._fixedUpdate.Remove(action);
            }
        }

        /// <summary>
        /// 添加一个在 Update 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void AddUpdateListener(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (Lock)
            {
                _updateQueue.Add(action);
            }
        }

        /// <summary>
        /// 从 Update 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void RemoveUpdateListener(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (Lock)
            {
                _updateQueue.Remove(action);
            }
        }

        /// <summary>
        /// 添加一个在 Destroy 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void AddDestroyListener(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (Lock)
            {
                _destroy.Add(action);
            }
        }

        /// <summary>
        /// 从 Destroy 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void RemoveDestroyListener(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (Lock)
            {
                _destroy.Remove(action);
            }
        }

        /// <summary>
        /// 添加一个在 OnApplicationPause 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void AddOnApplicationPauseListener(Action<bool> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (Lock)
            {
                _onApplicationPause.Add(action);
            }
        }

        /// <summary>
        /// 从 OnApplicationPause 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void RemoveOnApplicationPauseListener(Action<bool> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (Lock)
            {
                _onApplicationPause.Remove(action);
            }
        }

        /// <summary>
        /// 添加一个在 OnApplicationFocus 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void AddOnApplicationFocusListener(Action<bool> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (Lock)
            {
                _onApplicationFocus.Add(action);
            }
        }

        /// <summary>
        /// 从 OnApplicationFocus 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        public void RemoveOnApplicationFocusListener(Action<bool> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (Lock)
            {
                _onApplicationFocus.Remove(action);
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
        }


        private static void QueueInvoking(List<Action> a, List<Action> b)
        {
            lock (Lock)
            {
                ObjectHelper.Swap(ref a, ref b);

                foreach (var action in a)
                {
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

        private static void QueueInvoking(ref List<Action<bool>> a, ref List<Action<bool>> b, bool value)
        {
            lock (Lock)
            {
                ObjectHelper.Swap(ref a, ref b);

                foreach (var action in a)
                {
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

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            QueueInvoking(this._invokeUpdateQueue, this._updateQueue);
        }

        internal override void Shutdown()
        {
            this._updateQueue.Clear();
            this._fixedUpdate.Clear();
            this._lateUpdate.Clear();
            this._onApplicationFocus.Clear();
            this._onApplicationPause.Clear();
            this._destroy.Clear();
        }
    }
}