using System.Collections;
using Cysharp.Threading.Tasks;
using GameFramework.Fsm;
using GameFramework.Procedure;
using YooAsset;

namespace UnityGameFramework.Procedure
{
    internal sealed class ProcedureDownloadWebFiles : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.DownloadWebFiles);
            BeginDownload(procedureOwner).ToUniTask();
        }


        private IEnumerator BeginDownload(IFsm<IProcedureManager> procedureOwner)
        {
            var downloader = PatchUpdater.Downloader;

            // 注册下载回调
            downloader.OnDownloadErrorCallback = (name, error) =>
            {
                PatchEventDispatcher.SendWebFileDownloadFailedMsg(name, error);
                ChangeState<ProcedureCreateDownloader>(procedureOwner);
            };
            downloader.OnDownloadProgressCallback = PatchEventDispatcher.SendDownloadProgressUpdateMsg;
            downloader.BeginDownload();
            yield return downloader;

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
            {
                yield break;
            }

            ChangeState<ProcedurePatchDone>(procedureOwner);
        }
    }
}