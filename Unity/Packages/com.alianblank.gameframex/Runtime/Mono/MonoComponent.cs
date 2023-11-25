using System;
using GameFrameX.Event;
using GameFrameX.Mono;
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
        private IMonoManager _monoManager;
        private IEventManager _eventManager;

        protected override void Awake()
        {
            base.Awake();
            new MonoManager();
            _monoManager = GameFrameworkEntry.GetModule<IMonoManager>();
            if (_monoManager == null)
            {
                Log.Fatal("Mono manager is invalid.");
                return;
            }

            _eventManager = GameFrameworkEntry.GetModule<IEventManager>();
            if (_eventManager == null)
            {
                Log.Fatal("Event manager is invalid.");
                return;
            }
        }

        /// <summary>
        /// 在固定的帧率下调用。
        /// </summary>
        private void FixedUpdate()
        {
            _monoManager.FixedUpdate();
        }

        /// <summary>
        /// 在所有 Update 函数调用后每帧调用。
        /// </summary>
        private void LateUpdate()
        {
            _monoManager.LateUpdate();
        }

        /// <summary>
        /// 当 MonoBehaviour 将被销毁时调用。
        /// </summary>
        private void OnDestroy()
        {
            _monoManager.OnDestroy();
        }

        /// <summary>
        /// 当应用程序失去或获得焦点时调用。
        /// </summary>
        /// <param name="focusStatus">应用程序的焦点状态</param>
        private void OnApplicationFocus(bool focusStatus)
        {
            _monoManager.OnApplicationFocus(focusStatus);
            _eventManager.Fire(this, OnApplicationFocusChangedEventArgs.Create(focusStatus));
        }

        /// <summary>
        /// 当应用程序暂停或恢复时调用。
        /// </summary>
        /// <param name="pauseStatus">应用程序的暂停状态</param>
        private void OnApplicationPause(bool pauseStatus)
        {
            _monoManager.OnApplicationPause(pauseStatus);
            _eventManager.Fire(this, OnApplicationPauseChangedEventArgs.Create(pauseStatus));
        }

        /// <summary>
        /// 添加 LateUpdate 监听器
        /// </summary>
        /// <param name="fun">要添加的 LateUpdate 监听器回调函数</param>
        public void AddLateUpdateListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.AddLateUpdateListener(fun);
        }

        /// <summary>
        /// 移除 LateUpdate 监听器
        /// </summary>
        /// <param name="fun">要移除的 LateUpdate 监听器回调函数</param>
        public void RemoveLateUpdateListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.RemoveLateUpdateListener(fun);
        }

        /// <summary>
        /// 添加 OnApplicationFocus 监听器
        /// </summary>
        /// <param name="fun">要添加的 OnApplicationFocus 监听器回调函数</param>
        public void AddFixedUpdateListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.AddFixedUpdateListener(fun);
        }

        /// <summary>
        /// 移除 OnApplicationFocus 监听器
        /// </summary>
        /// <param name="fun">要移除的 OnApplicationFocus 监听器回调函数</param>
        public void RemoveFixedUpdateListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.RemoveFixedUpdateListener(fun);
        }

        /// <summary>
        /// 添加 Update 监听器
        /// </summary>
        /// <param name="fun">要添加的 Update 监听器回调函数</param>
        public void AddUpdateListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.AddUpdateListener(fun);
        }

        /// <summary>
        /// 移除 Update 监听器
        /// </summary>
        /// <param name="fun">要移除的 Update 监听器回调函数</param>
        public void RemoveUpdateListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.RemoveUpdateListener(fun);
        }

        /// <summary>
        /// 添加 Destroy 监听器
        /// </summary>
        /// <param name="fun">要添加的 Destroy 监听器回调函数</param>
        public void AddDestroyListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.AddDestroyListener(fun);
        }

        /// <summary>
        /// 移除 Destroy 监听器
        /// </summary>
        /// <param name="fun">要移除的 Destroy 监听器回调函数</param>
        public void RemoveDestroyListener(Action fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.RemoveDestroyListener(fun);
        }

        /// <summary>
        /// 添加 OnApplicationPause 监听器
        /// </summary>
        /// <param name="fun">要添加的 OnApplicationPause 监听器回调函数</param>
        public void AddOnApplicationPauseListener(Action<bool> fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.AddOnApplicationPauseListener(fun);
        }

        /// <summary>
        /// 移除 OnApplicationPause 监听器
        /// </summary>
        /// <param name="fun">要移除的 OnApplicationPause 监听器回调函数</param>
        public void RemoveOnApplicationPauseListener(Action<bool> fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.RemoveOnApplicationPauseListener(fun);
        }

        /// <summary>
        /// 添加 OnApplicationFocus 监听器
        /// </summary>
        /// <param name="fun">要添加的 OnApplicationFocus 监听器回调函数</param>
        public void AddOnApplicationFocusListener(Action<bool> fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.AddOnApplicationFocusListener(fun);
        }

        /// <summary>
        /// 移除 OnApplicationFocus 监听器
        /// </summary>
        /// <param name="fun">要移除的 OnApplicationFocus 监听器回调函数</param>
        public void RemoveOnApplicationFocusListener(Action<bool> fun)
        {
            if (fun == null)
            {
                Log.Fatal(nameof(fun) + "is invalid.");
                return;
            }

            _monoManager.RemoveOnApplicationFocusListener(fun);
        }
    }
}