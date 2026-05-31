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

        // Хто створив замовлення (Замовник)
        public int CustomerId { get; set; }
        public User? Customer { get; set; }

        // Пропозиції на це замовлення
        public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
    }
}