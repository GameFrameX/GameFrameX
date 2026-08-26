import UtilityIdGenerator from "../utility/UtilityIdGenerator";
import IMessage from "./IMessage";

export default class MessageObject {
    /**
     * 唯一标识,请不要外部修改
     */
    public UniqueId: number = 0;

    /**
     * 消息ID.请不要外部修改.
     */
    public MessageId: number = 0;

    /**
     *
     */
    constructor() {
        this.UniqueId = UtilityIdGenerator.getUniqueIntId();
    }
}
