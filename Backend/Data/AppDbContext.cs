using FreelancePlatformApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FreelancePlatformApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Це наші майбутні таблиці в базі даних
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Proposal> Proposals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Налаштовуємо зв'язки (Хто кому належить)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId);

            modelBuilder.Entity<Proposal>()
                .HasOne(p => p.Freelancer)
                .WithMany(u => u.Proposals)
                .HasForeignKey(p => p.FreelancerId);
                
            modelBuilder.Entity<Proposal>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Proposals)
                .HasForeignKey(p => p.OrderId);
        }
    }
}