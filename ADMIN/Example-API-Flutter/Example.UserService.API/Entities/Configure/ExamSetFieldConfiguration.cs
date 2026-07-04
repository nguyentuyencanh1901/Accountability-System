using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Example.UserService.API.Entities;

namespace Example.UserService.API.Entities.Configure
{
    public class ExamSetFieldConfiguration : IEntityTypeConfiguration<ExamSetField>
    {
        public void Configure(EntityTypeBuilder<ExamSetField> builder)
        {
            builder.ToTable("ExamSetFields");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.ExamSetId, x.FieldId }).IsUnique();

            builder.HasOne<ExamSet>()
                .WithMany()
                .HasForeignKey(x => x.ExamSetId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Field>()
                .WithMany()
                .HasForeignKey(x => x.FieldId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
