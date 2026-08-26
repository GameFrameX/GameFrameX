using Cysharp.Threading.Tasks;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;
using UnityEngine;

namespace GameFrameX.Startup.Application
{
    /// <summary>
    /// 应用启动入口 MonoBehaviour，负责在场景加载后执行启动流程。
    /// </summary>
    /// <remarks>
    /// Application startup entry MonoBehaviour responsible for running the startup process after the scene is loaded.
    /// </remarks>
    public sealed class ApplicationStartupEntry : MonoBehaviour
    {
        [SerializeField] private StartupOptions startupOptions;

        private async void Start()
        {
            await RunAsync();
        }

        private async UniTask RunAsync()
        {
            try
            {
                if (startupOptions == null)
                {
                    Log.Error("StartupOptions is not assigned.");
                    return;
                }

                startupOptions.Channel = BlankGetChannel.GetChannelName();
                startupOptions.SubChannel = BlankGetChannel.GetChannelName("sub_channel", "default");
                startupOptions.PackageName = UnityEngine.Application.identifier;
#if ENABLE_UI_UGUI
                startupOptions.LauncherUIResName = "UI/UILauncher/UGUI/UILauncher";
#endif
#if ENABLE_UI_FAIRYGUI
                startupOptions.LauncherUIResName = "UI/UILauncher";
#endif
                await UniTask.DelayFrame(10);
                var result = await StartupRunner.Run(startupOptions, new ApplicationStartupUIHandler(), new ApplicationHotfixLauncher());

                if (!result.Success)
                {
                    Log.Error($"Startup failed. Procedure: {result.FailedProcedureName}, Url: {result.FailedUrl}, Error: {result.ErrorMessage}");
                }
            }
            catch (System.Exception exception)
            {
                Log.Error(exception);
            }
        }
    }
}