import NetworkChannel from "../src/gameframex/network/NetworkChannel";
import NetworkManager from "../src/gameframex/network/NetworkManager";
import MessageObject from "../src/gameframex/network/MessageObject";
import ProtoMessageHelper from "../src/gameframex/network/ProtoMessageHelper";
import ProtoMessageHandler, { MessageHandler } from "../src/gameframex/network/ProtoMessageHandler";
import { encodePacketHeader, decodePacketHeader, OPERATION_TYPE_HEARTBEAT, PACKET_HEADER_LENGTH } from "../src/gameframex/network/PacketHeader";
import type IMessage from "../src/gameframex/network/IMessage";
import FakeTransport from "./FakeTransport";
import { test, assert, assertEqual } from "./harness";

const encoder = new TextEncoder();
const decoder = new TextDecoder();

/** JSON 往返的假 protobuf 类型:结构与真类型一致(create/encode/decode)。 */
function fakeType(): IMessage {
    return {
        create: (payload: object) => payload,
        verify: () => '',
        encode: (payload: object) => ({ finish: () => encoder.encode(JSON.stringify(payload)) }),
        decode: (buffer: Uint8Array) => JSON.parse(decoder.decode(buffer)) as unknown,
    } as IMessage;
}

/** 宿主注入的假 Root:任意全名都给 JSON 往返类型。 */
const fakeRoot = { lookupType: (_name: string) => fakeType() };

class ReqEcho extends MessageObject {
    public readonly PackageName: string = 'Test.ReqEcho';
    public Payload: string = '';
}

class RespEcho extends MessageObject {
    public readonly PackageName: string = 'Test.RespEcho';
    public Result: string = '';
}

class NotifyEcho extends MessageObject {
    public readonly PackageName: string = 'Test.NotifyEcho';
    public Info: string = '';
}

// ===== 装配:假 Root 灌注生成清单(68 条),随后注册测试专用消息 =====
ProtoMessageHelper.reset();
ProtoMessageHelper.adopt(fakeRoot);
const baselineCount = ProtoMessageHelper.registeredCount;

test('ProtoMessageHelper: 生成清单注册 68 条消息且消息号符合 module<<16|id', () => {
    assertEqual(baselineCount, 68, '注册总数(与 ProtoExport 生成物一致)');
    assertEqual(ProtoMessageHelper.getReqMessageId('Basic.ReqHeartBeat'), (10 << 16) + 10, 'ReqHeartBeat = 10<<16|10');
    assertEqual(ProtoMessageHelper.getReqMessageId('User.ReqLogin'), (300 << 16) + 10, 'ReqLogin = 300<<16|10');
    assertEqual(ProtoMessageHelper.getRespMessageName((300 << 16) + 11), 'User.RespLogin', 'RespLogin 反查');
    assert(ProtoMessageHelper.isHeartBeat('Basic.ReqHeartBeat'), '心跳识别');
    assert(!ProtoMessageHelper.isHeartBeat('User.ReqLogin'), '非心跳');
});

ProtoMessageHelper.registerReqMessage('Test.ReqEcho', (300 << 16) + 100);
ProtoMessageHelper.registerRespMessage('Test.RespEcho', (300 << 16) + 101);
ProtoMessageHelper.registerRespMessage('Test.NotifyEcho', (300 << 16) + 102);
/** 构造一条服务端响应帧(header + JSON body)。 */
function serverFrame(uniqueId: number, messageId: number, body: object): ArrayBuffer {
    const payload = encoder.encode(JSON.stringify(body));
    const header = encodePacketHeader({
        packetLength: PACKET_HEADER_LENGTH + payload.byteLength,
        operationType: 4,
        zipFlag: 0,
        uniqueId,
        messageId,
    });
    const frame = new Uint8Array(PACKET_HEADER_LENGTH + payload.byteLength);
    frame.set(new Uint8Array(header), 0);
    frame.set(payload, PACKET_HEADER_LENGTH);
    return frame.buffer;
}

function setup(): { channel: NetworkChannel; transport: FakeTransport } {
    const transport = new FakeTransport();
    const channel = new NetworkChannel('test', transport, { autoReconnect: false });
    channel.Connect('ws://test');
    return { channel, transport };
}

test('NetworkChannel: Connect 建连后 onConnected,Send 产出 14 字节大端帧', () => {
    const { channel, transport } = setup();
    let connected = 0;
    channel.onConnected = () => connected++;

    const req = new ReqEcho();
    req.Payload = 'hello';
    channel.Send(req);
    assertEqual(transport.sentFrames.length, 0, '未连接不写帧');

    channel.update(0.016, 0.016);
    assertEqual(connected, 0, 'Connect 同步回调尚未发生(假传输 connect 立即 open)');

    assert(transport.connected, '传输已连接');
    assertEqual(connected, 0, 'onConnected 由 transport.onOpen 触发后计数');

    channel.update(0.016, 0.016);
    assertEqual(transport.sentFrames.length, 1, 'update 后写出一帧');
    const header = decodePacketHeader(transport.sentFrames[0])!;
    assertEqual(header.operationType, 4, '普通消息 op=4');
    assertEqual(header.zipFlag, 0, '未压缩');
    assertEqual(header.uniqueId, req.UniqueId, '帧内 UniqueId 为请求编号');
    assertEqual(header.messageId, ProtoMessageHelper.getReqMessageId('Test.ReqEcho'), '帧内 messageId 来自请求表');
    assertEqual(header.packetLength, PACKET_HEADER_LENGTH + new TextDecoder().decode(new Uint8Array(transport.sentFrames[0]).subarray(14)).length, 'packetLength=14+body');
});

