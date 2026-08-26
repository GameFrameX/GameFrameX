using Cysharp.Threading.Tasks;
using GameFrameX.Startup.Runtime;

namespace GameFrameX.Startup.Application
{
    /// <summary>
    /// 应用层启动 UI 处理实现，将 <see cref="LauncherUIHandler"/> 适配到 <see cref="IStartupUIHandler"/> 接口。
    /// </summary>
    /// <remarks>
    /// Application startup UI handler that adapts <see cref="LauncherUIHandler"/> to the <see cref="IStartupUIHandler"/> interface.
    /// </remarks>
    public sealed class ApplicationStartupUIHandler : IStartupUIHandler
    {
        /// <summary>
        /// 异步打开启动 UI。
        /// </summary>
        /// <remarks>
        /// Asynchronously open the startup UI.
        /// </remarks>
        /// <param name="uiResName">启动 UI 资源名称 / Startup UI asset name</param>
        /// <returns>异步任务 / The async task</returns>
        public UniTask StartAsync(string uiResName)
        {
            return LauncherUIHandler.StartAsync(uiResName);
        }

        /// <summary>
        /// 设置提示文本。
        /// </summary>
        /// <remarks>
        /// Set the tip text.
        /// </remarks>
        /// <param name="text">要显示的提示文本 / The tip text to display</param>
        public void SetTipText(string text)
        {
            LauncherUIHandler.SetTipText(text);
        }

        /// <summary>
        /// 设置下载进度。
        /// </summary>
        /// <remarks>
        /// Set the download progress.
        /// </remarks>
        /// <param name="progress">下载进度，取值范围 [0, 1] / Download progress in range [0, 1]</param>
        /// <param name="sizeInfo">已下载与总大小信息字符串 / Downloaded size and total size info string</param>
        public void SetProgress(float progress, string sizeInfo)
        {
            LauncherUIHandler.SetProgress(progress, sizeInfo);
        }

        /// <summary>
        /// 标记下载流程完成。
        /// </summary>
        /// <remarks>
        /// Mark the download process as finished.
        /// </remarks>
        public void SetProgressUpdateFinish()
        {
            LauncherUIHandler.SetProgressUpdateFinish();
        }

        /// <summary>
        /// 异步显示强制或可选更新提示。
        /// </summary>
        /// <remarks>
        /// Asynchronously show the force or optional update prompt.
        /// </remarks>
        /// <param name="upgradeInfo">启动器更新信息 / The launcher upgrade info</param>
        /// <returns>异步任务，返回用户是否继续 / The async task returning whether the user continues</returns>
        public UniTask<bool> ShowUpgradeAsync(StartupUpgradeInfo upgradeInfo)
        {
            return LauncherUIHandler.ShowUpgradeAsync(upgradeInfo);
        }

        /// <summary>
        /// 关闭启动 UI 并释放资源。
        /// </summary>
        /// <remarks>
        /// Close the startup UI and release resources.
        /// </remarks>
        public void Dispose()
        {
            LauncherUIHandler.Dispose();
        }
    }
}
