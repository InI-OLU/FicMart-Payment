using FicMart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FicMart.Infrastructure.Persistence.Configuration
{
    public class IdempotencyKeyConfiguration:IEntityTypeConfiguration<IdempotencyKey>
    {
        public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.CustomerId)
                    .HasMaxLength(100);
            builder.Property(o => o.Idempotencykey)
                    .HasMaxLength(100);
            builder.Property(o => o.RequestMethod)
                  .HasMaxLength(10);
            builder.Property(o => o.RequestPath)
                  .HasMaxLength(100);
            builder.Property(o => o.RecoveryPoint)
                  .HasMaxLength(50);
            builder.Property(o => o.RecoveryPoint)
                 .HasConversion<string>();
            builder.HasIndex(o => new { o.CustomerId, o.Idempotencykey })
                    .IsUnique();
        }
    }
}
