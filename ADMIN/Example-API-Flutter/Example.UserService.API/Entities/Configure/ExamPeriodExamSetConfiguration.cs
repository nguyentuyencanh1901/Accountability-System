using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Example.UserService.API.Entities.Configure
{
    public class ExamPeriodExamSetConfiguration : IEntityTypeConfiguration<ExamPeriodExamSet>
    {
        public void Configure(EntityTypeBuilder<ExamPeriodExamSet> builder)
        {
            builder.ToTable("ExamPeriodExamSets");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.ExamPeriodId, x.ExamSetId })
                .IsUnique();

            builder.HasOne(x => x.ExamPeriod)
                .WithMany()
                .HasForeignKey(x => x.ExamPeriodId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ExamSet)
                .WithMany()
                .HasForeignKey(x => x.ExamSetId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
