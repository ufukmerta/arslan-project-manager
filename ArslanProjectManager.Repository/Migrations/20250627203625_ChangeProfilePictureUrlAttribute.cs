using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArslanProjectManager.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ChangeProfilePictureUrlAttribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "profile_picture_url",
                table: "user");

            migrationBuilder.AddColumn<byte[]>(
                name: "profile_picture",
                table: "user",
                type: "varbinary(max)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "profile_picture",
                table: "user");

            migrationBuilder.AddColumn<string>(
                name: "profile_picture_url",
                table: "user",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                defaultValue: "profile.png");
        }
    }
}
