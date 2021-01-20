using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CloudCopy.Server.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminOptions",
                columns: table => new
                {
                    AdminOptionsEntityId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SiteUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Inserted = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminOptions", x => x.AdminOptionsEntityId);
                    table.CheckConstraint("CK_AdminOptions_AdminOptionsEntityId", "AdminOptionsEntityId <= 1 AND AdminOptionsEntityId != 0");
                });

            migrationBuilder.CreateTable(
                name: "App",
                columns: table => new
                {
                    AppEntityId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JwtSecret = table.Column<string>(type: "TEXT", nullable: false),
                    PinCode = table.Column<string>(type: "TEXT", nullable: true),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Inserted = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_App", x => x.AppEntityId);
                    table.CheckConstraint("CK_AppEntity_AppEntityId", "AppEntityId <= 1 AND AppEntityId != 0");
                });

            migrationBuilder.CreateTable(
                name: "Copies",
                columns: table => new
                {
                    CopiedEntityId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    MimeType = table.Column<string>(type: "TEXT", nullable: false),
                    DayCreated = table.Column<int>(type: "INTEGER", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Inserted = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Copies", x => x.CopiedEntityId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Copies_DayCreated",
                table: "Copies",
                column: "DayCreated");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminOptions");

            migrationBuilder.DropTable(
                name: "App");

            migrationBuilder.DropTable(
                name: "Copies");
        }
    }
}
