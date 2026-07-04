using Example.Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Example.UserService.API.Entities.Configure
{
    public class ExamPeriodConfiguration : IEntityTypeConfiguration<ExamPeriod>
    {
        public void Configure(EntityTypeBuilder<ExamPeriod> builder)
        {
            builder.ToTable("ExamPeriods");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.StartAt)
                .IsRequired()
                .HasConversion(
                    v => VietnamTimeHelper.ToStorageDateTime(v),
                    v => VietnamTimeHelper.FromStorage(v));

            builder.Property(x => x.EndAt)
                .IsRequired()
                .HasConversion(
                    v => VietnamTimeHelper.ToStorageDateTime(v),
                    v => VietnamTimeHelper.FromStorage(v));

            builder.Property(x => x.Status)
                .IsRequired();
        }
    }
}
