using FirstAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FirstAPIProject.Infrastructure.Persistence.Configurations
{
    public class EmailWhitelistConfiguration : IEntityTypeConfiguration<EmailWhitelist>
    {
        public void Configure(EntityTypeBuilder<EmailWhitelist> builder)
        {
            builder.ToTable("EmailWhitelists");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Pattern)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Description)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.IsDeleted)
                .IsRequired();

            builder.Property(x => x.DeletedAt)
                .IsRequired(false);

            // Index pattern for fast lookup
            builder.HasIndex(x => x.Pattern)
                .IsUnique();
        }
    }
}
