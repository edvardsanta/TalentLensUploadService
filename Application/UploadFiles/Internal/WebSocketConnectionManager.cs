using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace UploadFiles.Internal
{
    // TODO: Need to refactor this whole code
    public static class WebSocketConnectionManager
    {
        private static readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public static string AddSocket(WebSocket socket)
        {
            var userID = Guid.NewGuid().ToString();
            if (_sockets.TryAdd(userID, socket))
                return userID;
            else
                return string.Empty;
        }

        public static async Task RemoveAndCloseSocket(string id)
        {
            if (_sockets.TryRemove(id, out WebSocket? socket) && socket != null)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                socket.Dispose();
            }
        }
        public static async Task WaitForSocketToClose(WebSocket webSocket)
        {
            try
            {
                var buffer = new ArraySegment<byte>(new byte[1024]);  // Small buffer since we're not expecting large messages

                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var receivedMessage = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                        Console.WriteLine("Received message: " + receivedMessage);

                        // Handle received message logic
                        await HandleReceivedMessage(webSocket, receivedMessage);
                    }

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // The client has initiated a close handshake, respond accordingly
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                    }
                    // Ignore any received messages since the server is only sending out notifications
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as necessary
                Console.WriteLine("WebSocket connection exception: " + ex.ToString());
            }
            finally
            {
                // Ensure the WebSocket is closed properly
                if (webSocket.State != WebSocketState.Closed)
                    webSocket.Abort();

                // Here, you could also trigger an event to clean up any resources related to this WebSocket
            }
        }
        public static async Task SendToAllAsync(string message)
        {
            foreach (var pair in _sockets)
            {
                if (pair.Value.State == WebSocketState.Open)
                {
                    await SendMessageAsync(pair.Value, message);
                }
            }
        }

        private static async Task SendMessageAsync(WebSocket socket, string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        public static async Task SendFileContent(WebSocket webSocket, string fileName)
        {
           
            var message = new
            {
                FileName = fileName,
             
            };

            string messageJson = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(messageJson);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async Task BroadcastStatusUpdate(string fileName, string status)
        {
            var message = SerializeMessage(fileName, status, false);
            await SendToAllAsync(message);
        }

        public static async Task NotifyUser(string userId, string fileName, string status, bool isError = false)
        {
            if (_sockets.TryGetValue(userId, out WebSocket? socket) && socket.State == WebSocketState.Open)
            {
                var message = SerializeMessage(fileName, status, isError);
                await SendMessageAsync(socket, message);
            }
        }

        private static string SerializeMessage(string fileName, string status, bool isError)
        {
            var message = new { FileName = fileName, Status = status, IsError = isError };
            return JsonSerializer.Serialize(message);
        }


        private static async Task HandleReceivedMessage(WebSocket webSocket, string message)
        {
            // Deserialize the message to get command type and data
            var command = JsonSerializer.Deserialize<WebSocketCommand>(message);

            switch (command.Type)
            {
                case MessageType.FileInfo:
                    // Handle FileInfo command
                    await SendFileContent(webSocket, command.FileName);
                    break;
                case MessageType.StatusUpdate:
                    // Handle StatusUpdate command
                    await SendStatusUpdate(webSocket, command.FileName, command.Status);
                    break;
                case MessageType.Notification:
                    // Handle Notification command
                    await SendMessageAsync(webSocket, "Notification received");
                    break;
            }
        }
        public static async Task SendStatusUpdate(WebSocket webSocket, string fileName, FileStatus status)
        {
            var message = SerializeMessage(fileName, status.ToString(), false);
            await SendMessageAsync(webSocket, message);
        }
        private class WebSocketCommand
        {
            public MessageType Type { get; set; }
            public string FileName { get; set; }
            public FileStatus Status { get; set; }
        }
        public enum FileStatus
        {
            Uploading,
            Completed,
            Failed
        }

        public enum MessageType
        {
            StatusUpdate,
            FileInfo,
            Notification
        }

    }
}
