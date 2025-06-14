using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ArslanProjectManager.Core.Models;

namespace ArslanProjectManager.Repository.Configurations;

public class BoardTagConfiguration : IEntityTypeConfiguration<BoardTag>
{
    public void Configure(EntityTypeBuilder<BoardTag> builder)
    {
        builder.ToTable("board_tag");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("board_id");

        builder.Property(e => e.BoardName)
            .HasColumnName("board_name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.BoardOrder)
            .HasColumnName("board_order")
            .IsRequired();

        builder.Property(e => e.CreatedDate)
           .HasColumnName("created_date")
           .HasColumnType("date")
           .HasDefaultValueSql("(getdate())");

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("updated_date")
            .HasColumnType("date");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.HasMany(e => e.ProjectTasks)
                .WithOne(e => e.Board)
                .HasForeignKey(e => e.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => e.IsActive);
    }
} 