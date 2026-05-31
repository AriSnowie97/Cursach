using System;

namespace FreelancePlatformApi.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string MessageText { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        // Зв'язки
        public Order? Order { get; set; }
        public User? Sender { get; set; }
        public User? Receiver { get; set; }
    }
}
