using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Example.Common.Entities;

namespace Example.Common.Entities.Configure
{
    public class NotificationUserConfiguration : IEntityTypeConfiguration<NotificationUser>
    {
        public void Configure(EntityTypeBuilder<NotificationUser> builder)
        {
            builder.Property(p => p.UserId).HasDefaultValue(0);
            builder.Property(p => p.NotificationId).HasDefaultValue(0);
            builder.Property(p => p.MarkRead).HasDefaultValue(false);
            builder.Property(p => p.MarkSeen).HasDefaultValue(false);

            builder.HasIndex(p => p.UserId);
            builder.HasIndex(p => p.NotificationId);
        }
    }
}
