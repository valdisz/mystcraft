using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations.pgsql
{
    public partial class engine_remote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Ruleset",
                table: "GameEngines",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Contents",
                table: "GameEngines",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea");

            migrationBuilder.AddColumn<bool>(
                name: "Remote",
                table: "GameEngines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RemoteApi",
                table: "GameEngines",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemoteOptions",
                table: "GameEngines",
                type: "text",
                maxLength: 2147483647,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Remote",
                table: "GameEngines");

            migrationBuilder.DropColumn(
                name: "RemoteApi",
                table: "GameEngines");

            migrationBuilder.DropColumn(
                name: "RemoteOptions",
                table: "GameEngines");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Ruleset",
                table: "GameEngines",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Contents",
                table: "GameEngines",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);
        }
    }
}
