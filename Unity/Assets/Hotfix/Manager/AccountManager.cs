using System.Collections.Generic;
using GameFrameX.Runtime;
using Hotfix.Proto;

namespace Hotfix.Manager
{
    public sealed class AccountManager : GameFrameworkSingleton<AccountManager>
    {
        public List<PlayerInfo> PlayerList { get; set; }
    }
}