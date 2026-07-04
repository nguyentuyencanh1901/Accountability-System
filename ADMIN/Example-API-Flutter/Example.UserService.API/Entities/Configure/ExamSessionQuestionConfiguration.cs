using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Example.UserService.API.Entities.Configure
{
    public class ExamSessionQuestionConfiguration : IEntityTypeConfiguration<ExamSessionQuestion>
    {
        public void Configure(EntityTypeBuilder<ExamSessionQuestion> builder)
        {
            builder.ToTable("ExamSessionQuestions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Points)
                .IsRequired();

            builder.Property(x => x.SortOrder)
                .IsRequired();

            builder.HasOne(x => x.ExamSession)
                .WithMany()
                .HasForeignKey(x => x.ExamSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Question)
                .WithMany()
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
