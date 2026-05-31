namespace FreelancePlatformApi.Models
{
    public class Proposal
    {
        public int Id { get; set; }
        public string CoverLetter { get; set; }
        public decimal Price { get; set; }
        public int DeliveryDays { get; set; }

        // До якого замовлення відноситься
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // Хто залишив відгук (Фрілансер)
        public int FreelancerId { get; set; }
        public User? Freelancer { get; set; }
    }
}