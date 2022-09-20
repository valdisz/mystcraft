using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace advisor.Migrations.pgsql
{
    public partial class player_turn_optional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrdersSubmitted",
                table: "PlayerTurns");

            migrationBuilder.DropColumn(
                name: "Ready",
                table: "PlayerTurns");

            migrationBuilder.DropColumn(
                name: "TimesSubmitted",
                table: "PlayerTurns");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OrdersSubmittedAt",
                table: "PlayerTurns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReadyAt",
                table: "PlayerTurns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TimesSubmittedAt",
                table: "PlayerTurns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LastTurnNumber",
                table: "Players",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrdersSubmittedAt",
                table: "PlayerTurns");

            migrationBuilder.DropColumn(
                name: "ReadyAt",
                table: "PlayerTurns");

            migrationBuilder.DropColumn(
                name: "TimesSubmittedAt",
                table: "PlayerTurns");

            migrationBuilder.AddColumn<bool>(
                name: "OrdersSubmitted",
                table: "PlayerTurns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Ready",
                table: "PlayerTurns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TimesSubmitted",
                table: "PlayerTurns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "LastTurnNumber",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
