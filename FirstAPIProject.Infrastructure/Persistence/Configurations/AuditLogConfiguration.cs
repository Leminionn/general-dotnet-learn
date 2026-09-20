using FirstAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FirstAPIProject.Infrastructure.Persistence.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserEmail)
                .IsRequired(false)
                .HasMaxLength(255);

            builder.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Endpoint)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Method)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.IpAddress)
                .IsRequired(false)
                .HasMaxLength(50);

            builder.Property(x => x.StatusCode)
                .IsRequired();

            builder.Property(x => x.ExecutionDurationMs)
                .IsRequired();

            builder.Property(x => x.Details)
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => x.Action);
            builder.HasIndex(x => x.UserId);
        }
    }
}
