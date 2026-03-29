namespace FreelancePlatformApi.Models
{
    public class Proposal
    {
        public int Id { get; set; }
        public string CoverLetter { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DeliveryDays { get; set; }

        // К какому заказу относится
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // Кто оставил отклик (Фрилансер)
        public int FreelancerId { get; set; }
        public User? Freelancer { get; set; }
    }
}