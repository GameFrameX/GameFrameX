using Cysharp.Threading.Tasks;
using GameFrameX.Fsm;
using GameFrameX.Procedure;

namespace GameFrameX.Procedure
{
    /// <summary>
    /// 启动热更新游戏
    /// </summary>
    public sealed class ProcedureGameLauncherState : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            Start();
        }

        private async void Start()
        {
            await UniTask.DelayFrame();

            // Game.EventSystem.Run(EventIdType.UILoadingMainSetText, "Loading...");
            LauncherUIHandler.Dispose();
            HotfixHelper.StartHotfix();
        }
    }
}