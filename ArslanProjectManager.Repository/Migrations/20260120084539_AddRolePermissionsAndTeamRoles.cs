using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArslanProjectManager.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddRolePermissionsAndTeamRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_role_role_name",
                table: "role");

            migrationBuilder.DropColumn(
                name: "edit_permission",
                table: "role");

            migrationBuilder.DropColumn(
                name: "view_permission",
                table: "role");

            migrationBuilder.AddColumn<bool>(
                name: "can_assign_tasks_override",
                table: "team_user",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "can_delete_projects_override",
                table: "team_user",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "can_delete_tasks_override",
                table: "team_user",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "can_edit_projects_override",
                table: "team_user",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "can_edit_tasks_override",
                table: "team_user",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "can_invite_members_override",
                table: "team_user",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "can_remove_members_override",
                table: "team_user",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "can_view_projects_override",
                table: "team_user",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "can_view_tasks_override",
                table: "team_user",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "can_assign_tasks",
                table: "role",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_delete_projects",
                table: "role",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_delete_tasks",
                table: "role",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_edit_projects",
                table: "role",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_edit_tasks",
                table: "role",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_invite_members",
                table: "role",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_manage_permissions",
                table: "role",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_manage_roles",
                table: "role",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_remove_members",
                table: "role",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_view_projects",
                table: "role",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_view_tasks",
                table: "role",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_system_role",
                table: "role",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "team_id",
                table: "role",
                type: "int",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_role_team_id",
                table: "role",
                column: "team_id");

            migrationBuilder.AddForeignKey(
                name: "FK_role_team_team_id",
                table: "role",
                column: "team_id",
                principalTable: "team",
                principalColumn: "team_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_role_team_team_id",
                table: "role");

            migrationBuilder.DropIndex(
                name: "IX_role_role_name",
                table: "role");

            migrationBuilder.DropIndex(
                name: "IX_role_role_name_team_id",
                table: "role");

            migrationBuilder.DropIndex(
                name: "IX_role_team_id",
                table: "role");

            migrationBuilder.DropColumn(
                name: "can_assign_tasks_override",
                table: "team_user");

            migrationBuilder.DropColumn(
                name: "can_delete_projects_override",
                table: "team_user");

            migrationBuilder.DropColumn(
                name: "can_delete_tasks_override",
                table: "team_user");

            migrationBuilder.DropColumn(
                name: "can_edit_projects_override",
                table: "team_user");

            migrationBuilder.DropColumn(
                name: "can_edit_tasks_override",
                table: "team_user");

            migrationBuilder.DropColumn(
                name: "can_invite_members_override",
                table: "team_user");

            migrationBuilder.DropColumn(
                name: "can_remove_members_override",
                table: "team_user");

            migrationBuilder.DropColumn(
                name: "can_view_projects_override",
                table: "team_user");

            migrationBuilder.DropColumn(
                name: "can_view_tasks_override",
                table: "team_user");

            migrationBuilder.DropColumn(
                name: "can_assign_tasks",
                table: "role");

            migrationBuilder.DropColumn(
                name: "can_delete_projects",
                table: "role");

            migrationBuilder.DropColumn(
                name: "can_delete_tasks",
                table: "role");

            migrationBuilder.DropColumn(
                name: "can_edit_projects",
                table: "role");

            migrationBuilder.DropColumn(
                name: "can_edit_tasks",
                table: "role");

            migrationBuilder.DropColumn(
                name: "can_invite_members",
                table: "role");

            migrationBuilder.DropColumn(
                name: "can_manage_permissions",
                table: "role");

            migrationBuilder.DropColumn(
                name: "can_manage_roles",
                table: "role");

            migrationBuilder.DropColumn(
                name: "can_remove_members",
                table: "role");

            migrationBuilder.DropColumn(
                name: "can_view_projects",
                table: "role");

            migrationBuilder.DropColumn(
                name: "can_view_tasks",
                table: "role");

            migrationBuilder.DropColumn(
                name: "is_system_role",
                table: "role");

            migrationBuilder.DropColumn(
                name: "team_id",
                table: "role");

            migrationBuilder.AddColumn<bool>(
                name: "edit_permission",
                table: "role",
                type: "bit",
                nullable: true,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "view_permission",
                table: "role",
                type: "bit",
                nullable: true,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_role_role_name",
                table: "role",
                column: "role_name",
                unique: true);
        }
    }
}
