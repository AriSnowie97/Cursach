namespace FreelancePlatform.Client.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public string Name { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Role { get; set; } = "";
        public string? AvatarUrl { get; set; }
    }
}