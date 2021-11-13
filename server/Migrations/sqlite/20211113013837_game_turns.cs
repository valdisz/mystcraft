using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.sqlite
{
    public partial class game_turns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ready",
                table: "Turns",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GameTurns",
                columns: table => new
                {
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    GameId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlayerData = table.Column<byte[]>(type: "BLOB", nullable: true),
                    GameData = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameTurns", x => new { x.GameId, x.Number });
                    table.ForeignKey(
                        name: "FK_GameTurns_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TurnNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    GameId = table.Column<long>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Articles_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Articles_GameTurns_GameId_TurnNumber",
                        columns: x => new { x.GameId, x.TurnNumber },
                        principalTable: "GameTurns",
                        principalColumns: new[] { "GameId", "Number" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_GameId_TurnNumber",
                table: "Articles",
                columns: new[] { "GameId", "TurnNumber" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "GameTurns");

            migrationBuilder.DropColumn(
                name: "Ready",
                table: "Turns");
        }
    }
}
