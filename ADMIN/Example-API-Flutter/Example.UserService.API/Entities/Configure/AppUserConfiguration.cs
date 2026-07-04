using Elastic.Apm.Api;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Example.UserService.API.Entities.Configure
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => x.Username).IsUnique();

            builder.Property(x => x.Email)
                .HasMaxLength(150);

            builder.HasIndex(x => x.Email).IsUnique();

            builder.Property(x => x.Phone)
                .HasMaxLength(20);

            builder.HasIndex(x => x.Phone).IsUnique();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.UserType)
                .IsRequired()
                .HasDefaultValue(2);
        }
    }
}

