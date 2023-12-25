using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using NLog;
using Server.Setting;

namespace Server.NetWork.HTTP
{
    public static class HttpHandler
    {
        static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task HandleRequest(HttpContext context, Func<string, BaseHttpHandler> baseHandler)
        {
            try
            {
                string ip = context.Connection.RemoteIpAddress.ToString();
                string url = context.Request.PathBase + context.Request.Path;
                Logger.Info("收到来自[{}]的HTTP请求. 请求url:[{}]", ip, url);
                Dictionary<string, string> paramMap = new Dictionary<string, string>();

                foreach (var keyValuePair in context.Request.Query)
                {
                    paramMap.Add(keyValuePair.Key, keyValuePair.Value.ToString());
                }

                context.Response.Headers.Add("content-type", "application/json; charset=utf-8");
                if (context.Request.Method.Equals("POST"))
                {
                    var headCType = context.Request.ContentType;
                    if (string.IsNullOrEmpty(headCType))
                    {
                        await context.Response.WriteAsync("http header content type is null");
                        return;
                    }

                    var isJson = context.Request.HasJsonContentType();
                    var isForm = context.Request.HasFormContentType;
                    Console.WriteLine("isJson:" + isJson);
                    if (isJson)
                    {
                        JsonElement json = await context.Request.ReadFromJsonAsync<JsonElement>();
                        foreach (var keyValuePair in json.EnumerateObject())
                        {
                            if (paramMap.ContainsKey(keyValuePair.Name))
                            {
                                await context.Response.WriteAsync(HttpResult.CreateErrorParam("参数重复了:" + keyValuePair.Name));
                                return;
                            }

                            var key = keyValuePair.Name;
                            var val = keyValuePair.Value.GetString();
                            paramMap.TryAdd(key, val);
                        }
                    }
                    else if (isForm)
                    {
                        foreach (var keyValuePair in context.Request.Form)
                        {
                            if (paramMap.ContainsKey(keyValuePair.Key))
                            {
                                await context.Response.WriteAsync(HttpResult.CreateErrorParam("参数重复了:" + keyValuePair.Key));
                                return;
                            }

                            paramMap.Add(keyValuePair.Key, keyValuePair.Value.ToString());
                        }
                    }
                }

                var str = new StringBuilder();
                str.Append("请求参数:");
                foreach (var parameter in paramMap)
                {
                    if (parameter.Key.Equals(""))
                        continue;
                    str.Append("'").Append(parameter.Key).Append("'='").Append(parameter.Value).Append("'  ");
                }

                Logger.Info(str.ToString());

                if (!paramMap.TryGetValue("command", out var cmd))
                {
                    await context.Response.WriteAsync(HttpResult.Undefine);
                    return;
                }

                if (!GlobalSettings.IsAppRunning)
                {
                    await context.Response.WriteAsync(HttpResult.CreateActionFailed("服务器状态错误[正在起/关服]"));
                    return;
                }

                var handler = baseHandler(cmd);
                if (handler == null)
                {
                    Logger.Warn($"http cmd handler 不存在：{cmd}");
                    await context.Response.WriteAsync(HttpResult.Undefine);
                    return;
                }

                //验证
                var checkCode = handler.CheckSign(paramMap);
                if (!string.IsNullOrEmpty(checkCode))
                {
                    await context.Response.WriteAsync(checkCode);
                    return;
                }

                var ret = await Task.Run(() => { return handler.Action(ip, url, paramMap); });
                Logger.Warn("http result:" + ret);
                await context.Response.WriteAsync(ret);
            }
            catch (Exception e)
            {
                Logger.Error("执行http异常. {} {}", e.Message, e.StackTrace);
                await context.Response.WriteAsync(e.Message);
            }
        }
    }
}