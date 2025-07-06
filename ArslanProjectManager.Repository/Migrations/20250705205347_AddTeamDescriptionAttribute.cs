using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArslanProjectManager.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamDescriptionAttribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "team",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "priority",
                table: "project_task",
                type: "int",
                nullable: false,
                defaultValue: 2,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "team");

            migrationBuilder.AlterColumn<int>(
                name: "priority",
                table: "project_task",
                type: "int",
                nullable: true,
                defaultValue: 2,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 2);
        }
    }
}
