using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Example.UserService.API.Entities.Configure
{
    public class ExamSessionConfiguration : IEntityTypeConfiguration<ExamSession>
    {
        public void Configure(EntityTypeBuilder<ExamSession> builder)
        {
            builder.ToTable("ExamSessions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ExamType)
                .IsRequired();

            builder.Property(x => x.StartedAt)
                .IsRequired();

            builder.Property(x => x.TotalScore)
                .HasPrecision(10, 2);

            builder.Property(x => x.MaxScore)
                .HasPrecision(10, 2);

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.ViolationCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.HasOne(x => x.ExamSet)
                .WithMany()
                .HasForeignKey(x => x.ExamSetId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ExamPeriodAssignment)
                .WithMany()
                .HasForeignKey(x => x.ExamPeriodAssignmentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
