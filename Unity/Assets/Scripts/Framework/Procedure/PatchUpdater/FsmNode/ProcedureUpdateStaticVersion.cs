using System.Collections;
using Cysharp.Threading.Tasks;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;
using UnityGameFramework.Runtime;
using YooAsset;

namespace UnityGameFramework.Procedure
{
    internal sealed class ProcedureUpdateStaticVersion : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.UpdateStaticVersion);
            GetStaticVersion(procedureOwner).ToUniTask();
        }

        private IEnumerator GetStaticVersion(IFsm<IProcedureManager> procedureOwner)
        {
            yield return new WaitForSecondsRealtime(0.5f);

            var package = YooAssets.GetAssetsPackage(AssetComponent.BuildInPackageName);
            var operation = package.UpdatePackageVersionAsync();
            yield return operation;

            if (operation.Status == EOperationStatus.Succeed)
            {
                //更新成功
                string packageVersion = operation.PackageVersion;
                GameApp.Asset.UpdateStaticVersion(packageVersion);
                Debug.Log($"Updated package Version : {packageVersion}");
                ChangeState<ProcedureUpdateManifest>(procedureOwner);
            }
            else
            {
                //更新失败
                Debug.LogError(operation.Error);
                PatchEventDispatcher.SendStaticVersionUpdateFailedMsg();
                ChangeState<ProcedureUpdateStaticVersion>(procedureOwner);
            }
        }
    }
}