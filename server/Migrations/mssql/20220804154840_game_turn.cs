using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.mssql
{
    public partial class game_turn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "PlayerData",
                table: "GameTurns",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "GameData",
                table: "GameTurns",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PlayerId",
                table: "Articles",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GameReports",
                columns: table => new
                {
                    GameId = table.Column<long>(type: "bigint", nullable: false),
                    TurnNumber = table.Column<int>(type: "int", nullable: false),
                    FactionNumber = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameReports", x => new { x.GameId, x.TurnNumber, x.FactionNumber });
                    table.ForeignKey(
                        name: "FK_GameReports_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameReports_GameTurns_GameId_TurnNumber",
                        columns: x => new { x.GameId, x.TurnNumber },
                        principalTable: "GameTurns",
                        principalColumns: new[] { "GameId", "Number" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameReports");

            migrationBuilder.DropColumn(
                name: "PlayerId",
                table: "Articles");

            migrationBuilder.AlterColumn<byte[]>(
                name: "PlayerData",
                table: "GameTurns",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "GameData",
                table: "GameTurns",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");
        }
    }
}
