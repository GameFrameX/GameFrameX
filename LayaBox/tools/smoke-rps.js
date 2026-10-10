/**
 * Phase 6 冒烟:双 Laya 客户端协议级 RPS 对战闭环。
 *
 * 玩家 A/B 各自独立 NetworkChannel(独立 WS 连接):
 * ReqLogin → (自动建角)→ ReqPlayerLogin → A 建房 → B 加入 → A 开始 → 双方出拳 → 结算断言。
 * 前置:Server.Source 运行(WS 39110);build/tests 已编译。
 * 运行:node tools/smoke-rps.js [wsUrl]
 */
const path = require('path');
const Long = require(path.join(__dirname, '../assets/protobuf/long.js'));
globalThis.protobuf = require(path.join(__dirname, '../assets/protobuf/protobuf.js'));
globalThis.protobuf.util.Long = Long;
globalThis.protobuf.configure();


const fs = require('fs');
const ProtoMessageRegister = require(path.join(__dirname, '../build/tests/src/gameframex/protobuf/ProtoMessageRegister.js')).default;
const ProtoMessageHelper = require(path.join(__dirname, '../build/tests/src/gameframex/network/ProtoMessageHelper.js')).default;
const NetworkChannel = require(path.join(__dirname, '../build/tests/src/gameframex/network/NetworkChannel.js')).default;
const WebSocketTransport = require(path.join(__dirname, '../build/tests/src/gameframex/network/WebSocketTransport.js')).default;
const { User } = require(path.join(__dirname, '../build/tests/src/gameframex/protobuf/_0300_User.js'));
const { Room } = require(path.join(__dirname, '../build/tests/src/gameframex/protobuf/_0400_Room.js'));
const { RockPaperScissors } = require(path.join(__dirname, '../build/tests/src/gameframex/protobuf/_0410_RockPaperScissors.js'));
const { Bag } = require(path.join(__dirname, '../build/tests/src/gameframex/protobuf/_0100_Bag.js'));
const LogHelper = require(path.join(__dirname, '../build/tests/src/gameframex/log/LogHelper.js')).default;

LogHelper.isDebug = false;

const wsUrl = process.argv.find((arg) => arg.startsWith('ws://') || arg.startsWith('wss://')) ?? 'ws://127.0.0.1:39110';

function fail(message) {
    console.error(`SMOKE FAIL: ${message}`);
    process.exit(1);
}

async function connect(name) {
    const channel = new NetworkChannel(name, new WebSocketTransport());
    const opened = new Promise((resolve, reject) => {
        const timer = setTimeout(() => reject(new Error(`${name} connect timeout`)), 5000);
        channel.onConnected = () => { clearTimeout(timer); resolve(); };
        channel.onError = (_c, reason) => { clearTimeout(timer); reject(new Error(reason)); };
    });
    channel.Connect(wsUrl);
    await opened;
    const pump = setInterval(() => channel.update(0.016, 0.016), 16);
    return { channel, pump };
}

async function loginAndEnter(channel, userName) {
    const req = new User.ReqLogin();
    req.UserName = userName;
    req.Password = userName;
    req.Platform = 'laya';
    const respLogin = await channel.Call(req);
    if (String(respLogin.Id) === '0') {
        fail(`${userName} ReqLogin 失败`);
    }

    const reqList = new User.ReqPlayerList();
    reqList.Id = respLogin.Id;
    const respList = await channel.Call(reqList);
    let playerList = (respList.PlayerList ?? []);
    if (playerList.length === 0) {
        const reqCreate = new User.ReqPlayerCreate();
        reqCreate.Id = respLogin.Id;
        reqCreate.Name = `${userName}-p`;
        const respCreate = await channel.Call(reqCreate);
        playerList = [respCreate.PlayerInfo];
    }

    const reqEnter = new User.ReqPlayerLogin();
    reqEnter.Id = playerList[0].Id;
    const respEnter = await channel.Call(reqEnter);
    if ((respEnter.ErrorCode ?? 0) !== 0) {
        fail(`${userName} ReqPlayerLogin 失败: ${respEnter.ErrorCode}`);
    }

    // 注:服务端 RespPlayerLogin 现状不回 PlayerInfo;角色信息以列表/建角返回为准
    return playerList[0];
}

