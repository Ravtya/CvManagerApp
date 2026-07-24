using CvManager.Domain;
using CvManager.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManager.Infrastructure.Data.Configurations;

public class DiscussionPostConfiguration : IEntityTypeConfiguration<DiscussionPost>
{
    public void Configure(EntityTypeBuilder<DiscussionPost> builder)
    {
        builder.ToTable("DiscussionPosts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AuthorUserId)
            .IsRequired()
            .HasMaxLength(FieldLengths.UserId);

        builder.Property(x => x.Content)
            .HasColumnName("ContentMarkdown")
            .IsRequired()
            .HasMaxLength(FieldLengths.Discussion);

        builder.HasIndex(x => new { x.PositionId, x.CreatedAt });

        builder.HasGeneratedTsVectorColumn(
                p => p.SearchVector,
                "simple",
                p => p.Content)
            .HasIndex(p => p.SearchVector)
            .HasMethod("GIN");

        builder.HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(x => x.AuthorUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
