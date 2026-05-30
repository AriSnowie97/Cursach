namespace FreelancePlatform.Client.Models
{
    public class Proposal
    {
        public int Id { get; set; }
        public string CoverLetter { get; set; }
        public decimal Price { get; set; }
        public int OrderId { get; set; }
        public int FreelancerId { get; set; }
        public User Freelancer { get; set; }
    }
}