using FairyGUI;
using Game.Model;
using GameFramework;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Procedure
{
    public static class LauncherUIHandler
    {
        private static UILauncher _ui;

        public static void Start()
        {
            _ui = GameApp.UI.AddToFullScreen(UILauncher.CreateInstance, "UI/UILauncher/UILauncher", UILayer.Loading);
        }

        public static void Dispose()
        {
            GameApp.UI.Remove(UILauncher.UIResName, UILayer.Loading);
            _ui.Dispose();
            _ui = null;
        }

        public static void SetTipText(string text)
        {
            _ui.m_TipText.text = text;
        }

        public static void SetProgressUpdateFinish()
        {
            _ui.m_IsDownload.SetSelectedIndex(0);
        }

        public static void SetProgressUpdate(PatchEventMessageDefine.DownloadProgressUpdate message)
        {
            _ui.m_IsDownload.SetSelectedIndex(1);
            float progress = message.CurrentDownloadSizeBytes / (message.TotalDownloadSizeBytes * 1f);
            string currentSizeMb = Utility.File.GetBytesSize(message.CurrentDownloadSizeBytes);
            string totalSizeMb = Utility.File.GetBytesSize(message.TotalDownloadSizeBytes);
            _ui.m_ProgressBar.value = progress * 100;
            _ui.m_TipText.text = $"Downloading {currentSizeMb}/{totalSizeMb}";
        }
    }
}