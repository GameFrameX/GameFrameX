using UnityGameFramework.Runtime;

namespace UnityGameFramework.Procedure
{
    public static class PatchEventDispatcher
    {
        public static void SendPatchStepsChangeMsg(EPatchStates currentStates)
        {
            PatchEventMessageDefine.PatchStatesChange msg = new PatchEventMessageDefine.PatchStatesChange();
            msg.CurrentStates = currentStates;
            // GameApp.EventSystem.Run(EventIdType.YooAssetPatchStatesChange, msg);
        }

        public static void SendFoundUpdateFilesMsg(int totalCount, long totalSizeBytes)
        {
            PatchEventMessageDefine.FoundUpdateFiles msg = new PatchEventMessageDefine.FoundUpdateFiles();
            msg.TotalCount = totalCount;
            msg.TotalSizeBytes = totalSizeBytes;
            // GameEntry.GetComponent<EventComponent>().Fire();.Run(EventIdType.YooAssetFoundUpdateFiles, msg);
        }

        public static void SendDownloadProgressUpdateMsg(int totalDownloadCount, int currentDownloadCount, long totalDownloadSizeBytes,
        long currentDownloadSizeBytes)
        {
            PatchEventMessageDefine.DownloadProgressUpdate msg = new PatchEventMessageDefine.DownloadProgressUpdate();
            msg.TotalDownloadCount = totalDownloadCount;
            msg.CurrentDownloadCount = currentDownloadCount;
            msg.TotalDownloadSizeBytes = totalDownloadSizeBytes;
            msg.CurrentDownloadSizeBytes = currentDownloadSizeBytes;
            LauncherUIHandler.SetProgressUpdate(msg);
            // GameApp.EventSystem.Run(EventIdType.YooAssetDownloadProgressUpdate, msg);
        }

        public static void SendStaticVersionUpdateFailedMsg()
        {
            PatchEventMessageDefine.StaticVersionUpdateFailed msg = new PatchEventMessageDefine.StaticVersionUpdateFailed();
            // LauncherUIHandler.SetTipText(msg);
            // LauncherUIHandler.SetProgressUpdate(msg);
            // GameApp.EventSystem.Run(EventIdType.YooAssetStaticVersionUpdateFailed, msg);
        }

        public static void SendPatchManifestUpdateFailedMsg()
        {
            PatchEventMessageDefine.PatchManifestUpdateFailed msg = new PatchEventMessageDefine.PatchManifestUpdateFailed();
            // GameApp.EventSystem.Run(EventIdType.YooAssetPatchManifestUpdateFailed, msg);
        }

        public static void SendWebFileDownloadFailedMsg(string fileName, string error)
        {
            PatchEventMessageDefine.WebFileDownloadFailed msg = new PatchEventMessageDefine.WebFileDownloadFailed();
            msg.FileName = fileName;
            msg.Error = error;
            
            // GameApp.EventSystem.Run(EventIdType.YooAssetWebFileDownloadFailed, msg);
        }
    }
}