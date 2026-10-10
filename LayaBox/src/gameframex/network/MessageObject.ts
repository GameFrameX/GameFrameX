import UtilityIdGenerator from "../utility/UtilityIdGenerator";

/**
 * 网络消息基类。
 *
 * 对照 Unity `GameFrameX.Network.Runtime.MessageObject`:
 * 构造时分配单调递增 UniqueId;子类(生成物)携带同名只读 PackageName。
 */
export default class MessageObject {
    /**
     * 请求编号(RPC 配对键),由服务端响应原样回传。请不要外部修改。
     */
    public UniqueId: number = 0;

    /**
     * 消息ID.请不要外部修改.
     */
    public MessageId: number = 0;

    /**
     * 消息全名(如 'User.ReqLogin');由生成消息类覆写,基类仅占位类型。
     */
    public PackageName: string = '';

    constructor() {
        this.UniqueId = UtilityIdGenerator.getUniqueIntId();
    }
}
