// GameFrameX 组织下的以及组织衍生的项目的版权、商标和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发。许可证位于源代码树根目录的 LICENSE 文件。

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameFrameX.Event.Runtime;
using GameFrameX.Network.Runtime;
using GameFrameX.Runtime;
using System.Collections;
using GameFrameX.Web.ProtoBuff.Runtime;
using Hotfix.Network;
using Hotfix.Manager;
using Hotfix.Proto;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hotfix.Tests.PlayMode
{
    /// <summary>
    /// C14 四子棋双客户端联调冒烟（自动化版）——玩家 A 侧（Unity）。
    /// 真实依赖：本机 28080 HTTP 与 29100 TCP 的 GameFrameX 服务器，以及一个以
    /// --scenario=connectfour 启动的 Bot 玩家 B（负责加入房间并轮流落子）。
    /// 走最小初始化路径：不经过场景/UI boot，直接创建 Event/Network/WebProtoBuff 三个框架组件。
    /// </summary>
    [TestFixture]
    public class ConnectFourSmokePlayModeTests
    {
        private const string HttpApiBase = "http://127.0.0.1:28080/game/api/";
        private const string TcpAddress = "tcp://127.0.0.1:29100";
        /// <summary>单调时钟毫秒（Unity netstandard 无 Environment.TickCount64）。</summary>
        private static long NowMs() => _clock.ElapsedMilliseconds;

        private static readonly System.Diagnostics.Stopwatch _clock = System.Diagnostics.Stopwatch.StartNew();
        private const string BotPassword = "12312";
        private const int ColumnCount = 7;
        private const int RowCount = 6;

        /// <summary>中心优先的确定性落子列偏好（与玩家 B 的 Bot 策略一致，先手据此必胜）。</summary>
        private static readonly int[] ColumnPreference = { 3, 2, 4, 1, 5, 0, 6 };

        private GameObject _frameworkRoot;
        private long _myPlayerId;

        [SetUp]
        public void SetUp()
        {
            // PlayMode 下 GameFrameXRuntimeHost 会自动注入全部框架组件（RuntimeInitializeOnLoadMethod），
            // 手动 AddComponent 会触发 "component type already exist"；此处只等待宿主组件就绪。
            var deadline = NowMs() + 10_000;
            while (NowMs() < deadline
                   && (GameEntry.GetComponent<EventComponent>() == null
                       || GameEntry.GetComponent<WebProtoBuffComponent>() == null
                       || GameEntry.GetComponent<NetworkComponent>() == null))
            {
                System.Threading.Thread.Sleep(50);
            }

            Assert.NotNull(GameEntry.GetComponent<EventComponent>(), "宿主应已自动创建 EventComponent");
            Assert.NotNull(GameEntry.GetComponent<WebProtoBuffComponent>(), "宿主应已自动创建 WebProtoBuffComponent");
            Assert.NotNull(GameEntry.GetComponent<NetworkComponent>(), "宿主应已自动创建 NetworkComponent");
            ProtoMessageIdHandler.Init(HotfixProtoHandler.CurrentAssembly);

            // 实例化数据管理器（构造时注册协议处理器与房间事件订阅）。
            _ = RoomDataManager.Instance;
            _ = ConnectFourDataManager.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            if (GameApp.Network.HasNetworkChannel("network"))
            {
                GameApp.Network.DestroyNetworkChannel("network");
            }

            // 组件由运行时宿主持有，测试不销毁；只清理本测试创建的网络通道。
        }

        [UnityTest]
        public IEnumerator ConnectFour_UnityVsBot_FullChain_TwoRounds_Reconnect()
        {
            yield return RunSmoke().ToCoroutine();
        }

        private async UniTask RunSmoke()
        {
            var userName = "UnitySmokeA_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // ===== 1. HTTP 登录/建号 =====
            var respLogin = await GameApp.WebProtoBuff.Post<RespLogin>(HttpApiBase + "req_login", new ReqLogin
            {
                UserName = userName,
                Password = BotPassword,
                Platform = "LoginPlatform.Custom",
                SdkType = 0,
                SdkToken = "",
                Device = SystemInfo.deviceUniqueIdentifier,
            });
            AssertZero(respLogin.ErrorCode, "req_login");
            Smoke($"账号登录成功 accountId={respLogin.Id}");

            var respPlayerList = await GameApp.WebProtoBuff.Post<RespPlayerList>(HttpApiBase + "req_player_list", new ReqPlayerList { Id = respLogin.Id });
            AssertZero(respPlayerList.ErrorCode, "req_player_list");

            PlayerInfo player;
            if (respPlayerList.PlayerList.Count > 0)
            {
                player = respPlayerList.PlayerList[0];
                Smoke($"复用已有角色 {player.Id} {player.Name}");
            }
            else
            {
                var respCreate = await GameApp.WebProtoBuff.Post<RespPlayerCreate>(HttpApiBase + "req_player_create", new ReqPlayerCreate
                {
                    Id = respLogin.Id,
                    Name = userName,
                });
                AssertZero(respCreate.ErrorCode, "req_player_create");
                player = respCreate.PlayerInfo;
                Smoke($"创建角色成功 playerId={player.Id}");
            }

            // ===== 2. TCP 连接与角色登录 =====
            var channel = await ConnectNetworkChannel();
            var respPlayerLogin = await channel.Call<RespPlayerLogin>(new ReqPlayerLogin { Id = player.Id });
            AssertZero(respPlayerLogin.ErrorCode, "req_player_login(TCP)");
            PlayerManager.Instance.PlayerInfo = respPlayerLogin.PlayerInfo;
            _myPlayerId = respPlayerLogin.PlayerInfo.Id;
            Smoke($"角色登录成功 playerId={_myPlayerId}");

            // ===== 3. 建房并等待玩家 B（Bot）加入 =====
            await RoomDataManager.Instance.CreateRoom("C4SmokeByUnity", GameType.ConnectFour);
            var room = RoomDataManager.Instance.CurrentRoom;
            Assert.NotNull(room, "建房后 CurrentRoom 不应为空");
            Assert.AreEqual(GameType.ConnectFour, room.GameType);
            Assert.AreEqual(_myPlayerId, room.OwnerPlayerId, "房主应为玩家 A");
            long roomId = room.RoomId;
            Smoke($"创建四子棋房间成功 roomId={roomId} round={room.Round}");

            await WaitUntil(() => RoomDataManager.Instance.CurrentRoom != null
                                   && RoomDataManager.Instance.CurrentRoom.PlayerCount >= 2, 180_000, "等待玩家B加入");
            room = RoomDataManager.Instance.CurrentRoom;
            Assert.AreEqual(RoomStatus.Ready, room.Status, "两人后房间应进入 Ready");
            Assert.AreEqual(1, room.Players.Count(p => p.PlayerId != _myPlayerId), "应恰有一名对手");
            var opponentId = room.Players.First(p => p.PlayerId != _myPlayerId).PlayerId;
            Smoke($"玩家B已加入 roomId={roomId} opponentId={opponentId}");

            // ===== 4. 开始游戏 =====
            Assert.IsTrue(RoomDataManager.Instance.CanStartGame(), "房主应可开始游戏");
            await RoomDataManager.Instance.StartGame();
            await WaitUntil(() => RoomDataManager.Instance.CurrentRoom != null
                                   && RoomDataManager.Instance.CurrentRoom.Status == RoomStatus.Playing, 15_000, "等待房间 Playing");
            await WaitUntil(() => ConnectFourDataManager.Instance.CurrentGame != null
                                   && ConnectFourDataManager.Instance.CurrentGame.RoomId == roomId, 15_000, "等待首局棋盘数据");
            var game = ConnectFourDataManager.Instance.CurrentGame;
            Assert.AreEqual(1, game.Round, "首局 Round 应为 1");
            Assert.AreEqual(_myPlayerId, game.CurrentPlayerId, "先手应为 PlayerIds[0]（房主=玩家A）");
            Smoke($"对局开始 roomId={roomId} 先手={game.CurrentPlayerId}");

            // ===== 5. 第一局：交替落子直到结算 =====
            var (winner1, round1) = await PlayUntilSettled(60_000);
            Assert.AreEqual(1, round1, "第一局 Round");
            Assert.AreEqual(_myPlayerId, winner1, "确定性策略下先手（玩家A）应获胜");
            AssertHasConnectFour(ConnectFourDataManager.Instance.CurrentGame, winner1);
            Smoke($"第一局结算：胜者={winner1}（玩家A）");

            // ===== 6. 房主再来一局 =====
            await WaitUntil(ConnectFourDataManager.Instance.CanRestartGame, 15_000, "等待可重开");
            await ConnectFourDataManager.Instance.RestartGame();
            await WaitUntil(() =>
            {
                var g = ConnectFourDataManager.Instance.CurrentGame;
                return g != null && g.Round == 2 && g.MoveCount == 0 && g.CurrentPlayerId == _myPlayerId;
            }, 15_000, "等待第二局开局快照");
            Smoke("第二局已开局（Round=2，棋盘清空，玩家A先手）");

            // ===== 7. 第二局中途断线重进：快照对账 =====
            await PlayOneOwnMove("第二局首手");
            await WaitUntil(() =>
            {
                var g = ConnectFourDataManager.Instance.CurrentGame;
                return g != null && g.MoveCount >= 2 && (g.CurrentPlayerId == _myPlayerId || IsGameOver(g));
            }, 30_000, "等待玩家B应手且回合交还玩家A");
            var before = CloneGame(ConnectFourDataManager.Instance.CurrentGame);
            Smoke($"断线前快照：moveCount={before.MoveCount} current={before.CurrentPlayerId} round={before.Round}");

            GameApp.Network.DestroyNetworkChannel("network");
            Smoke("已断开 TCP 连接，模拟掉线");
            await UniTask.Delay(2000);

            channel = await ConnectNetworkChannel();
            var respRelogin = await channel.Call<RespPlayerLogin>(new ReqPlayerLogin { Id = _myPlayerId });
            AssertZero(respRelogin.ErrorCode, "req_player_login(重连)");
            await ConnectFourDataManager.Instance.RequestGameInfo(roomId);
            var after = ConnectFourDataManager.Instance.CurrentGame;
            Assert.NotNull(after, "重进后必须能拉到棋盘快照");
            Assert.AreEqual(before.Round, after.Round, "重进后 Round 应一致");
            Assert.AreEqual(before.MoveCount, after.MoveCount, "重进后落子数应一致（无人再落子）");
            Assert.AreEqual(_myPlayerId, after.CurrentPlayerId, "重进后回合应仍是玩家A");
            Assert.AreEqual(before.WinnerPlayerId, after.WinnerPlayerId, "重进后胜负状态应一致");
            CollectionAssert.AreEqual(before.Board, after.Board, "重进后棋盘应逐格一致");
            Smoke($"断线重进快照对账通过：board 42 格一致，moveCount={after.MoveCount}，回合=玩家A");

            // ===== 8. 第二局继续到二次结算 =====
            var (winner2, round2) = await PlayUntilSettled(60_000);
            Assert.AreEqual(2, round2, "第二局 Round");
            Assert.AreEqual(_myPlayerId, winner2, "确定性策略下先手（玩家A）应再次获胜");
            AssertHasConnectFour(ConnectFourDataManager.Instance.CurrentGame, winner2);
            Smoke($"第二局结算：胜者={winner2}（玩家A）——全链完成");
        }

        /// <summary>建立 TCP 网络通道（含心跳），等待连接就绪后返回。</summary>
        private async UniTask<INetworkChannel> ConnectNetworkChannel()
        {
            if (GameApp.Network.HasNetworkChannel("network"))
            {
                GameApp.Network.DestroyNetworkChannel("network");
            }

            var channel = GameApp.Network.CreateNetworkChannel("network", new DefaultNetworkChannelHelper());
            channel.RegisterHeartBeatHandler(new DefaultPacketHeartBeatHandler());
            channel.Connect(new Uri(TcpAddress));
            await WaitUntil(() => GameApp.Network.GetNetworkChannel("network").Connected, 15_000, "TCP 连接");
            Smoke("TCP 网络通道已连接 " + TcpAddress);
            return channel;
        }

        /// <summary>轮到自己时按确定性策略落子，直到本局结算；返回 (胜者, 局数)。</summary>
        private async UniTask<(long winner, int round)> PlayUntilSettled(int timeoutMs)
        {
            var deadline = NowMs() + timeoutMs;
            var moveGuard = -1;
            while (NowMs() < deadline)
            {
                var game = ConnectFourDataManager.Instance.CurrentGame;
                if (game != null && IsGameOver(game))
                {
                    return (game.WinnerPlayerId, game.Round);
                }

                if (ConnectFourDataManager.Instance.CanSubmitMove() && game.MoveCount != moveGuard)
                {
                    moveGuard = game.MoveCount;
                    var column = PickColumn(game);
                    Assert.GreaterOrEqual(column, 0, "轮到玩家A时必须存在可落子的列");
                    Smoke($"玩家A落子 col={column} (moveCount={game.MoveCount}, round={game.Round})");
                    await ConnectFourDataManager.Instance.SubmitMove(column);
                }

                await UniTask.Delay(100);
            }

            Assert.Fail($"对局在 {timeoutMs}ms 内未结算");
            return (0, 0);
        }

        /// <summary>执行一步己方落子（断线演练用），并确认回合交还对方。</summary>
        private async UniTask PlayOneOwnMove(string tag)
        {
            await WaitUntil(ConnectFourDataManager.Instance.CanSubmitMove, 15_000, tag + "等待己方回合");
            var game = ConnectFourDataManager.Instance.CurrentGame;
            var column = PickColumn(game);
            Smoke($"[{tag}] 玩家A落子 col={column}");
            await ConnectFourDataManager.Instance.SubmitMove(column);
        }

        /// <summary>确定性策略：按 3,2,4,1,5,0,6 取第一个未满列。</summary>
        private static int PickColumn(ConnectFourGameInfo game)
        {
            foreach (var column in ColumnPreference)
            {
                for (var row = RowCount - 1; row >= 0; row--)
                {
                    if (game.Board[row * ColumnCount + column] == 0)
                    {
                        return column;
                    }
                }
            }

            return -1;
        }

        private static bool IsGameOver(ConnectFourGameInfo game)
        {
            return game.WinnerPlayerId != 0 || game.IsDraw;
        }

        /// <summary>本地复算：胜者最后落子位置必须存在横/竖/斜四连。</summary>
        private static void AssertHasConnectFour(ConnectFourGameInfo game, long winnerId)
        {
            Assert.NotNull(game, "结算时必须有对局数据");
            var winner = game.Players.FirstOrDefault(p => p.PlayerId == winnerId);
            Assert.NotNull(winner, "胜者必须在玩家列表中");
            var seat = winner.SeatIndex;
            Assert.IsTrue(HasConnectFourAt(game.Board, game.LastMoveRow, game.LastMoveColumn, seat),
                $"胜者 seat={seat} 最后落子 ({game.LastMoveRow},{game.LastMoveColumn}) 处应存在四连");
        }

        private static bool HasConnectFourAt(List<int> board, int row, int column, int seat)
        {
            int[][] directions = { new[] { 0, 1 }, new[] { 1, 0 }, new[] { 1, 1 }, new[] { 1, -1 } };
            foreach (var dir in directions)
            {
                var count = 1;
                for (var sign = -1; sign <= 1; sign += 2)
                {
                    var r = row + dir[0] * sign;
                    var c = column + dir[1] * sign;
                    while (r >= 0 && r < RowCount && c >= 0 && c < ColumnCount && board[r * ColumnCount + c] == seat)
                    {
                        count++;
                        r += dir[0] * sign;
                        c += dir[1] * sign;
                    }
                }

                if (count >= 4)
                {
                    return true;
                }
            }

            return false;
        }

        private static ConnectFourGameInfo CloneGame(ConnectFourGameInfo source)
        {
            return new ConnectFourGameInfo
            {
                RoomId = source.RoomId,
                Round = source.Round,
                CurrentPlayerId = source.CurrentPlayerId,
                WinnerPlayerId = source.WinnerPlayerId,
                IsDraw = source.IsDraw,
                MoveCount = source.MoveCount,
                LastMoveColumn = source.LastMoveColumn,
                LastMoveRow = source.LastMoveRow,
                Board = new List<int>(source.Board),
                Players = source.Players.Select(p => new ConnectFourPlayerInfo { PlayerId = p.PlayerId, SeatIndex = p.SeatIndex }).ToList(),
            };
        }

        private static async UniTask WaitUntil(Func<bool> predicate, int timeoutMs, string what)
        {
            var deadline = NowMs() + timeoutMs;
            while (NowMs() < deadline)
            {
                if (predicate())
                {
                    return;
                }

                await UniTask.Delay(100);
            }

            Assert.Fail($"等待超时（{timeoutMs}ms）：{what}");
        }

        private static void AssertZero(int errorCode, string what)
        {
            Assert.AreEqual(0, errorCode, $"{what} 不应返回错误码");
        }

        private static void Smoke(string message)
        {
            var line = $"[C4SMOKE] {message}";
            TestContext.Out.WriteLine(line);
            Debug.Log(line);
        }
    }
}
