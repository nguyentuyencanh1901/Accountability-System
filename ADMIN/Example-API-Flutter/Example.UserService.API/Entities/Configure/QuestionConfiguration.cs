using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Example.UserService.API.Entities.Configure
{
    public class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.ToTable("Questions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(x => x.ImageUrl)
                .HasMaxLength(500);

            builder.Property(x => x.Points)
                .IsRequired();

            builder.Property(x => x.QuestionType)
                .IsRequired();

            builder.Property(x => x.DifficultyLevel)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.HasOne<Field>()
                .WithMany()
                .HasForeignKey(x => x.FieldId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.FieldId);
        }
    }
}
