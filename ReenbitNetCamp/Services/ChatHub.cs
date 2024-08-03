using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Azure.AI.TextAnalytics;
using ReenbitNetCamp.Models;

namespace ReenbitNetCamp.Services
{
    public class ChatHub : Hub
    {
        readonly IAiService _aiService;
        readonly ChatContext _chatContext;
        public ChatHub(IAiService aiService, ChatContext chatContext)
        {
            _aiService = aiService;
            _chatContext = chatContext;
            _chatContext.Database.EnsureCreated();
        }

        public const string HubUrl = "/chat";
        private static ConcurrentDictionary<string, List<string>> _connectedUsers = new ConcurrentDictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        public async Task Broadcast(string username, string reciever, string message)
        {
            TextSentiment sentiment = _aiService.GetSentiment(message);
            DateTimeOffset time = DateTimeOffset.UtcNow;

            //If there is no recipient, send broadcast message
            if (string.IsNullOrEmpty(reciever))
            {
                await Clients.All.SendAsync("Broadcast", username, message, sentiment, time);
                _chatContext.Add(new Message() { User = username, Reciever = "", Text = message, Sentiment = sentiment, SendTime = time });
                await _chatContext.SaveChangesAsync();
            }
            //else send direct message
            else if (_connectedUsers.TryGetValue(reciever, out var connections) && connections.Count > 0)
            {
                await Clients.Clients(connections).SendAsync("SendToUser", username, reciever, message, sentiment, time);
                //also send message to sender as acknowledgement
                if (reciever != username && _connectedUsers.TryGetValue(username, out var usernameConn) && usernameConn.Count > 0)
                {
                    await Clients.Clients(usernameConn).SendAsync("SendToUser", username, reciever, message, sentiment, time);
                }
                _chatContext.Add(new Message() { User = username, Reciever = reciever, Text = message, Sentiment = sentiment, SendTime = time });
                await _chatContext.SaveChangesAsync();
            }
        }

        public override async Task OnConnectedAsync()
        {
            //Update connections
            string? username = Context.GetHttpContext()?.Request.Query["username"];
            if (username != null)
            {
                if(_connectedUsers.TryGetValue(username, out var connections))
                {
                    connections.Add(Context.ConnectionId);
                }
                else
                {
                    connections = new List<string>() { Context.ConnectionId };
                }
                _connectedUsers[username] = connections;
                Console.WriteLine($"{Context.ConnectionId}:${username} connected");

                //Send message about connection
                DateTimeOffset time = DateTimeOffset.UtcNow;
                await Clients.All.SendAsync("UpdateConnectedUsers", _connectedUsers.Keys);
                await base.OnConnectedAsync();
                await Clients.All.SendAsync("Broadcast", "", $"{username} connected", TextSentiment.Neutral, time);
                _chatContext.Add(new Message() { User = "", Reciever = "", Text = $"{username} connected", Sentiment = TextSentiment.Neutral, SendTime = time });
                await _chatContext.SaveChangesAsync();
            }
        }

        public override async Task OnDisconnectedAsync(Exception? e)
        {
            //Update connections
            string? username = Context.GetHttpContext()?.Request.Query["username"];
            if (username != null)
            {
                if (_connectedUsers.TryGetValue(username, out var connections) && connections.Contains(Context.ConnectionId))
                {
                    connections.Remove(Context.ConnectionId);
                    _connectedUsers[username] = connections;
                    if (connections.Count == 0)
                    {
                        _connectedUsers.Remove(username, out var removed);
                    }
                }
                Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}:${username}");

                //Send message about disconnection
                DateTimeOffset time = DateTimeOffset.UtcNow;
                await Clients.All.SendAsync("UpdateConnectedUsers", _connectedUsers.Keys);
                await base.OnDisconnectedAsync(e);
                await Clients.All.SendAsync("Broadcast", "", $"{username} disconnected", TextSentiment.Neutral, time);
                _chatContext.Add(new Message() { User = "", Reciever = "", Text = $"{username} disconnected", Sentiment = TextSentiment.Neutral, SendTime = time });
                await _chatContext.SaveChangesAsync();
            }
        }
    }
}
