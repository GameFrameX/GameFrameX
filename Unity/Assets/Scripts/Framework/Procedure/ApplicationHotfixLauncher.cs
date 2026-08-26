using Cysharp.Threading.Tasks;
using GameFrameX.Startup.Runtime;

namespace GameFrameX.Startup.Application
{
    /// <summary>
    /// 应用层热更启动器，将 <see cref="HotfixHelper"/> 适配到 <see cref="IHotfixLauncher"/> 接口。
    /// </summary>
    /// <remarks>
    /// Application hotfix launcher that adapts <see cref="HotfixHelper"/> to the <see cref="IHotfixLauncher"/> interface.
    /// </remarks>
    public sealed class ApplicationHotfixLauncher : IHotfixLauncher
    {
        /// <summary>
        /// 异步启动热更程序集。
        /// </summary>
        /// <remarks>
        /// Asynchronously start the hotfix assembly.
        /// </remarks>
        /// <param name="options">启动选项 / Startup options</param>
        /// <returns>包含启动结果的 <see cref="HotfixLaunchResult"/> / The <see cref="HotfixLaunchResult"/> containing the startup result</returns>
        public UniTask<HotfixLaunchResult> StartAsync(StartupOptions options)
        {
            return HotfixHelper.StartHotfixAsync(options);
        }
    }
}
