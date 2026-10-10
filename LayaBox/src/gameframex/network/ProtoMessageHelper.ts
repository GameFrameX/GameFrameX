import type IMessage from "./IMessage";
import ProtoMessageRegister from "../protobuf/ProtoMessageRegister";

/**
 * 消息注册表。
 *
 * 承担 Unity `ProtoMessageIdHandler` 的职责:消息全名 ↔ 消息号 (module<<16)|id 双向表,
 * 请求表(Request)/响应表(Response+Notify)分离;消息类型解析依赖宿主注入的 protobuf Root。
 * 生成物(ProtoExport)的 `Xxx.register()` 调用 registerReqMessage/registerRespMessage 建表。
 */
export default class ProtoMessageHelper {
    /** 宿主注入的 protobuf Root(protobufjs),null 表示尚未就绪。 */
    private static root: { lookupType: (name: string) => IMessage } | null = null;

    /** 请求消息号表:全名 → id。 */
    private static readonly reqIdByType: Map<string, number> = new Map();

    /** 响应/通知消息号表:全名 → id。 */
    private static readonly respIdByType: Map<string, number> = new Map();

    /** 响应/通知全名表:id → 全名。 */
    private static readonly respTypeById: Map<number, string> = new Map();

    /** 心跳消息全名集合(registerRespMessage 时按 IHeartBeatMessage 语义无法区分,由生成物显式注册)。 */
    private static readonly heartBeatTypes: Set<string> = new Set();

    /**
     * 注入 protobuf Root 并完成全部消息注册。
     * @param root 宿主加载好的 protobufjs Root(或等价 lookupType 接口)
     */
    public static adopt(root: { lookupType: (name: string) => IMessage }): void {
        this.root = root;
        ProtoMessageRegister.register();
    }

    /**
     * 注册请求消息。由生成物调用,全名即 protobuf 类型全名。
     * @param fullName 消息全名(如 'User.ReqLogin')
     * @param messageId 消息号
     */
    public static registerReqMessage(fullName: string, messageId: number): void {
        this.reqIdByType.set(fullName, messageId);
        if (fullName.includes('HeartBeat')) {
            this.heartBeatTypes.add(fullName);
        }
    }

    /**
     * 注册响应/通知消息。由生成物调用。
     * @param fullName 消息全名(如 'User.RespLogin')
     * @param messageId 消息号
     */
    public static registerRespMessage(fullName: string, messageId: number): void {
        this.respIdByType.set(fullName, messageId);
        this.respTypeById.set(messageId, fullName);
        if (fullName.includes('HeartBeat')) {
            this.heartBeatTypes.add(fullName);
        }
    }

    /**
     * 是否心跳消息(包头 OperationType=1)。
     * @param fullName 消息全名
     */
    public static isHeartBeat(fullName: string): boolean {
        return this.heartBeatTypes.has(fullName);
    }

    /**
     * 按全名取 protobuf 消息类型(create/encode/decode)。
     * @param fullName 消息全名
     */
    public static getMessageType(fullName: string): IMessage {
        const type = this.root?.lookupType(fullName);
        if (type == null) {
            throw new Error(`protobuf type '${fullName}' not found (root adopted?)`);
        }

        return type;
    }

    /**
     * 请求消息号。
     * @param fullName 消息全名
     */
    public static getReqMessageId(fullName: string): number {
        const id = this.reqIdByType.get(fullName);
        if (id == null) {
            throw new Error(`request message '${fullName}' is not registered`);
        }

        return id;
    }

    /**
     * 响应/通知消息号。
     * @param fullName 消息全名
     */
    public static getRespMessageId(fullName: string): number {
        const id = this.respIdByType.get(fullName);
        if (id == null) {
            throw new Error(`response message '${fullName}' is not registered`);
        }

        return id;
    }

    /**
     * 响应/通知消息全名。
     * @param messageId 消息号
     */
    public static getRespMessageName(messageId: number): string | null {
        return this.respTypeById.get(messageId) ?? null;
    }

    /**
     * 已注册消息总数(启动自检用:应等于生成清单数量)。
     */
    public static get registeredCount(): number {
        return this.reqIdByType.size + this.respIdByType.size;
    }

    /**
     * 清空注册表(测试用)。
     */
    public static reset(): void {
        this.root = null;
        this.reqIdByType.clear();
        this.respIdByType.clear();
        this.respTypeById.clear();
        this.heartBeatTypes.clear();
    }
}
