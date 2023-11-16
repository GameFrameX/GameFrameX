using System;

namespace GameFrameX.Mono
{
    /// <summary>
    /// 用于管理 MonoBehaviour 的接口。
    /// </summary>
    public interface IMonoManager
    {
        /// <summary>
        /// 在固定的帧率下调用。
        /// </summary>
        void FixedUpdate();

        /// <summary>
        /// 在所有 Update 函数调用后每帧调用。
        /// </summary>
        void LateUpdate();

        /// <summary>
        /// 当 MonoBehaviour 将被销毁时调用。
        /// </summary>
        void OnDestroy();

        /// <summary>
        /// 在绘制 Gizmos 时调用。
        /// </summary>
        void OnDrawGizmos();

        /// <summary>
        /// 当应用程序失去或获得焦点时调用。
        /// </summary>
        /// <param name="focusStatus">应用程序的焦点状态</param>
        void OnApplicationFocus(bool focusStatus);

        /// <summary>
        /// 当应用程序暂停或恢复时调用。
        /// </summary>
        /// <param name="pauseStatus">应用程序的暂停状态</param>
        void OnApplicationPause(bool pauseStatus);

        /// <summary>
        /// 添加一个在 LateUpdate 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        void AddLateUpdateListener(Action action);

        /// <summary>
        /// 从 LateUpdate 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        void RemoveLateUpdateListener(Action action);

        /// <summary>
        /// 添加一个在 FixedUpdate 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        void AddFixedUpdateListener(Action action);

        /// <summary>
        /// 从 FixedUpdate 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        void RemoveFixedUpdateListener(Action action);

        /// <summary>
        /// 添加一个在 Update 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        void AddUpdateListener(Action action);

        /// <summary>
        /// 从 Update 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        void RemoveUpdateListener(Action action);

        /// <summary>
        /// 添加一个在 Destroy 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        void AddDestroyListener(Action action);

        /// <summary>
        /// 从 Destroy 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        void RemoveDestroyListener(Action action);

        /// <summary>
        /// 添加一个在 OnDrawGizmos 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        void AddOnDrawGizmosListener(Action action);

        /// <summary>
        /// 从 OnDrawGizmos 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        void RemoveOnDrawGizmosListener(Action action);

        /// <summary>
        /// 添加一个在 OnApplicationPause 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        void AddOnApplicationPauseListener(Action<bool> action);

        /// <summary>
        /// 从 OnApplicationPause 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        void RemoveOnApplicationPauseListener(Action<bool> action);

        /// <summary>
        /// 添加一个在 OnApplicationFocus 期间调用的监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        void AddOnApplicationFocusListener(Action<bool> action);

        /// <summary>
        /// 从 OnApplicationFocus 中移除一个监听器。
        /// </summary>
        /// <param name="action">监听器函数</param>
        void RemoveOnApplicationFocusListener(Action<bool> action);
    }
}