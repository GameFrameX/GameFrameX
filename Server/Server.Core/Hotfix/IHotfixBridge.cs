
using Server.Core.Utility;
using Server.Utility;

namespace Server.Core.Hotfix
{
    public interface IHotfixBridge
    {
        ServerType BridgeType { get; }

        Task<bool> OnLoadSuccess(bool reload);

        Task Stop();
    }
}
