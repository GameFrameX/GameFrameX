/**
 * Phase 2 冒烟:以 Laya 网络通道连 Server.Source,完成 ReqLogin ↔ RespLogin RPC 往返。
 *
 * 前置:
 * - Server.Source 已运行(默认 TCP 29100 / WS 29110;本机 docker 映射 TCP 39100 / WS 39110)。
 * - build/tests 已编译(tsc -p tests.tsconfig.json)。
 *
 * 运行:node tools/smoke-login.js [wsUrl]
 */
const path = require('path');
const root = path.join(__dirname, '..');

// protobufjs UMD 挂全局(与浏览器 script 标签等价)
globalThis.protobuf = require(path.join(root, 'assets/protobuf/protobuf.js'));
// int64 精度:雪花 Id 超 2^53,必须启用 Long(否则回传 Id 精度丢失)
require(path.join(root, 'assets/protobuf/long.js'));
if (globalThis.Long == null) {
    globalThis.Long = require(path.join(root, 'assets/protobuf/long.js'));
}
globalThis.protobuf.util.Long = globalThis.Long;
globalThis.protobuf.configure();

const { loadProtoRoot } = require(path.join(root, 'build/tests/src/gameframex/network/ProtoRootLoader.js'));
const ProtoMessageRegister = require(path.join(root, 'build/tests/src/gameframex/protobuf/ProtoMessageRegister.js')).default;
const NetworkChannel = require(path.join(root, 'build/tests/src/gameframex/network/NetworkChannel.js')).default;
const WebSocketTransport = require(path.join(root, 'build/tests/src/gameframex/network/WebSocketTransport.js')).default;
const { User } = require(path.join(root, 'build/tests/src/gameframex/protobuf/_0300_User.js'));
const LogHelper = require(path.join(root, 'build/tests/src/gameframex/log/LogHelper.js')).default;

LogHelper.isDebug = false;

const wsUrl = process.argv.find((arg) => arg.startsWith('ws://') || arg.startsWith('wss://')) ?? 'ws://127.0.0.1:39110';

function fail(message) {
    console.error(`SMOKE FAIL: ${message}`);
    process.exit(1);
}

async function main() {
    // proto 路径:运行时清单为 'resources/protobuf/x.proto',node 下映射到 assets/resources/protobuf/
    const paths = ProtoMessageRegister.getProtoBuffList().map((p) => path.join(__dirname, '..', p.replace(/^resources\//, 'assets/resources/')));
    // node 宿主:protobuf.load 的 fetch 探测不可用,直接 fs + parse 灌入同一 Root
    const fs = require('fs');
    const protoRoot = new globalThis.protobuf.Root();
    for (const p of paths) {
        globalThis.protobuf.parse(fs.readFileSync(p, 'utf8'), protoRoot, { keepCase: true });
    }

    const ProtoMessageHelper = require(path.join(__dirname, '../build/tests/src/gameframex/network/ProtoMessageHelper.js')).default;
    ProtoMessageHelper.adopt(protoRoot);

    const channel = new NetworkChannel('smoke', new WebSocketTransport());
    const pump = setInterval(() => channel.update(0.016, 0.016), 16);

    const opened = new Promise((resolve, reject) => {
        const timer = setTimeout(() => reject(new Error('connect timeout')), 5000);
        channel.onConnected = () => { clearTimeout(timer); resolve(); };
        channel.onError = (_c, reason) => { clearTimeout(timer); reject(new Error(reason)); };
    });
    channel.Connect(wsUrl);
    await opened;
    console.log('connected:', wsUrl);

    if (process.argv.includes('--heartbeat')) {
        // 心跳观测:保持连接 > 30s,确认发出 op=1 的 ReqHeartBeat 且收到 NotifyHeartBeat
        const ProtoMessageHandler = require(path.join(__dirname, '../build/tests/src/gameframex/network/ProtoMessageHandler.js'));
        const { Basic } = require(path.join(__dirname, '../build/tests/src/gameframex/protobuf/_0010_Basic.js'));
        let notifySeen = false;
        class HeartBeatHost {
            onNotifyHeartBeat(msg) {
                notifySeen = true;
                console.log('NotifyHeartBeat received, Timestamp =', String(msg.Timestamp));
            }
        }
        ProtoMessageHandler.MessageHandler(Basic.NotifyHeartBeat)(HeartBeatHost.prototype, 'onNotifyHeartBeat');
        ProtoMessageHandler.default.add(new HeartBeatHost());


        await new Promise((resolve) => setTimeout(resolve, 32000));
        console.log('heartbeat notify seen =', notifySeen);
        channel.Close('smoke done');
        clearInterval(pump);
        if (!notifySeen) {
            fail('NotifyHeartBeat not received within 32s');
        }

        console.log('SMOKE PASS: heartbeat roundtrip OK');
        process.exit(0);
    }

    const req = new User.ReqLogin();
    req.UserName = 'laya_smoke';
    req.Password = 'laya_smoke';
    req.Platform = 'laya';

    const resp = await channel.Call(req);
    console.log('resp.PackageName =', resp.PackageName);
    console.log('resp.Code =', resp.Code);
    console.log('resp.Id =', String(resp.Id));
    console.log('resp.UniqueId(match request) =', resp.UniqueId === req.UniqueId);
    console.log('resp.MessageId =', resp.MessageId, `(expect ${(300 << 16) + 11})`);

    if (resp.PackageName !== 'User.RespLogin') {
        fail(`unexpected response ${resp.PackageName}`);
    }
    if (resp.UniqueId !== req.UniqueId) {
        fail('uniqueId mismatch: RPC pairing broken');
    }
    if (resp.MessageId !== (300 << 16) + 11) {
        fail(`messageId mismatch: ${resp.MessageId}`);
    }

    channel.Close('smoke done');
    clearInterval(pump);
    console.log('SMOKE PASS: ReqLogin ↔ RespLogin RPC roundtrip OK');
    process.exit(0);
}

main().catch((error) => fail(error && error.message ? error.message : String(error)));
