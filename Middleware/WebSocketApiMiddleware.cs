using EzFtl.Models;
using EzFtl.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace EzFtl.Middleware
{
    public class WebSocketApiMiddleware
    {
        // Private types
        private class WebSocketChannelUpdateModel
        {
            [JsonPropertyName("channel_id")]
            public int ChannelId { get; set; }

            [JsonPropertyName("channel_name")]
            public string ChannelName { get; set; }

            [JsonPropertyName("has_active_stream")]
            public bool HasActiveStream { get; set; }
            
            [JsonPropertyName("active_stream_id")]
            public int ActiveStreamId { get; set; }

            public static WebSocketChannelUpdateModel FromChannelModel(ChannelModel channel)
            {
                bool hasActiveStream = (channel.ActiveStreams.Count() > 0);
                int activeStreamId = hasActiveStream ? channel.ActiveStreams.First().Id : -1;
                return new WebSocketChannelUpdateModel()
                {
                    ChannelId = channel.Id,
                    ChannelName = channel.Name,
                    HasActiveStream = hasActiveStream,
                    ActiveStreamId = activeStreamId,
                };
            }
        }

        private readonly RequestDelegate _next;
        private ILogger<WebSocketApiMiddleware> _logger;
        private readonly StreamManagerService _streamManager;
        private readonly HashSet<WebSocket> _clients = new HashSet<WebSocket>();

        public WebSocketApiMiddleware(RequestDelegate next, ILogger<WebSocketApiMiddleware> logger,
            StreamManagerService streamManager)
        {
            _next = next;
            _logger = logger;
            _streamManager = streamManager;
            _streamManager.ChannelUpdated += OnChannelUpdated;
        }

        private void OnChannelUpdated(object sender, ChannelModel channel)
        {
            foreach (var client in _clients)
            {
                sendChannelUpdate(client, channel);
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                    {
                        await handleWebSocket(webSocket);
                    }
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }
            else
            {
                await _next(context);
            }
        }

        private async Task handleWebSocket(WebSocket webSocket)
        {
            _clients.Add(webSocket);
            var buffer = new byte[1024];
            while (true)
            {
                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (message == "channels")
                    {
                        // Send the client updates on all of our channels
                        var activeChannels = _streamManager.GetChannels();
                        foreach (var channel in activeChannels)
                        {
                            sendChannelUpdate(webSocket, channel);
                        }
                    }
                }
            }

            _clients.Remove(webSocket);
        }

        private void sendChannelUpdate(WebSocket client, ChannelModel channel)
        {
            string jsonPayload = JsonSerializer.Serialize(
                WebSocketChannelUpdateModel.FromChannelModel(channel));
            client.SendAsync(Encoding.UTF8.GetBytes(jsonPayload), WebSocketMessageType.Text, true,
                CancellationToken.None);
        }
    }
}