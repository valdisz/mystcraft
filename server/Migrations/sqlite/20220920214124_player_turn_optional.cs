using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace advisor.Migrations.sqlite
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
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReadyAt",
                table: "PlayerTurns",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TimesSubmittedAt",
                table: "PlayerTurns",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LastTurnNumber",
                table: "Players",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");
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
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Ready",
                table: "PlayerTurns",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TimesSubmitted",
                table: "PlayerTurns",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "LastTurnNumber",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
