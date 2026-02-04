using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArslanProjectManager.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddManageRolesPermissionsOverride : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "can_manage_roles_override",
                table: "team_user",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "can_manage_permissions_override",
                table: "team_user",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "can_manage_roles_override",
                table: "team_user");

            migrationBuilder.DropColumn(
                name: "can_manage_permissions_override",
                table: "team_user");
        }
    }
}
