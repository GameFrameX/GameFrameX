using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Hotfix.Events;
using Hotfix.Manager;
using Hotfix.Proto;

namespace Hotfix.UI
{
    /// <summary>
    /// UIRoomListPanel 的共享控制器，UI 框架无关。
    /// Shared controller for UIRoomListPanel, UI-framework agnostic.
    /// </summary>
    internal sealed class UIRoomListPanelController
    {
        private const int RoomListRefreshIntervalMilliseconds = 5000;

        private readonly IRoomListPanelBinder _binder;
        private readonly Func<bool> _isDisposedGetter;

        private long _selectedRoomId;
        private bool _isOpen;
        private bool _refreshLoopRunning;

        public UIRoomListPanelController(IRoomListPanelBinder binder, Func<bool> isDisposedGetter)
        {
            _binder = binder;
            _isDisposedGetter = isDisposedGetter;
        }

        public void OnOpen()
        {
            _isOpen = true;
            GameApp.Event.CheckSubscribe(RoomChangedEventArgs.EventId, OnRoomChanged);

            _binder.RegisterHandlers(
                () => RoomDataManager.Instance.RequestRoomList().Forget(),
                () => CreateRoomAndEnter().Forget(),
                CloseSelf,
                roomId => JoinRoomAndEnter(roomId).Forget());

            Refresh();
            StartRefreshLoop().Forget();
            RoomDataManager.Instance.RequestRoomList().Forget();
            EnterBattleIfAlreadyInRoom().Forget();
        }

        public void OnClose()
        {
            _isOpen = false;
            GameApp.Event.CheckUnsubscribe(RoomChangedEventArgs.EventId, OnRoomChanged);
            _binder.ClearHandlers();
        }

        private void OnRoomChanged(object sender, GameEventArgs e)
        {
            Refresh();
            EnterBattleIfAlreadyInRoom().Forget();
        }

        private async UniTask StartRefreshLoop()
        {
            if (_refreshLoopRunning)
            {
                return;
            }

            _refreshLoopRunning = true;
            try
            {
                while (_isOpen && !IsViewDisposed())
                {
                    await UniTask.Delay(RoomListRefreshIntervalMilliseconds);
                    if (!_isOpen || IsViewDisposed())
                    {
                        break;
                    }

                    await RoomDataManager.Instance.RequestRoomList();
                }
            }
            finally
            {
                _refreshLoopRunning = false;
            }
        }

        private void Refresh()
        {
            var manager = RoomDataManager.Instance;
            _binder.SetSummary($"当前玩家:{manager.CurrentRoleId}  房间数:{manager.Rooms.Count}");
            _binder.SetTips(manager.CurrentRoom == null
                ? "加入或创建房间后会进入对战界面。"
                : "你已在房间中，正在进入对战界面。");
            _binder.SetRoomList(manager.Rooms, _selectedRoomId);
        }

        private async UniTask CreateRoomAndEnter()
        {
            await RoomDataManager.Instance.CreateRoom(RoomUIFormatter.CreateRoomName());
            await EnterBattleIfAlreadyInRoom();
        }

        private async UniTask JoinRoomAndEnter(long roomId)
        {
            await RoomDataManager.Instance.JoinRoom(roomId);
            await EnterBattleIfAlreadyInRoom();
        }

        private async UniTask EnterBattleIfAlreadyInRoom()
        {
            if (RoomDataManager.Instance.CurrentRoom == null)
            {
                return;
            }

            GameApp.UI.CloseUIForm<UIRoomListPanel>();
            await GameApp.UI.OpenAsync<UIRoomBattlePanel>(UIGroupConstants.Window);
        }

        private void CloseSelf()
        {
            GameApp.UI.CloseUIForm<UIRoomListPanel>();
        }

        private bool IsViewDisposed()
        {
            return _isDisposedGetter == null || _isDisposedGetter();
        }
    }
}
