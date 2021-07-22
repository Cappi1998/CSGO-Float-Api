using Microsoft.EntityFrameworkCore.Migrations;

namespace CSGO_Float_Api.Migrations
{
    public partial class createdb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FloatRequests",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloatRequests", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SteamAccounts",
                columns: table => new
                {
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    Shared_secret = table.Column<string>(type: "TEXT", nullable: true),
                    LoginKey = table.Column<string>(type: "TEXT", nullable: true),
                    SentryFileBase64 = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamAccounts", x => x.Username);
                });

            migrationBuilder.CreateTable(
                name: "Skins",
                columns: table => new
                {
                    param_a = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    param_s = table.Column<ulong>(type: "INTEGER", nullable: false),
                    param_m = table.Column<ulong>(type: "INTEGER", nullable: false),
                    param_d = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Float = table.Column<float>(type: "REAL", nullable: false),
                    FloatRequestID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skins", x => x.param_a);
                    table.ForeignKey(
                        name: "FK_Skins_FloatRequests_FloatRequestID",
                        column: x => x.FloatRequestID,
                        principalTable: "FloatRequests",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Skins_FloatRequestID",
                table: "Skins",
                column: "FloatRequestID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Skins");

            migrationBuilder.DropTable(
                name: "SteamAccounts");

            migrationBuilder.DropTable(
                name: "FloatRequests");
        }
    }
}
