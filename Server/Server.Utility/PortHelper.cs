using System.Net;
using System.Net.Sockets;

namespace Server.Utility;

/// <summary>
/// 端口帮助类
/// </summary>
public static class PortHelper
{
    /// <summary>
    /// 异步扫描给定IP地址上指定范围内的端口，并返回可用端口列表。
    /// </summary>
    /// <param name="ip">目标IP</param>
    /// <param name="startPort">开始端口</param>
    /// <param name="endPort">结束端口</param>
    /// <returns>可用端口列表</returns>
    public static async Task<List<int>> ScanPorts(int startPort, int endPort, string? ip = null)
    {
        var availablePorts = new List<int>();
        for (int port = startPort; port <= endPort; port++)
        {
            if (await IsPortAvailable(port, ip))
            {
                availablePorts.Add(port);
            }
        }

        return availablePorts;
    }

    private static async Task<bool> IsPortAvailable(int port, string? ip = null)
    {
        IPAddress ipAddress = IPAddress.Any;
        if (ip != null)
        {
            ipAddress = IPAddress.Parse(ip);
        }

        var endpoint = new IPEndPoint(ipAddress, port);
        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

        try
        {
            await socket.ConnectAsync(endpoint);
            return false;
        }
        catch (SocketException)
        {
            // SocketException is thrown when the socket can't connect to the specified port, which means it's available.
            return true;
        }
        finally
        {
            socket.Close();
        }
    }
}