test('NetworkChannel: Call 配对响应,字段解码回填', async () => {
    const { channel, transport } = setup();
    const req = new ReqEcho();
    req.Payload = 'ping';

    const promise = channel.Call<RespEcho>(req);
    channel.update(0.016, 0.016);
    assertEqual(transport.sentFrames.length, 1, '请求已写出');

    transport.serverRespond(serverFrame(req.UniqueId, ProtoMessageHelper.getRespMessageId('Test.RespEcho'), { Result: 'pong' }));
    channel.update(0.016, 0.016);

    const resp = await promise;
    assertEqual(resp.Result, 'pong', '响应字段解码');
    assertEqual(resp.UniqueId, req.UniqueId, 'UniqueId 回传');
    assertEqual(resp.MessageId, ProtoMessageHelper.getRespMessageId('Test.RespEcho'), 'MessageId 回填');
    assertEqual(resp.PackageName, 'Test.RespEcho', 'PackageName 回填');
});

test('NetworkChannel: 非 RPC 响应走 @MessageHandler 分发(实例方法经 add 绑定)', () => {
    ProtoMessageHandler.reset();
    const received: string[] = [];

    class HandlerHost {
        @MessageHandler(NotifyEcho)
        private onNotifyEcho(msg: NotifyEcho): void {
            received.push(msg.Info);
        }
    }

    const host = new HandlerHost();
    ProtoMessageHandler.add(host);

    const { channel, transport } = setup();
    const notifyId = ProtoMessageHelper.getRespMessageId('Test.NotifyEcho');
    transport.serverRespond(serverFrame(424242, notifyId, { Info: 'notify-body' }));
    channel.update(0.016, 0.016);
    assertEqual(received.join(','), 'notify-body', 'Notify 分发到实例方法');
});


test('NetworkChannel: 心跳 30s 触发(op=1),丢失超阈值关闭;收到任一包归零', () => {
    const { channel, transport } = setup();
    const closed: string[] = [];
    const missed: number[] = [];
    channel.onClosed = (_c, reason) => closed.push(reason);
    channel.onMissHeartBeat = (_c, count) => missed.push(count);

    for (let i = 0; i < 31; i++) {
        channel.update(1, 1);
    }

    const frames = transport.sentFrames;
    const lastHeader = decodePacketHeader(frames[frames.length - 1])!;
    assertEqual(lastHeader.operationType, OPERATION_TYPE_HEARTBEAT, '心跳帧 op=1');
    assertEqual(lastHeader.messageId, ProtoMessageHelper.getReqMessageId('Basic.ReqHeartBeat'), '心跳消息号 10<<16|10');

    // 30s+31s:missCount 已累计;继续空转直到 missCount > 5 关闭
    for (let i = 0; i < 200 && closed.length === 0; i++) {
        channel.update(1, 1);
    }

    assert(closed.length > 0, '超阈值后关闭通道');
    assert(missed.length > 0, '丢失事件已触发');
    assert(!channel.IsConnected, '关闭后不可通信');
});

test('NetworkChannel: 收包重置心跳丢失计数(不误断)', () => {
    const { channel, transport } = setup();
    const closed: string[] = [];
    channel.onClosed = (_c, reason) => closed.push(reason);

    for (let round = 0; round < 8; round++) {
        for (let i = 0; i < 29; i++) {
            channel.update(1, 1);
        }
        // 每轮 29s 未到心跳阈值,再收一个包归零计数
        transport.serverRespond(serverFrame(1, ProtoMessageHelper.getRespMessageId('Test.NotifyEcho'), { Info: 'keepalive' }));
        channel.update(1, 1);
    }

    assertEqual(closed.length, 0, '持续收包不应关闭');
});

test('NetworkChannel: Close 清理等待中的 RPC(异常收场)', async () => {
    const { channel } = setup();
    const req = new ReqEcho();
    const promise = channel.Call<RespEcho>(req);
    channel.update(0.016, 0.016);
    channel.Close('unit test');

    await promise.then(
        () => { throw new Error('should not resolve'); },
        (error: Error) => assert(error.message.includes('network closed'), '关闭错误信息: ' + error.message),
    );
});

test('NetworkManager: 通道创建/获取/销毁与关闭触发', () => {
    const manager = new NetworkManager();
    const transport = new FakeTransport();
    const channel = new NetworkChannel('main', transport);
    manager.CreateNetworkChannel(channel);

    assert(manager.HasNetworkChannel('main'), '通道存在');
    assertEqual(manager.GetNetworkChannel('main'), channel, '按名获取');
    assertEqual(manager.NetworkChannelCount, 1, '数量');
    let closed = 0;
    manager.GetNetworkChannel('main').onClosed = () => closed++;
    manager.DestroyNetworkChannel('main');
    assertEqual(closed, 1, '销毁触发关闭');
    assert(!manager.HasNetworkChannel('main'), '销毁后不存在');
});
