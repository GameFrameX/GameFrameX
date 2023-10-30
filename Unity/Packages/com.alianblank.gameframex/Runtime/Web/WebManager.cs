using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BestHTTP;
using GameFrameX.Runtime;

namespace GameFrameX.Web
{
    public class WebManager : GameFrameworkModule, IWebManager
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder(256);

        public WebManager()
        {
            HTTPManager.MaxConnectionPerServer = 20;
            HTTPManager.ConnectTimeout = new TimeSpan(0, 0, 5);
            HTTPManager.RequestTimeout = new TimeSpan(0, 0, 10);
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        public Task<string> GetToString(string url)
        {
            return GetToString(url, null, null);
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        public Task<byte[]> GetToBytes(string url)
        {
            return GetToBytes(url, null, null);
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">请求参数</param>
        /// <returns></returns>
        public Task<string> GetToString(string url, Dictionary<string, string> queryString)
        {
            return GetToString(url, queryString, null);
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">请求参数</param>
        /// <returns></returns>
        public Task<byte[]> GetToBytes(string url, Dictionary<string, string> queryString)
        {
            return GetToBytes(url, queryString, null);
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">请求参数</param>
        /// <param name="header">请求头</param>
        /// <returns></returns>
        public Task<string> GetToString(string url, Dictionary<string, string> queryString, Dictionary<string, string> header)
        {
            var uniTaskCompletionSource = new TaskCompletionSource<string>();
            _stringBuilder.Clear();

            if (queryString.Count > 0)
            {
                _stringBuilder.Append(url);
                if (!url.EndsWithFast("?"))
                {
                    _stringBuilder.Append("?");
                }

                foreach (var kv in queryString)
                {
                    _stringBuilder.AppendFormat("{0}={1}&", kv.Key, kv.Value);
                }

                url = _stringBuilder.ToString(0, _stringBuilder.Length - 1);
                _stringBuilder.Clear();
            }


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
            if (header != null && header.Count > 0)
            {
                foreach (var kv in header)
                {
                    httpRequest.SetHeader(kv.Key, kv.Value);
                }
            }

            httpRequest.Send();
            return uniTaskCompletionSource.Task;
        }

        /// <summary>
        /// 发送Get 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryString">请求参数</param>
        /// <param name="header">请求头</param>
        /// <returns></returns>
        public Task<byte[]> GetToBytes(string url, Dictionary<string, string> queryString, Dictionary<string, string> header)
        {
            TaskCompletionSource<byte[]> uniTaskCompletionSource = new TaskCompletionSource<byte[]>();
            _stringBuilder.Clear();

            if (queryString.Count > 0)
            {
                _stringBuilder.Append(url);
                if (!url.EndsWithFast("?"))
                {
                    _stringBuilder.Append("?");
                }

                foreach (var kv in queryString)
                {
                    _stringBuilder.AppendFormat("{0}={1}&", kv.Key, kv.Value);
                }

                url = _stringBuilder.ToString(0, _stringBuilder.Length - 1);
                _stringBuilder.Clear();
            }


            HTTPRequest httpRequest = new HTTPRequest(new Uri(url), HTTPMethods.Get, (request, response) =>
            {
                switch (request.State)
                {
                    case HTTPRequestStates.Finished:
                    {
                        if (response.IsSuccess)
                        {
                            uniTaskCompletionSource.TrySetResult(response.Data);
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
            if (header != null && header.Count > 0)
            {
                foreach (var kv in header)
                {
                    httpRequest.SetHeader(kv.Key, kv.Value);
                }
            }

            httpRequest.Send();
            return uniTaskCompletionSource.Task;
        }


        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">请求参数</param>
        /// <returns></returns>
        public Task<string> PostToString(string url, Dictionary<string, string> from)
        {
            return PostToString(url, from, null, null);
        }

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">请求参数</param>
        /// <returns></returns>
        public Task<byte[]> PostToBytes(string url, Dictionary<string, string> from)
        {
            return PostToBytes(url, from, null, null);
        }

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单请求参数</param>
        /// <param name="queryString">URl请求参数</param>
        /// <returns></returns>
        public Task<string> PostToString(string url, Dictionary<string, string> from, Dictionary<string, string> queryString)
        {
            return PostToString(url, from, queryString, null);
        }

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单请求参数</param>
        /// <param name="queryString">URl请求参数</param>
        /// <returns></returns>
        public Task<byte[]> PostToBytes(string url, Dictionary<string, string> from, Dictionary<string, string> queryString)
        {
            return PostToBytes(url, from, queryString, null);
        }


        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单请求参数</param>
        /// <param name="queryString">URl请求参数</param>
        /// <param name="header">请求头</param>
        /// <returns></returns>
        public Task<string> PostToString(string url, Dictionary<string, string> from, Dictionary<string, string> queryString, Dictionary<string, string> header)
        {
            TaskCompletionSource<string> uniTaskCompletionSource = new TaskCompletionSource<string>();

            _stringBuilder.Clear();

            if (queryString.Count > 0)
            {
                _stringBuilder.Append(url);
                if (!url.EndsWithFast("?"))
                {
                    _stringBuilder.Append("?");
                }

                foreach (var kv in queryString)
                {
                    _stringBuilder.AppendFormat("{0}={1}&", kv.Key, kv.Value);
                }

                url = _stringBuilder.ToString(0, _stringBuilder.Length - 1);
                _stringBuilder.Clear();
            }

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
            if (from != null && from.Count > 0)
            {
                foreach (var kv in from)
                {
                    httpRequest.AddField(kv.Key, kv.Value);
                }
            }

            if (header != null && header.Count > 0)
            {
                foreach (var kv in header)
                {
                    httpRequest.SetHeader(kv.Key, kv.Value);
                }
            }

            httpRequest.Send();
            return uniTaskCompletionSource.Task;
        }

        /// <summary>
        /// 发送Post 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="from">表单请求参数</param>
        /// <param name="queryString">URl请求参数</param>
        /// <param name="header">请求头</param>
        /// <returns></returns>
        public Task<byte[]> PostToBytes(string url, Dictionary<string, string> from, Dictionary<string, string> queryString, Dictionary<string, string> header)
        {
            TaskCompletionSource<byte[]> uniTaskCompletionSource = new TaskCompletionSource<byte[]>();

            _stringBuilder.Clear();

            if (queryString.Count > 0)
            {
                _stringBuilder.Append(url);
                if (!url.EndsWithFast("?"))
                {
                    _stringBuilder.Append("?");
                }

                foreach (var kv in queryString)
                {
                    _stringBuilder.AppendFormat("{0}={1}&", kv.Key, kv.Value);
                }

                url = _stringBuilder.ToString(0, _stringBuilder.Length - 1);
                _stringBuilder.Clear();
            }

            HTTPRequest httpRequest = new HTTPRequest(new Uri(url), HTTPMethods.Post, (request, response) =>
            {
                switch (request.State)
                {
                    case HTTPRequestStates.Finished:
                    {
                        if (response.IsSuccess)
                        {
                            uniTaskCompletionSource.TrySetResult(response.Data);
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
            if (from != null && from.Count > 0)
            {
                foreach (var kv in from)
                {
                    httpRequest.AddField(kv.Key, kv.Value);
                }
            }

            if (header != null && header.Count > 0)
            {
                foreach (var kv in header)
                {
                    httpRequest.SetHeader(kv.Key, kv.Value);
                }
            }

            httpRequest.Send();
            return uniTaskCompletionSource.Task;
        }
    }
}