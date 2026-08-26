using GameFrameX.Runtime;
using Hotfix.Proto;

namespace Hotfix.Manager
{
    public sealed class PlayerManager : GameFrameworkSingleton<PlayerManager>
    {
        public PlayerManager()
        {
            PlayerInfo = new PlayerInfo();
        }

        public PlayerInfo PlayerInfo { get; set; }
    }
}