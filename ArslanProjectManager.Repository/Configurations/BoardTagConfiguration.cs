using ArslanProjectManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArslanProjectManager.Repository.Configurations;

public class BoardTagConfiguration : BaseEntityConfiguration<BoardTag>
{
    protected override void ConfigureEntity(EntityTypeBuilder<BoardTag> builder)
    {
        builder.ToTable("board_tag");

        builder.Property(e => e.Id)
            .HasColumnName("board_id");

        builder.Property(e => e.BoardName)
            .HasColumnName("board_name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.BoardOrder)
            .HasColumnName("board_order")
            .IsRequired();

        builder.HasMany(e => e.ProjectTasks)
            .WithOne(e => e.Board)
            .HasForeignKey(e => e.BoardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
