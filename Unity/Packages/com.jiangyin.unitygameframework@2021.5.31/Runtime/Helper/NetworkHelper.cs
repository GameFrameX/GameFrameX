using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 网络帮助类
    /// </summary>
    public static class NetworkHelper
    {
        /// <summary>
        /// 获取本地的IP列表
        /// </summary>
        /// <returns></returns>
        public static string[] GetAddressIPs()
        {
            //获取本地的IP地址
            var list = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            string[] addressIPs = new string[list.Length];
            for (var index = 0; index < list.Length; index++)
            {
                IPAddress address = list[index];
                addressIPs[index] = address.ToString();
            }

            return addressIPs;
        }

        /// <summary>
        /// 是否有网络
        /// </summary>
        /// <returns></returns>
        public static bool IsReachable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        /// <summary>
        /// 是否是WIFI
        /// </summary>
        /// <returns></returns>
        public static bool IsWifi()
        {
            return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }

        /// <summary>
        /// 是否是移动网络
        /// </summary>
        /// <returns></returns>
        public static bool IsViaCarrierData()
        {
            //当用户使用移动网络时
            return Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork;
        }
    }
}