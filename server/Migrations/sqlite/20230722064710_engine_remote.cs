using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations.sqlite
{
    public partial class engine_remote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Ruleset",
                table: "GameEngines",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Contents",
                table: "GameEngines",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AddColumn<bool>(
                name: "Remote",
                table: "GameEngines",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RemoteApi",
                table: "GameEngines",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemoteOptions",
                table: "GameEngines",
                type: "TEXT",
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
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Contents",
                table: "GameEngines",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);
        }
    }
}
