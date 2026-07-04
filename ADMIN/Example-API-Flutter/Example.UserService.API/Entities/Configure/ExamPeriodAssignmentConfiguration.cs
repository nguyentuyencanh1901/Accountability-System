using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Example.UserService.API.Entities.Configure
{
    public class ExamPeriodAssignmentConfiguration : IEntityTypeConfiguration<ExamPeriodAssignment>
    {
        public void Configure(EntityTypeBuilder<ExamPeriodAssignment> builder)
        {
            builder.ToTable("ExamPeriodAssignments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ExamType)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.HasIndex(x => new { x.ExamPeriodId, x.UserId, x.ExamType })
                .IsUnique();

            builder.HasOne(x => x.ExamPeriod)
                .WithMany()
                .HasForeignKey(x => x.ExamPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ExamSet)
                .WithMany()
                .HasForeignKey(x => x.ExamSetId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ExamSession)
                .WithMany()
                .HasForeignKey(x => x.ExamSessionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
