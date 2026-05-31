using System.Collections.Generic;

namespace FreelancePlatform.Client.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Budget { get; set; }
        public string Status { get; set; } = "";
        public int CustomerId { get; set; }
        public User? Customer { get; set; }
        public int? FreelancerId { get; set; }
        public User? Freelancer { get; set; }
        public List<Proposal> Proposals { get; set; } = new();
    }
}