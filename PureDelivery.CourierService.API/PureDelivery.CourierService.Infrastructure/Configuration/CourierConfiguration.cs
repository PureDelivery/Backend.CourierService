using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PureDelivery.CourierService.Core.Models;

namespace PureDelivery.CourierService.Infrastructure.Configuration
{
    public class CourierConfiguration : IEntityTypeConfiguration<Courier>
    {
        public void Configure(EntityTypeBuilder<Courier> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.UserId).IsRequired();
            builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Phone).HasMaxLength(20);
            builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
            builder.Property(e => e.VehicleType).IsRequired().HasConversion<int>();
            builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(e => e.UserId).IsUnique().HasDatabaseName("Couriers_UserId");
            builder.HasIndex(e => e.Email).IsUnique().HasDatabaseName("Couriers_Email");

            builder.ToTable("Couriers");
        }
    }
}
