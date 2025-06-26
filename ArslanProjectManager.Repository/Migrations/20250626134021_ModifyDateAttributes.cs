using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArslanProjectManager.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ModifyDateAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "token",
                newName: "updated_date");

            migrationBuilder.RenameColumn(
                name: "invite_date",
                table: "team_user",
                newName: "created_date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "register_date",
                table: "user",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(GETUTCDATE())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "token",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(GETUTCDATE())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "token",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "team_user",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "team_user",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(GETUTCDATE())",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "invite_date",
                table: "team_invite",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(GETUTCDATE())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "team",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "team",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(GETUTCDATE())",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "task_tag",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "task_tag",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(GETUTCDATE())",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "task_log",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(GETUTCDATE())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "task_comment",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(GETUTCDATE())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "task_category",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "task_category",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(GETUTCDATE())",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "role",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "role",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(GETUTCDATE())",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "start_date",
                table: "project_task",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "expected_end_date",
                table: "project_task",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "project_task",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(GETUTCDATE())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "project",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "start_date",
                table: "project",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "project",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(GETUTCDATE())",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "log_category",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "log_category",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(GETUTCDATE())",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "board_tag",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "board_tag",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(GETUTCDATE())",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldDefaultValueSql: "(getdate())");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updated_date",
                table: "token",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "created_date",
                table: "team_user",
                newName: "invite_date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "register_date",
                table: "user",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(GETUTCDATE())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "token",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(GETUTCDATE())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                table: "token",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "team_user",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "invite_date",
                table: "team_user",
                type: "date",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(GETUTCDATE())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "invite_date",
                table: "team_invite",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(GETUTCDATE())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "team",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "team",
                type: "date",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(GETUTCDATE())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "task_tag",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "task_tag",
                type: "date",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(GETUTCDATE())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "task_log",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(GETUTCDATE())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "task_comment",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(GETUTCDATE())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "task_category",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "task_category",
                type: "date",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(GETUTCDATE())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "role",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "role",
                type: "date",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(GETUTCDATE())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "start_date",
                table: "project_task",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "expected_end_date",
                table: "project_task",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "project_task",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(GETUTCDATE())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "project",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "start_date",
                table: "project",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "project",
                type: "date",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(GETUTCDATE())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "log_category",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "log_category",
                type: "date",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(GETUTCDATE())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_date",
                table: "board_tag",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "board_tag",
                type: "date",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(GETUTCDATE())");
        }
    }
}
