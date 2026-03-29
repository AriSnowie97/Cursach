namespace FreelancePlatformApi.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public DateTime Deadline { get; set; }
        public string Status { get; set; } = "Open"; // Open, InProgress, Completed

        // Кто создал заказ (Заказчик)
        public int CustomerId { get; set; }
        public User? Customer { get; set; }

        // Отклики на этот заказ
        public List<Proposal> Proposals { get; set; } = new();
    }
}