using System.Net.WebSockets;
using UploadFiles.Internal;

namespace UploadFiles.Middleware
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;

        public WebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var userId = context.Request.Query["userId"];
                    await HandleWebSocket(context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await _next(context);
            }
        }

        public async Task HandleWebSocket(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

                var idSocket = WebSocketConnectionManager.AddSocket(webSocket);

                await WebSocketConnectionManager.WaitForSocketToClose(webSocket);

                await WebSocketConnectionManager.RemoveAndCloseSocket(idSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }
    }
}
