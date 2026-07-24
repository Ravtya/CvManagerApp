using CvManager.Domain;
using CvManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManager.Infrastructure.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(FieldLengths.Name);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(FieldLengths.Text);

        builder.HasGeneratedTsVectorColumn(
                p => p.SearchVector,
                "simple",
                p => new { p.Name, p.Description })
            .HasIndex(p => p.SearchVector)
            .HasMethod("GIN");

        builder.HasMany(x => x.Tags)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
