/**
 * 14 字节网络包头编解码。
 *
 * 与 Unity `DefaultPacketSendHeaderHandler` / `DefaultPacketReceiveHeaderHandler`
 * 及服务端 SuperSocket 组包字节级一致:
 *
 * ```
 * uint32 PacketLength   // 整包长度,含包头自身(14 + body)
 * byte   OperationType  // 1=心跳,4=普通消息
 * byte   ZipFlag        // 0=未压缩,1=已压缩
 * int32  UniqueId       // 请求编号,响应原样回传用于 RPC 配对
 * int32  MessageId      // (module << 16) | id
 * ```
 *
 * 全字段大端(Unity BufferExtension 使用 BinaryPrimitives.WriteUInt32BigEndian / HostToNetworkOrder)。
 */
export const PACKET_HEADER_LENGTH = 14;

/** 普通消息操作类型。 */
export const OPERATION_TYPE_MESSAGE = 4;

/** 心跳消息操作类型。 */
export const OPERATION_TYPE_HEARTBEAT = 1;

export interface PacketHeader {
    /** 整包长度(含包头)。 */
    packetLength: number;
    /** 操作类型:1=心跳,4=普通。 */
    operationType: number;
    /** 压缩标记。 */
    zipFlag: number;
    /** 请求编号(RPC 配对键)。 */
    uniqueId: number;
    /** 消息号 (module << 16) | id。 */
    messageId: number;
}

/**
 * 编码包头到 14 字节大端缓冲。
 * @param header 包头字段
 * @returns 14 字节 ArrayBuffer
 */
export function encodePacketHeader(header: PacketHeader): ArrayBuffer {
    const buffer = new ArrayBuffer(PACKET_HEADER_LENGTH);
    const view = new DataView(buffer);
    view.setUint32(0, header.packetLength, false);
    view.setUint8(4, header.operationType);
    view.setUint8(5, header.zipFlag);
    view.setInt32(6, header.uniqueId, false);
    view.setInt32(10, header.messageId, false);
    return buffer;
}

/**
 * 从缓冲偏移处解码包头。
 * @param buffer 数据源
 * @param offset 起始偏移
 * @returns 包头;长度不足返回 null
 */
export function decodePacketHeader(buffer: ArrayBuffer, offset: number = 0): PacketHeader | null {
    if (buffer.byteLength - offset < PACKET_HEADER_LENGTH) {
        return null;
    }

    const view = new DataView(buffer, offset);
    return {
        packetLength: view.getUint32(0, false),
        operationType: view.getUint8(4),
        zipFlag: view.getUint8(5),
        uniqueId: view.getInt32(6, false),
        messageId: view.getInt32(10, false),
    };
}