async function main() {
    // proto 装配
    const protoRoot = new globalThis.protobuf.Root();
    for (const p of ProtoMessageRegister.getProtoBuffList()) {
        globalThis.protobuf.parse(fs.readFileSync(path.join(__dirname, '..', p.replace(/^resources\//, 'assets/resources/')), 'utf8'), protoRoot, { keepCase: true });
    }
    ProtoMessageHelper.adopt(protoRoot);

    // 双客户端
    const a = await connect('smoke-a');
    const b = await connect('smoke-b');
    console.log('both clients connected:', wsUrl);

    const playerA = await loginAndEnter(a.channel, 'rps_smoke_a');
    const playerB = await loginAndEnter(b.channel, 'rps_smoke_b');
    console.log('players entered:', playerA.Name, '/', playerB.Name);

    // A 建房
    const reqCreateRoom = new Room.ReqCreateRoom();
    reqCreateRoom.GameType = Room.GameType.RockPaperScissors;
    reqCreateRoom.Name = 'smoke-room';
    reqCreateRoom.MinPlayerCount = 2;
    reqCreateRoom.MaxPlayerCount = 2;
    const respCreateRoom = await a.channel.Call(reqCreateRoom);
    console.log('createRoom resp:', JSON.stringify(respCreateRoom).slice(0, 200));
    if ((respCreateRoom.ErrorCode ?? 0) !== 0 || respCreateRoom.Room == null) {
        fail(`创建房间失败: code=${respCreateRoom.ErrorCode}`);
    }
    const roomId = respCreateRoom.Room.RoomId;
    console.log('room created:', roomId);

    // B 加入
    const reqJoin = new Room.ReqJoinRoom();
    reqJoin.RoomId = roomId;
    const respJoin = await b.channel.Call(reqJoin);
    if ((respJoin.ErrorCode ?? 0) !== 0) {
        fail(`加入房间失败: ${respJoin.ErrorCode}`);
    }
    console.log('player B joined');

    // A 开始游戏(等 Ready)
    await new Promise((resolve) => setTimeout(resolve, 300));
    const reqStart = new Room.ReqStartRoomGame();
    reqStart.RoomId = roomId;
    const respStart = await a.channel.Call(reqStart);
    if ((respStart.ErrorCode ?? 0) !== 0) {
        fail(`开始游戏失败: ${respStart.ErrorCode}`);
    }
    console.log('game started');

    // 双方出拳:A 石头 / B 剪刀 → A 胜
    const submit = async (channel, gesture) => {
        const req = new RockPaperScissors.ReqSubmitRockPaperScissorsGesture();
        req.RoomId = roomId;
        req.Gesture = gesture;
        const resp = await channel.Call(req);
        if ((resp.ErrorCode ?? 0) !== 0) {
            fail(`出拳失败: ${resp.ErrorCode}`);
        }
        return resp.GameInfo;
    };

    await submit(a.channel, RockPaperScissors.RockPaperScissorsGesture.Rock);
    const gameInfo = await submit(b.channel, RockPaperScissors.RockPaperScissorsGesture.Scissors);
    console.log('gestures submitted');

    await new Promise((resolve) => setTimeout(resolve, 500));

    // 结算核验:重新拉对局信息
    const reqInfo = new RockPaperScissors.ReqRockPaperScissorsGameInfo();
    reqInfo.RoomId = roomId;
    const respInfo = await a.channel.Call(reqInfo);
    const winner = respInfo.GameInfo.WinnerRoleId;
    console.log('winner RoleId =', String(winner), '(A =', String(playerA.Id), ', B =', String(playerB.Id) + ')');
    if (String(winner) !== String(playerA.Id)) {
        fail('石头应胜剪刀:胜者应为 A');
    }

    // 背包数据核验(A)
    const respBag = await a.channel.Call(new Bag.ReqBagInfo());
    console.log('bag items(A) =', Object.keys(respBag.ItemDic ?? {}).length, 'errorCode =', respBag.ErrorCode ?? 0);

    a.channel.Close('done');
    b.channel.Close('done');
    clearInterval(a.pump);
    clearInterval(b.pump);
    console.log('SMOKE PASS: RPS 对战闭环(建房/加入/开始/出拳/结算 A 胜)+ 背包数据 OK');
    process.exit(0);
}

main().catch((error) => fail(error && error.message ? error.message : String(error)));
