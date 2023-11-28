using System.Collections;
using Cysharp.Threading.Tasks;
using GameFrameX.Fsm;
using GameFrameX.Procedure;
using GameFrameX.Runtime;
using UnityEngine;
using YooAsset;

namespace GameFrameX.Procedure
{
    internal sealed class ProcedureUpdateManifest : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.UpdateManifest);
            UpdateManifest(procedureOwner).ToUniTask();
        }


        private IEnumerator UpdateManifest(IFsm<IProcedureManager> procedureOwner)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            var package = YooAssets.GetPackage(AssetComponent.BuildInPackageName);
            var operation = package.UpdatePackageManifestAsync(GameApp.Asset.StaticVersion);
            yield return operation;

            if (operation.Status == EOperationStatus.Succeed)
            {
                //更新成功
                ChangeState<ProcedureCreateDownloader>(procedureOwner);
            }
            else
            {
                //更新失败
                Debug.LogError(operation.Error);
                PatchEventDispatcher.SendPatchManifestUpdateFailedMsg();
                ChangeState<ProcedureUpdateManifest>(procedureOwner);
            }
        }
    }
}