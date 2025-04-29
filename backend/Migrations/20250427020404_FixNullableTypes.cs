using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    public partial class FixNullableTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "achievement",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    required_points = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__achievem__3213E83F121C4DCE", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__category__3213E83F68DBA026", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "oauth_provider",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__oauth_pr__3213E83FF39B442B", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__role__3213E83FAB0BD8F0", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    avatar_url = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    role_id = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    points = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    status = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true, defaultValue: "active"),
                    registration_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user__3213E83FAD2E384B", x => x.id);
                    table.ForeignKey(
                        name: "FK__user__role_id__2B3F6F97",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "minigame_drawing",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    json_data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__minigame__3213E83FEBE879CF", x => x.id);
                    table.ForeignKey(
                        name: "FK__minigame___user___45F365D3",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    read = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__notifica__3213E83F2A5B8F21", x => x.id);
                    table.ForeignKey(
                        name: "FK__notificat__user___4AB81AF0",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "oauth_user",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    provider_id = table.Column<int>(type: "int", nullable: false),
                    provider_user_id = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__oauth_us__3213E83FEC4EEB04", x => x.id);
                    table.ForeignKey(
                        name: "FK__oauth_use__provi__4D94879B",
                        column: x => x.provider_id,
                        principalTable: "oauth_provider",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__oauth_use__user___4E88ABD4",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tutorial",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    Difficulty = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    estimated_duration = table.Column<int>(type: "int", nullable: true),
                    author_id = table.Column<int>(type: "int", nullable: false),
                    publication_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true, defaultValue: "pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tutorial__3213E83F05648820", x => x.id);
                    table.ForeignKey(
                        name: "FK__tutorial__author__37A5467C",
                        column: x => x.author_id,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_achievement",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    achievement_id = table.Column<int>(type: "int", nullable: false),
                    obtained_date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user_ach__9A7AA5E75354052F", x => new { x.user_id, x.achievement_id });
                    table.ForeignKey(
                        name: "FK__user_achi__achie__5EBF139D",
                        column: x => x.achievement_id,
                        principalTable: "achievement",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__user_achi__user___5DCAEF64",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tutorial_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    action = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__audit__3213E83FACDD0C31", x => x.id);
                    table.ForeignKey(
                        name: "FK__audit__tutorial___3B75D760",
                        column: x => x.tutorial_id,
                        principalTable: "tutorial",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__audit__user_id__3C69FB99",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "CategoryTutorial",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "int", nullable: false),
                    TutorialsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryTutorial", x => new { x.CategoriesId, x.TutorialsId });
                    table.ForeignKey(
                        name: "FK_CategoryTutorial_category_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryTutorial_tutorial_TutorialsId",
                        column: x => x.TutorialsId,
                        principalTable: "tutorial",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comment",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tutorial_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    edited = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__comment__3213E83F9DE3DAEF", x => x.id);
                    table.ForeignKey(
                        name: "FK__comment__tutoria__412EB0B6",
                        column: x => x.tutorial_id,
                        principalTable: "tutorial",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__comment__user_id__4222D4EF",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "rating",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tutorial_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    score = table.Column<int>(type: "int", nullable: true),
                    date = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__rating__3213E83F29C8967F", x => x.id);
                    table.ForeignKey(
                        name: "FK__rating__tutorial__52593CB8",
                        column: x => x.tutorial_id,
                        principalTable: "tutorial",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__rating__user_id__534D60F1",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tutorial_category",
                columns: table => new
                {
                    tutorial_id = table.Column<int>(type: "int", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tutorial_category", x => new { x.tutorial_id, x.category_id });
                    table.ForeignKey(
                        name: "FK_tutorial_category_category",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tutorial_category_tutorial",
                        column: x => x.tutorial_id,
                        principalTable: "tutorial",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tutorial_content",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tutorial_id = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    order = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tutorial__3213E83F01AF71A0", x => x.id);
                    table.ForeignKey(
                        name: "FK__tutorial___tutor__59FA5E80",
                        column: x => x.tutorial_id,
                        principalTable: "tutorial",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_tutorial_id",
                table: "audit",
                column: "tutorial_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_user_id",
                table: "audit",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ__category__72E12F1BDF677F65",
                table: "category",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryTutorial_TutorialsId",
                table: "CategoryTutorial",
                column: "TutorialsId");

            migrationBuilder.CreateIndex(
                name: "IX_comment_tutorial_id",
                table: "comment",
                column: "tutorial_id");

            migrationBuilder.CreateIndex(
                name: "IX_comment_user_id",
                table: "comment",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_minigame_drawing_user_id",
                table: "minigame_drawing",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_user_id",
                table: "notification",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ__oauth_pr__72E12F1B0D9ADE5A",
                table: "oauth_provider",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_oauth_user_provider_id",
                table: "oauth_user",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_oauth_user_user_id",
                table: "oauth_user",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_rating_tutorial_id",
                table: "rating",
                column: "tutorial_id");

            migrationBuilder.CreateIndex(
                name: "IX_rating_user_id",
                table: "rating",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tutorial_author_id",
                table: "tutorial",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_tutorial_category_category_id",
                table: "tutorial_category",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_tutorial_content_tutorial_id",
                table: "tutorial_content",
                column: "tutorial_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_role_id",
                table: "user",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "UQ__user__AB6E6164BAB59BC2",
                table: "user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_achievement_achievement_id",
                table: "user_achievement",
                column: "achievement_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit");

            migrationBuilder.DropTable(
                name: "CategoryTutorial");

            migrationBuilder.DropTable(
                name: "comment");

            migrationBuilder.DropTable(
                name: "minigame_drawing");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "oauth_user");

            migrationBuilder.DropTable(
                name: "rating");

            migrationBuilder.DropTable(
                name: "tutorial_category");

            migrationBuilder.DropTable(
                name: "tutorial_content");

            migrationBuilder.DropTable(
                name: "user_achievement");

            migrationBuilder.DropTable(
                name: "oauth_provider");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "tutorial");

            migrationBuilder.DropTable(
                name: "achievement");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "role");
        }
    }
}
