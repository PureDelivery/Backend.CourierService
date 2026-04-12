using Microsoft.EntityFrameworkCore;
using PureDelivery.CourierService.Core.Models;
using PureDelivery.CourierService.Infrastructure.Configuration;

namespace PureDelivery.CourierService.Infrastructure.Data
{
    public class CourierDbContext : DbContext
    {
        public CourierDbContext(DbContextOptions<CourierDbContext> options) : base(options) { }

        public DbSet<Courier> Couriers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new CourierConfiguration());
        }
    }
}
