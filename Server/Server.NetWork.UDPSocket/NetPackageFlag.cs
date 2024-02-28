namespace Geek.Server.Core.Net.Kcp;

public static class NetPackageFlag
{
    public const byte SYN = 1;
    public const byte SYN_OLD_NET_ID = 2;
    public const byte ACK = 3;
    public const byte HEART = 4;
    public const byte NO_GATE_CONNECT = 5;
    public const byte CLOSE = 6;
    public const byte NO_INNER_SERVER = 7;
    public const byte MSG = 8;

    public static string GetFlagDesc(byte flag)
    {
        switch (flag)
        {
            case SYN:
                return "连接请求";
            case SYN_OLD_NET_ID:
                return "带NetId的连接请求";
            case ACK:
                return "连接应答";
            case HEART:
                return "心跳";
            case NO_GATE_CONNECT:
                return "无网关连接";
            case CLOSE:
                return "关闭";
            case MSG:
                return "消息";
            case NO_INNER_SERVER:
                return "无内部服务器";
            default:
                return "无效标记:" + flag;
        }
    }
}