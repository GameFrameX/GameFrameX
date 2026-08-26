export enum NetworkSatus {
    /** 
     * 连接中 
    */
    STATUS_CONNECTING = 1,
    /** 
     * 检验中 
     * */
    STATUS_CHECKING = 2,
    /** 
     * 连接生效 
     * */
    STATUS_COMMUNICATION = 3,
    /** 
     * 关闭连接 
     * */
    STATUS_DISCONNECT = 4
}