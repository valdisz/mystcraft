using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.mssql
{
    public partial class changes3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Engine",
                table: "Games");

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

            migrationBuilder.AddColumn<bool>(
                name: "IsRemote",
                table: "GameTurns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "EngineId",
                table: "Games",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GameEngines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Contents = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEngines", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Games_EngineId",
                table: "Games",
                column: "EngineId");

            migrationBuilder.CreateIndex(
                name: "IX_GameEngines_Name",
                table: "GameEngines",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_GameEngines_EngineId",
                table: "Games",
                column: "EngineId",
                principalTable: "GameEngines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_GameEngines_EngineId",
                table: "Games");

            migrationBuilder.DropTable(
                name: "GameEngines");

            migrationBuilder.DropIndex(
                name: "IX_Games_EngineId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "IsRemote",
                table: "GameTurns");

            migrationBuilder.DropColumn(
                name: "EngineId",
                table: "Games");

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

            migrationBuilder.AddColumn<byte[]>(
                name: "Engine",
                table: "Games",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
