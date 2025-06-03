using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LogiTrack.Models;

namespace LogiTrack.Context
{
    public class LogiTrackContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<Order> Orders { get; set; }

        public LogiTrackContext(DbContextOptions<LogiTrackContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // Replace with your actual SQL Server connection string
            options.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=LogiTrackDb;Trusted_Connection=True;",
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5, // Number of retry attempts
                        maxRetryDelay: TimeSpan.FromSeconds(10), // Delay between retries
                        errorNumbersToAdd: null // Additional SQL error codes to consider transient
                    );
                });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure one-to-many relationship
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}