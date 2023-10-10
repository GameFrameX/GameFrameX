using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;

namespace Server.Core.Net.Websocket
{
    /// <summary>
    /// TCP server
    /// </summary>
    public static class WebSocketServer
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private static WebApplication App { get; set; }

        public static Task Start(string url, WebSocketConnectionHandler handler)
        {
            var builder = WebApplication.CreateBuilder();

            builder.WebHost.UseUrls(url).UseNLog();
            App = builder.Build();

            App.UseWebSockets();

            App.Map("/ws", async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var clientAddress = $"{context.Connection?.RemoteIpAddress}:{context.Connection?.RemotePort}";
                    await handler.OnConnectedAsync(webSocket, clientAddress);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            });
            Log.Info("启动websocket服务...");
            return App.StartAsync();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public static Task Stop()
        {
            if (App != null)
            {
                Log.Info("停止websocket服务...");
                var task = App.StopAsync();
                App = null;
                return task;
            }

            return Task.CompletedTask;
        }
    }
}