using System.Net;
using System.Net.Sockets;

namespace GameFrameX.Network
{
    /// <summary>
    /// 网络套接字接口。
    /// </summary>
    public interface INetworkSocket
    {
        /// <summary>
        /// 获取是否已连接。
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 获取本地终结点。
        /// </summary>
        EndPoint LocalEndPoint { get; }

        /// <summary>
        /// 获取远程终结点。
        /// </summary>
        EndPoint RemoteEndPoint { get; }

        /// <summary>
        /// 获取或设置接收缓冲区大小。
        /// </summary>
        int ReceiveBufferSize { get; set; }

        /// <summary>
        /// 获取或设置发送缓冲区大小。
        /// </summary>
        int SendBufferSize { get; set; }

        /// <summary>
        /// 关闭网络套接字。
        /// </summary>
        void Shutdown();

        /// <summary>
        /// 关闭网络套接字并释放资源。
        /// </summary>
        void Close();
    }
}