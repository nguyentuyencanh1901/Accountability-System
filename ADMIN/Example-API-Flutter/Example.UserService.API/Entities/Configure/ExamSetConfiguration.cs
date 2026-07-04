using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Example.UserService.API.Entities.Configure
{
    public class ExamSetConfiguration : IEntityTypeConfiguration<ExamSet>
    {
        public void Configure(EntityTypeBuilder<ExamSet> builder)
        {
            builder.ToTable("ExamSets");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.RequiredTotalPoints)
                .IsRequired();

            builder.Property(x => x.QuestionCount)
                .IsRequired();

            builder.Property(x => x.EasyCount)
                .IsRequired();

            builder.Property(x => x.MediumCount)
                .IsRequired();

            builder.Property(x => x.HardCount)
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.DurationMinutes)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();
        }
    }
}
