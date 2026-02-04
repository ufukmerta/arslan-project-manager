using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArslanProjectManager.Repository.Migrations
{
    /// <inheritdoc />
    public partial class FilterRoleUniqueIndexesByIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_role_role_name",
                table: "role");

            migrationBuilder.DropIndex(
                name: "IX_role_role_name_team_id",
                table: "role");

            migrationBuilder.CreateIndex(
                name: "IX_role_role_name",
                table: "role",
                column: "role_name",
                unique: true,
                filter: "[team_id] IS NULL AND [is_active] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_role_role_name_team_id",
                table: "role",
                columns: new[] { "role_name", "team_id" },
                unique: true,
                filter: "[is_active] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_role_role_name",
                table: "role");

            migrationBuilder.DropIndex(
                name: "IX_role_role_name_team_id",
                table: "role");

            migrationBuilder.CreateIndex(
                name: "IX_role_role_name",
                table: "role",
                column: "role_name",
                unique: true,
                filter: "[team_id] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_role_role_name_team_id",
                table: "role",
                columns: new[] { "role_name", "team_id" },
                unique: true,
                filter: "[team_id] IS NOT NULL");
        }
    }
}
