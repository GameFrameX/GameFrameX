using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Server.Utility;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Server.NetWork.WebSocket
{
    /// <summary>
    /// TCP server
    /// </summary>
    public static class WebSocketServer
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private static WebApplication? App { get; set; }
        public static IMessageHelper MessageHelper { get; private set; }

        public static Task Start(int wsPort, int wssPort,string wssCertFilePath, IMessageHelper messageHelper, WebSocketConnectionHandler handler)
        {
            Guard.NotNull(messageHelper, nameof(messageHelper));
            Guard.NotNull(handler, nameof(handler));
            MessageHelper = messageHelper;
            var builder = WebApplication.CreateBuilder();
            builder.WebHost
                .UseKestrel(
                    options =>
                    {
                        options.ListenAnyIP(wsPort);
                        if (wssPort > 0)
                        {
                            options.ListenAnyIP(wssPort, listenOptions =>
                            {
                                listenOptions.UseHttps((httpsConnectionAdapterOptions) =>
                                {
                                    httpsConnectionAdapterOptions.ServerCertificate = X509Certificate2.CreateFromPemFile(wssCertFilePath);
                                });
                            });
                        }
                    })
                .ConfigureLogging(logging => { logging.SetMinimumLevel(LogLevel.Error); })
                .UseNLog();
            App = builder.Build();
            Log.Info("启动websocket服务...");
            App.UseWebSockets();

            async Task RequestDelegate(HttpContext context)
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
            }

            App.Map("/ws", RequestDelegate);
            if (wssPort > 0)
            {
                App.Map("/wss", RequestDelegate);
            }

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