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

            builder.Property(e => e.CurrentLatitude);
            builder.Property(e => e.CurrentLongitude);
            builder.Property(e => e.IsOnline).IsRequired().HasDefaultValue(false);
            builder.Property(e => e.IsAvailable).IsRequired().HasDefaultValue(false);
            builder.Property(e => e.LastLocationUpdated);

            builder.Property(e => e.RatingSum).IsRequired().HasDefaultValue(0.0);
            builder.Property(e => e.RatingCount).IsRequired().HasDefaultValue(0);
            builder.Ignore(e => e.AverageRating); // computed, not stored

            builder.Property(e => e.TotalDeliveries).IsRequired().HasDefaultValue(0);

            builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(e => e.UserId).IsUnique().HasDatabaseName("Couriers_UserId");
            builder.HasIndex(e => e.Email).IsUnique().HasDatabaseName("Couriers_Email");
            builder.HasIndex(e => new { e.IsOnline, e.IsAvailable, e.IsActive })
                   .HasDatabaseName("Couriers_Online_Available");

            builder.HasIndex(e => new { e.CurrentLatitude, e.CurrentLongitude })
                .HasFilter("[IsOnline] = 1 AND [IsAvailable] = 1 AND [IsActive] = 1");

            builder.HasMany(e => e.Ratings)
                   .WithOne(r => r.Courier)
                   .HasForeignKey(r => r.CourierId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("Couriers");
        }
    }
}
