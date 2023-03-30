using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;
using YooAsset;

namespace UnityGameFramework.Procedure
{
    internal sealed class ProcedureCreateDownloader : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.CreateDownloader);
            CreateDownloader(procedureOwner);
        }


        void CreateDownloader(IFsm<IProcedureManager> procedureOwner)
        {
            // Debug.Log("创建补丁下载器.");
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            PatchUpdater.Downloader = YooAssets.CreatePatchDownloader(downloadingMaxNum, failedTryAgain);
            if (PatchUpdater.Downloader.TotalDownloadCount == 0)
            {
                Debug.Log("没有发现需要下载的资源");
                ChangeState<ProcedurePatchDone>(procedureOwner);
            }
            else
            {
                Debug.Log($"一共发现了{PatchUpdater.Downloader.TotalDownloadCount}个资源需要更新下载。");

                // 发现新更新文件后，挂起流程系统
                int totalDownloadCount = PatchUpdater.Downloader.TotalDownloadCount;
                long totalDownloadBytes = PatchUpdater.Downloader.TotalDownloadBytes;
                PatchEventDispatcher.SendFoundUpdateFilesMsg(totalDownloadCount, totalDownloadBytes);
                ChangeState<ProcedureDownloadWebFiles>(procedureOwner);
            }
        }
    }
}