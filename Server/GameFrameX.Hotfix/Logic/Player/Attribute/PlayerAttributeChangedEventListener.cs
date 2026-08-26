using GameFrameX.Apps.Common.Event;
using GameFrameX.Apps.Common.EventData;
using GameFrameX.Apps.Common.Session;
using GameFrameX.Core.Abstractions.Events;

namespace GameFrameX.Hotfix.Logic.Player.Attribute;

/// <summary>
/// 监听玩家最终属性变化，向在线玩家 session 推送增量同步消息。
/// </summary>
[Event(EventId.AttributeChanged)]
internal sealed class PlayerAttributeChangedEventListener : EventListener<PlayerAttributeComponentAgent>
{
    protected override async Task HandleEvent(PlayerAttributeComponentAgent agent, GameEventArgs gameEventArgs)
    {
        if (agent == null || !(gameEventArgs is AttributeChangedEventArgs args))
        {
            return;
        }

        var session = SessionManager.GetByRoleId(args.PlayerId);
        if (session == null)
        {
            return;
        }

        await session.WriteAsync(PlayerAttributeSyncBuilder.BuildChanged(args.AttributeType, args.NewValue));
    }
}
