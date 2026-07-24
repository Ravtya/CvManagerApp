using CvManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManager.Infrastructure.Data.Configurations;

public class PositionTagConfiguration : IEntityTypeConfiguration<PositionTag>
{
    public void Configure(EntityTypeBuilder<PositionTag> builder)
    {
        builder.ToTable("PositionTags");

        builder.HasKey(x => new { x.PositionId, x.TagId });

        builder.HasOne(x => x.Tag)
            .WithMany(x => x.PositionTags)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
