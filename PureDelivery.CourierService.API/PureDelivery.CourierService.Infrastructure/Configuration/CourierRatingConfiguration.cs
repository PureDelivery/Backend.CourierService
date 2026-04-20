using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PureDelivery.CourierService.Core.Models;

namespace PureDelivery.CourierService.Infrastructure.Configuration
{
    public class CourierRatingConfiguration : IEntityTypeConfiguration<CourierRating>
    {
        public void Configure(EntityTypeBuilder<CourierRating> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.CourierId).IsRequired();
            builder.Property(e => e.OrderId).IsRequired();
            builder.Property(e => e.RatedByCustomerId).IsRequired();
            builder.Property(e => e.Score).IsRequired();
            builder.Property(e => e.Comment).HasMaxLength(500);
            builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

            // One order → one rating per courier from a customer
            builder.HasIndex(e => new { e.CourierId, e.OrderId, e.RatedByCustomerId })
                   .IsUnique()
                   .HasDatabaseName("CourierRatings_Unique");

            builder.HasIndex(e => e.CourierId).HasDatabaseName("CourierRatings_CourierId");

            builder.ToTable("CourierRatings");
        }
    }
}
