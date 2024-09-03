using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Insurance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "archived_files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    file_path = table.Column<string>(type: "longtext", nullable: false),
                    file_directory = table.Column<string>(type: "longtext", nullable: false),
                    filename = table.Column<string>(type: "longtext", nullable: false),
                    is_done = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    file_not_found = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_archived_files", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "crawled_files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    file_path = table.Column<string>(type: "longtext", nullable: false),
                    file_directory = table.Column<string>(type: "longtext", nullable: false),
                    filename = table.Column<string>(type: "longtext", nullable: false),
                    mime_type = table.Column<string>(type: "longtext", nullable: true),
                    is_microsoft = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    creation_datetime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified_datetime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    origin_creation_datetime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    origin_last_modified_datetime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crawled_files", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "archived_files");

            migrationBuilder.DropTable(
                name: "crawled_files");
        }
    }
}
