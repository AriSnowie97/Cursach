namespace FreelancePlatformApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        
        // Роль: "Customer" (Замовник) або "Freelancer" (Фрілансер)
        public string Role { get; set; } = string.Empty; 
        
        // Зв'язки
        public List<Order> Orders { get; set; } = new();
        public List<Proposal> Proposals { get; set; } = new();
    }
}