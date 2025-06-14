using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArslanProjectManager.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "board_tag",
                columns: table => new
                {
                    board_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    board_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    board_order = table.Column<byte>(type: "tinyint", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_date = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "(getdate())"),
                    updated_date = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_board_tag", x => x.board_id);
                });

            migrationBuilder.CreateTable(
                name: "log_category",
                columns: table => new
                {
                    log_category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    log_category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_date = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "(getdate())"),
                    updated_date = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_log_category", x => x.log_category_id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    view_permission = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    edit_permission = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_date = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "(getdate())"),
                    updated_date = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "task_category",
                columns: table => new
                {
                    task_category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_date = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "(getdate())"),
                    updated_date = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_category", x => x.task_category_id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    profile_picture_url = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, defaultValue: "profile.png"),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    register_date = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_date = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "team",
                columns: table => new
                {
                    team_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    team_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    manager_id = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_date = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "(getdate())"),
                    updated_date = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team", x => x.team_id);
                    table.ForeignKey(
                        name: "FK_team_user",
                        column: x => x.manager_id,
                        principalTable: "user",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "token",
                columns: table => new
                {
                    token_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    access_token = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    refresh_token = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    RefreshTokenExpiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_token", x => x.token_id);
                    table.ForeignKey(
                        name: "FK_token_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "project",
                columns: table => new
                {
                    project_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    project_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    team_id = table.Column<int>(type: "int", nullable: false),
                    project_detail = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_date = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "(getdate())"),
                    updated_date = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project", x => x.project_id);
                    table.ForeignKey(
                        name: "FK_project_team_team_id",
                        column: x => x.team_id,
                        principalTable: "team",
                        principalColumn: "team_id");
                });

            migrationBuilder.CreateTable(
                name: "team_invite",
                columns: table => new
                {
                    team_invite_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    team_id = table.Column<int>(type: "int", nullable: false),
                    invited_email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    invited_by_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    status_change_note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    invite_date = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    status_change_date = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_invite", x => x.team_invite_id);
                    table.ForeignKey(
                        name: "FK_team_invite_team",
                        column: x => x.team_id,
                        principalTable: "team",
                        principalColumn: "team_id");
                    table.ForeignKey(
                        name: "FK_team_invite_user_invited_by_id",
                        column: x => x.invited_by_id,
                        principalTable: "user",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "team_user",
                columns: table => new
                {
                    team_user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    team_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    invite_date = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "(getdate())"),
                    updated_date = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_user", x => x.team_user_id);
                    table.ForeignKey(
                        name: "FK_team_user_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "role_id");
                    table.ForeignKey(
                        name: "FK_team_user_team_team_id",
                        column: x => x.team_id,
                        principalTable: "team",
                        principalColumn: "team_id");
                    table.ForeignKey(
                        name: "FK_team_user_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "project_task",
                columns: table => new
                {
                    task_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    project_id = table.Column<int>(type: "int", nullable: false),
                    task_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    starting_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    expected_end_date = table.Column<DateTime>(type: "datetime", nullable: true),
                    task_category_id = table.Column<int>(type: "int", nullable: false),
                    board_id = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    appointee_id = table.Column<int>(type: "int", nullable: false),
                    appointer_id = table.Column<int>(type: "int", nullable: false),
                    priority = table.Column<int>(type: "int", nullable: true, defaultValue: 2),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_date = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_date = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_task", x => x.task_id);
                    table.ForeignKey(
                        name: "FK_project_task_team_user_appointee_id",
                        column: x => x.appointee_id,
                        principalTable: "team_user",
                        principalColumn: "team_user_id");
                    table.ForeignKey(
                        name: "FK_project_task_team_user_appointer_id",
                        column: x => x.appointer_id,
                        principalTable: "team_user",
                        principalColumn: "team_user_id");
                    table.ForeignKey(
                        name: "FK_task_board_tag",
                        column: x => x.board_id,
                        principalTable: "board_tag",
                        principalColumn: "board_id");
                    table.ForeignKey(
                        name: "FK_task_project",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "project_id");
                    table.ForeignKey(
                        name: "FK_task_task_category",
                        column: x => x.task_category_id,
                        principalTable: "task_category",
                        principalColumn: "task_category_id");
                });

            migrationBuilder.CreateTable(
                name: "task_comment",
                columns: table => new
                {
                    comment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    task_id = table.Column<int>(type: "int", nullable: false),
                    team_user_id = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_date = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_date = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_comment", x => x.comment_id);
                    table.ForeignKey(
                        name: "FK_task_comment_project_task_task_id",
                        column: x => x.task_id,
                        principalTable: "project_task",
                        principalColumn: "task_id");
                    table.ForeignKey(
                        name: "FK_task_comment_team_user_team_user_id",
                        column: x => x.team_user_id,
                        principalTable: "team_user",
                        principalColumn: "team_user_id");
                });

            migrationBuilder.CreateTable(
                name: "task_log",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    task_id = table.Column<int>(type: "int", nullable: false),
                    team_user_id = table.Column<int>(type: "int", nullable: false),
                    log_category_id = table.Column<int>(type: "int", nullable: false),
                    affected_team_user_id = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_date = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    updated_date = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_log", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_task_log_log_category_log_category_id",
                        column: x => x.log_category_id,
                        principalTable: "log_category",
                        principalColumn: "log_category_id");
                    table.ForeignKey(
                        name: "FK_task_log_project_task_task_id",
                        column: x => x.task_id,
                        principalTable: "project_task",
                        principalColumn: "task_id");
                    table.ForeignKey(
                        name: "FK_task_log_team_user_affected_team_user_id",
                        column: x => x.affected_team_user_id,
                        principalTable: "team_user",
                        principalColumn: "team_user_id");
                    table.ForeignKey(
                        name: "FK_task_log_team_user_team_user_id",
                        column: x => x.team_user_id,
                        principalTable: "team_user",
                        principalColumn: "team_user_id");
                });

            migrationBuilder.CreateTable(
                name: "task_tag",
                columns: table => new
                {
                    tag_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    task_id = table.Column<int>(type: "int", nullable: false),
                    tag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_date = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "(getdate())"),
                    updated_date = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_tag", x => x.tag_id);
                    table.ForeignKey(
                        name: "FK_task_tag_project_task_task_id",
                        column: x => x.task_id,
                        principalTable: "project_task",
                        principalColumn: "task_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_team_id",
                table: "project",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_task_appointee_id",
                table: "project_task",
                column: "appointee_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_task_appointer_id",
                table: "project_task",
                column: "appointer_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_task_board_id",
                table: "project_task",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_task_project_id",
                table: "project_task",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_task_task_category_id",
                table: "project_task",
                column: "task_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_role_name",
                table: "role",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_task_comment_task_id",
                table: "task_comment",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_comment_team_user_id",
                table: "task_comment",
                column: "team_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_log_affected_team_user_id",
                table: "task_log",
                column: "affected_team_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_log_log_category_id",
                table: "task_log",
                column: "log_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_log_task_id",
                table: "task_log",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_log_team_user_id",
                table: "task_log",
                column: "team_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_tag_task_id",
                table: "task_tag",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_manager_id",
                table: "team",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_invite_invited_by_id",
                table: "team_invite",
                column: "invited_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_invite_team_id",
                table: "team_invite",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_user_role_id",
                table: "team_user",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_user_team_id",
                table: "team_user",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_user_user_id",
                table: "team_user",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_token_user_id",
                table: "token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_email",
                table: "user",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "task_comment");

            migrationBuilder.DropTable(
                name: "task_log");

            migrationBuilder.DropTable(
                name: "task_tag");

            migrationBuilder.DropTable(
                name: "team_invite");

            migrationBuilder.DropTable(
                name: "token");

            migrationBuilder.DropTable(
                name: "log_category");

            migrationBuilder.DropTable(
                name: "project_task");

            migrationBuilder.DropTable(
                name: "team_user");

            migrationBuilder.DropTable(
                name: "board_tag");

            migrationBuilder.DropTable(
                name: "project");

            migrationBuilder.DropTable(
                name: "task_category");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "team");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
