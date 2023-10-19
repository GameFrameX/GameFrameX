//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using BestHTTP;
using Cysharp.Threading.Tasks;
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
        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            HTTPManager.MaxConnectionPerServer = 20;
            HTTPManager.ConnectTimeout = new TimeSpan(0, 0, 5);
            HTTPManager.RequestTimeout = new TimeSpan(0, 0, 10);
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        public UniTask<string> Get(string url)
        {
            UniTaskCompletionSource<string> uniTaskCompletionSource = new UniTaskCompletionSource<string>();
            HTTPRequest httpRequest = new HTTPRequest(new Uri(url), HTTPMethods.Get, (request, response) =>
            {
                switch (request.State)
                {
                    case HTTPRequestStates.Finished:
                    {
                        if (response.IsSuccess)
                        {
                            uniTaskCompletionSource.TrySetResult(response.DataAsText);
                        }
                        else
                        {
                            uniTaskCompletionSource.TrySetException(new Exception(response.Message));
                        }
                    }
                        break;
                    case HTTPRequestStates.Error:
                        uniTaskCompletionSource.TrySetException(new Exception(response.Message));
                        break;
                    case HTTPRequestStates.Aborted:
                        uniTaskCompletionSource.TrySetCanceled();
                        break;
                    case HTTPRequestStates.ConnectionTimedOut:
                    case HTTPRequestStates.TimedOut:
                        uniTaskCompletionSource.TrySetException(new TimeoutException(response.Message));

                        break;
                }
            });
            httpRequest.Send();
            return uniTaskCompletionSource.Task;
        }

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">请求参数</param>
        /// <returns></returns>
        public UniTask<string> Post(string url, Dictionary<string, string> from)
        {
            UniTaskCompletionSource<string> uniTaskCompletionSource = new UniTaskCompletionSource<string>();
            HTTPRequest httpRequest = new HTTPRequest(new Uri(url), HTTPMethods.Post, (request, response) =>
            {
                switch (request.State)
                {
                    case HTTPRequestStates.Finished:
                    {
                        if (response.IsSuccess)
                        {
                            uniTaskCompletionSource.TrySetResult(response.DataAsText);
                        }
                        else
                        {
                            uniTaskCompletionSource.TrySetException(new Exception(response.Message));
                        }
                    }
                        break;
                    case HTTPRequestStates.Error:
                        uniTaskCompletionSource.TrySetException(new Exception(response.Message));
                        break;
                    case HTTPRequestStates.Aborted:
                        uniTaskCompletionSource.TrySetCanceled();
                        break;
                    case HTTPRequestStates.ConnectionTimedOut:
                    case HTTPRequestStates.TimedOut:
                        uniTaskCompletionSource.TrySetException(new TimeoutException(response.Message));

                        break;
                }
            });
            foreach (var kv in from)
            {
                httpRequest.AddField(kv.Key, kv.Value);
            }

            httpRequest.Send();
            return uniTaskCompletionSource.Task;
        }
    }
}