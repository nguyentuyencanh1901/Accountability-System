using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Example.UserService.API.Entities.Configure
{
    public class ExamSessionAnswerConfiguration : IEntityTypeConfiguration<ExamSessionAnswer>
    {
        public void Configure(EntityTypeBuilder<ExamSessionAnswer> builder)
        {
            builder.ToTable("ExamSessionAnswers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.IsCorrect)
                .IsRequired();

            builder.Property(x => x.Score)
                .HasPrecision(10, 2);

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
