using Cysharp.Threading.Tasks;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GlobalConfig;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Procedure
{
    internal sealed class ProcedurePatchInit : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            // Game.EventSystem.Run(EventIdType.UILoadingMainSetText, "Loading...");
            // 加载更新面板
            Start(procedureOwner);
        }

        async void Start(IFsm<IProcedureManager> procedureOwner)
        {
            var assetComponent = GameEntry.GetComponent<AssetComponent>();

            await assetComponent.Initialize(GetHostServerURL());
            // 运行补丁流程
            PatchUpdater.Run();
            await UniTask.DelayFrame(10);

            ChangeState<ProcedureUpdateStaticVersion>(procedureOwner);
        }

        private string GetHostServerURL()
        {
            return $"{GameEntry.GetComponent<GlobalConfigComponent>().HostServerUrl}";
        }
    }
}