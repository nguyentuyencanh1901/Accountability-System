using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Example.UserService.API.Entities.Configure
{
    public class ExamSessionAnswerOptionConfiguration : IEntityTypeConfiguration<ExamSessionAnswerOption>
    {
        public void Configure(EntityTypeBuilder<ExamSessionAnswerOption> builder)
        {
            builder.ToTable("ExamSessionAnswerOptions");

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.ExamSessionAnswer)
                .WithMany()
                .HasForeignKey(x => x.ExamSessionAnswerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.AnswerOption)
                .WithMany()
                .HasForeignKey(x => x.AnswerOptionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
