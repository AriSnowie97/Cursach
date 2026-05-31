using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancePlatformApi.Data;
using FreelancePlatformApi.Models;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace FreelancePlatformApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/chat/{orderId}/history?userId={userId}
        [HttpGet("{orderId}/history")]
        public async Task<IActionResult> GetChatHistory(int orderId, [FromQuery] int userId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound("Замовлення не знайдено");
            }

            // Verify the order is InProgress
            if (order.Status != "InProgress")
            {
                return BadRequest("Чат доступний тільки під час виконання замовлення");
            }

            // Verify the user is a participant
            if (order.CustomerId != userId && order.FreelancerId != userId)
            {
                return Forbid("Ви не є учасником цього замовлення");
            }

            var messages = await _context.ChatMessages
                .Where(m => m.OrderId == orderId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return Ok(messages);
        }

        // PUT api/chat/{orderId}/read?userId={userId}
        [HttpPut("{orderId}/read")]
        public async Task<IActionResult> MarkMessagesAsRead(int orderId, [FromQuery] int userId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound("Замовлення не знайдено");
            }

            // Verify the user is a participant
            if (order.CustomerId != userId && order.FreelancerId != userId)
            {
                return Forbid("Ви не є учасником цього замовлення");
            }

            // Find all unread messages where the current user is the receiver
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.OrderId == orderId && m.ReceiverId == userId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new { Count = unreadMessages.Count });
        }
    }
}
