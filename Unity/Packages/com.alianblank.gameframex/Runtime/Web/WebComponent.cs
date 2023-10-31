//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFrameX.Web;
using UnityEngine;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// Web 请求组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Web")]
    public sealed class WebComponent : GameFrameworkComponent
    {
        private IWebManager _webManager;

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            new WebManager();
            _webManager = GameFrameworkEntry.GetModule<IWebManager>();
            if (_webManager == null)
            {
                Log.Fatal("Web manager is invalid.");
                return;
            }
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        public UniTask<string> GetToString(string url)
        {
            return _webManager.GetToString(url).AsUniTask();
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">请求参数</param>
        /// <returns></returns>
        public UniTask<string> GetToString(string url, Dictionary<string, string> queryString)
        {
            return _webManager.GetToString(url, queryString).AsUniTask();
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">请求参数</param>
        /// <param name="header">请求头</param>
        /// <returns></returns>
        public UniTask<string> GetToString(string url, Dictionary<string, string> queryString, Dictionary<string, string> header)
        {
            return _webManager.GetToString(url, queryString, header).AsUniTask();
        }


        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        public UniTask<byte[]> GetToBytes(string url)
        {
            return _webManager.GetToBytes(url).AsUniTask();
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">请求参数</param>
        /// <returns></returns>
        public UniTask<byte[]> GetToBytes(string url, Dictionary<string, string> queryString)
        {
            return _webManager.GetToBytes(url, queryString).AsUniTask();
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">请求参数</param>
        /// <param name="header">请求头</param>
        /// <returns></returns>
        public UniTask<byte[]> GetToBytes(string url, Dictionary<string, string> queryString, Dictionary<string, string> header)
        {
            return _webManager.GetToBytes(url, queryString, header).AsUniTask();
        }


        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">请求参数</param>
        /// <returns></returns>
        public UniTask<string> PostToString(string url, Dictionary<string, string> from)
        {
            return _webManager.PostToString(url, from).AsUniTask();
        }

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单请求参数</param>
        /// <param name="queryString">URl请求参数</param>
        /// <returns></returns>
        public UniTask<string> PostToString(string url, Dictionary<string, string> from, Dictionary<string, string> queryString)
        {
            return _webManager.PostToString(url, from, queryString).AsUniTask();
        }

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单请求参数</param>
        /// <param name="queryString">URl请求参数</param>
        /// <param name="header">请求头</param>
        /// <returns></returns>
        public UniTask<string> PostToString(string url, Dictionary<string, string> from, Dictionary<string, string> queryString, Dictionary<string, string> header)
        {
            return _webManager.PostToString(url, from, queryString, header).AsUniTask();
        }


        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">请求参数</param>
        /// <returns></returns>
        public UniTask<byte[]> PostToBytes(string url, Dictionary<string, string> from)
        {
            return _webManager.PostToBytes(url, from).AsUniTask();
        }

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单请求参数</param>
        /// <param name="queryString">URl请求参数</param>
        /// <returns></returns>
        public UniTask<byte[]> PostToBytes(string url, Dictionary<string, string> from, Dictionary<string, string> queryString)
        {
            return _webManager.PostToBytes(url, from, queryString).AsUniTask();
        }

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单请求参数</param>
        /// <param name="queryString">URl请求参数</param>
        /// <param name="header">请求头</param>
        /// <returns></returns>
        public UniTask<byte[]> PostToBytes(string url, Dictionary<string, string> from, Dictionary<string, string> queryString, Dictionary<string, string> header)
        {
            return _webManager.PostToBytes(url, from, queryString, header).AsUniTask();
        }
    }
}