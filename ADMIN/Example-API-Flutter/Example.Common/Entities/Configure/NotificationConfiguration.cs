using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Example.Common.Entities;

namespace Example.Common.Entities.Configure
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.Property(p => p.DeviceId).HasDefaultValue(0);
            builder.Property(p => p.CameraId).HasDefaultValue(0);
            builder.Property(p => p.PackageNameType).HasDefaultValue(0);
            builder.Property(p => p.UserViolate).HasDefaultValue(0);
            builder.Property(p => p.MediaType).HasDefaultValue(0);
            builder.Property(p => p.BucketMedia).IsRequired(false);
            builder.Property(p => p.ObjectMedia).IsRequired(false);
            builder.Property(p => p.Title).HasMaxLength(100).IsRequired();
            builder.Property(p => p.Description).HasMaxLength(500).IsRequired(false);
            builder.Property(p => p.Link).IsRequired(false);
            builder.Property(p => p.EvidenceKey).HasMaxLength(100);
        }
    }
}
