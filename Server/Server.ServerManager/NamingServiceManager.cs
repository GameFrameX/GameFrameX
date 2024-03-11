using System.Collections.Concurrent;
using Server.Log;
using Server.Setting;
using Server.Utility;

namespace Server.ServerManager
{
    /// <summary>
    /// 服务器管理
    /// </summary>
    public sealed class NamingServiceManager : Singleton<NamingServiceManager>
    {

        /// <summary>
        /// 服务器节点的id 为自身的serverId
        /// </summary>
        readonly ConcurrentDictionary<long, ServerInfo> serverMap = new ConcurrentDictionary<long, ServerInfo>();

        /// <summary>
        /// 根据节点数据从服务器列表中删除
        /// </summary>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public bool TryRemove(long serverId)
        {
            return serverMap.TryRemove(serverId, out _);
        }

        /// <summary>
        /// 根据节点数据从服务器列表中删除
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool TryRemove(ServerInfo info)
        {
            if (info == null)
            {
                return true;
            }

            return serverMap.TryRemove(new KeyValuePair<long, ServerInfo>(info.ServerId, info));
        }

        /// <summary>
        /// 根据id获取节点数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ServerInfo TryGet(long id)
        {
            serverMap.TryGetValue(id, out var v);
            return v;
        }

        /// <summary>
        /// 获取节点列表
        /// </summary>
        /// <returns></returns>
        public List<ServerInfo> GetAllNodes()
        {
            return serverMap.Values.ToList();
        }

        /// <summary>
        /// 获取节点数量
        /// </summary>
        /// <returns></returns>
        public int GetNodeCount()
        {
            return serverMap.Count;
        }

        private ServerInfo serverInfo;

        /// <summary>
        /// 添加自身
        /// </summary>
        public void AddSelf(BaseSetting setting)
        {
            serverInfo = new ServerInfo(setting.ServerType, setting.ServerName, setting.ServerId, setting.LocalIp, setting.GrpcPort);
            serverMap[serverInfo.ServerId] = serverInfo;
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="node">节点信息</param>
        public void Add(ServerInfo node)
        {
            if (node.Type == serverInfo.Type)
            {
                LogHelper.Error($"不能添加discovery节点...{node}");
                return;
            }

            serverMap[node.ServerId] = node;
            LogHelper.Info($"新的网络节点:[{node}]   总数：{GetNodeCount()}");
        }

        /// <summary>
        /// 根据服务器类型获取节点列表
        /// </summary>
        /// <param name="type">服务器类型</param>
        /// <returns></returns>
        public List<ServerInfo> GetNodesByType(ServerType type)
        {
            var list = new List<ServerInfo>();
            foreach (var node in serverMap)
            {
                if (node.Value.Type == type)
                {
                    list.Add(node.Value);
                }
            }

            return list;
        }

        /// <summary>
        /// 设置节点的状态信息
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <param name="statusInfo">节点状态信息</param>
        public void SetNodeState(long nodeId, ServerStatusInfo statusInfo)
        {
            //Log.Debug($"设置节点{nodeId}状态");
            if (serverMap.TryGetValue(nodeId, out var node))
            {
                node.StatusInfo = statusInfo;
            }
        }

        /// <summary>
        /// 设置节点的状态
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <param name="state">节点状态</param>
        public void SetNodeState(long nodeId, ServerStatus state)
        {
            //Log.Debug($"设置节点{nodeId}状态");
            if (serverMap.TryGetValue(nodeId, out var node))
            {
                node.Status = state;
            }
        }
    }
}