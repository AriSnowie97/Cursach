using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using FreelancePlatformApi.Data;
using FreelancePlatformApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace FreelancePlatformApi.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        
        // Tracks userId -> connectionId
        private static readonly ConcurrentDictionary<int, string> OnlineUsers = new();

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userIdStr = httpContext?.Request.Query["userId"];
            if (int.TryParse(userIdStr, out int userId))
            {
                OnlineUsers[userId] = Context.ConnectionId;
                // Broadcast to all that this user is online
                await Clients.All.SendAsync("UserStatusChanged", userId, true);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            var userIdStr = httpContext?.Request.Query["userId"];
            if (int.TryParse(userIdStr, out int userId))
            {
                if (OnlineUsers.TryGetValue(userId, out var connId) && connId == Context.ConnectionId)
                {
                    OnlineUsers.TryRemove(userId, out _);
                    await Clients.All.SendAsync("UserStatusChanged", userId, false);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(int orderId, int senderId, int receiverId, string messageText)
        {
            // 1. Verify order exists and is InProgress
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.Status != "InProgress")
            {
                return; // Chat is only allowed for InProgress orders
            }

            // 2. Verify sender and receiver are allowed participants (Customer and Freelancer)
            if (!((order.CustomerId == senderId && order.FreelancerId == receiverId) ||
                  (order.CustomerId == receiverId && order.FreelancerId == senderId)))
            {
                return; // Unauthorized participant
            }

            // 3. Save message to database
            var message = new ChatMessage
            {
                OrderId = orderId,
                SenderId = senderId,
                ReceiverId = receiverId,
                MessageText = messageText,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            // Create a clean DTO to avoid EF navigation property circular reference issues
            // SignalR uses its own JSON serializer (separate from controllers), so IgnoreCycles
            // configured in AddControllers() does NOT apply here — we must send plain data.
            var messageDto = new
            {
                message.Id,
                message.OrderId,
                message.SenderId,
                message.ReceiverId,
                message.MessageText,
                message.SentAt,
                message.IsRead
            };

            // 4. Send message to receiver if online
            if (OnlineUsers.TryGetValue(receiverId, out var receiverConnectionId))
            {
                await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", messageDto);
            }

            // 5. Send message back to sender (confirming delivery/saving)
            if (OnlineUsers.TryGetValue(senderId, out var senderConnectionId))
            {
                await Clients.Client(senderConnectionId).SendAsync("ReceiveMessage", messageDto);
            }
        }

        public async Task MarkAsRead(int orderId, int senderId, int receiverId)
        {
            // Mark all messages from senderId to receiverId for orderId as read
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.OrderId == orderId && m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync();

                // Notify sender that their messages were read
                if (OnlineUsers.TryGetValue(senderId, out var senderConnectionId))
                {
                    await Clients.Client(senderConnectionId).SendAsync("MessagesRead", orderId, receiverId);
                }
            }
        }

        public async Task CheckUserOnline(int userId)
        {
            bool isOnline = OnlineUsers.ContainsKey(userId);
            await Clients.Caller.SendAsync("UserStatusResponse", userId, isOnline);
        }
    }
}
