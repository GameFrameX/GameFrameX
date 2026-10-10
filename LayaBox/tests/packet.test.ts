import { encodePacketHeader, decodePacketHeader, PACKET_HEADER_LENGTH } from "../src/gameframex/network/PacketHeader";
import { test, assert, assertEqual } from "./harness";

test('PacketHeader: 长度为 14 字节(uint32+byte+byte+int32+int32)', () => {
    assertEqual(PACKET_HEADER_LENGTH, 14, '包头长度');
});

test('PacketHeader: 编码为大端(与 Unity BufferExtension 一致)', () => {
    const buffer = encodePacketHeader({ packetLength: 0x01020304, operationType: 4, zipFlag: 0, uniqueId: 0x11223344, messageId: 0x55667788 });
    const bytes = new Uint8Array(buffer);
    assertEqual(bytes[0], 0x01, 'packetLength 高字节在前(大端)');
    assertEqual(bytes[1], 0x02, 'packetLength 次高字节');
    assertEqual(bytes[4], 4, 'operationType 偏移 4');
    assertEqual(bytes[5], 0, 'zipFlag 偏移 5');
    assertEqual(bytes[6], 0x11, 'uniqueId 高字节(大端)');
    assertEqual(bytes[10], 0x55, 'messageId 高字节(大端)');
});

test('PacketHeader: 解码与编码往返一致;不足 14 字节返回 null', () => {
    const header = { packetLength: 14 + 8, operationType: 1, zipFlag: 0, uniqueId: -12345678, messageId: 655370 };
    const decoded = decodePacketHeader(encodePacketHeader(header))!;
    assert(decoded != null, '可解码');
    assertEqual(decoded.packetLength, header.packetLength, 'packetLength 往返');
    assertEqual(decoded.operationType, header.operationType, 'operationType 往返');
    assertEqual(decoded.zipFlag, header.zipFlag, 'zipFlag 往返');
    assertEqual(decoded.uniqueId, header.uniqueId, 'uniqueId 往返(负数补码)');
    assertEqual(decoded.messageId, header.messageId, 'messageId 往返');
    assertEqual(decodePacketHeader(new ArrayBuffer(13)), null, '半包返回 null');
});

test('PacketHeader: 带偏移解码(粘包场景)', () => {
    const header = { packetLength: 20, operationType: 4, zipFlag: 0, uniqueId: 7, messageId: 9 };
    const frame = new Uint8Array(4 + 14);
    frame.set(new Uint8Array(4), 0);
    frame.set(new Uint8Array(encodePacketHeader(header)), 4);
    const decoded = decodePacketHeader(frame.buffer, 4)!;
    assertEqual(decoded.uniqueId, 7, '偏移后解码');
    assertEqual(decoded.messageId, 9, '偏移后 messageid');
});
