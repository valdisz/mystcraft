using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations.mssql
{
    public partial class engine_remote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Ruleset",
                table: "GameEngines",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Contents",
                table: "GameEngines",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

            migrationBuilder.AddColumn<bool>(
                name: "Remote",
                table: "GameEngines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RemoteApi",
                table: "GameEngines",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemoteOptions",
                table: "GameEngines",
                type: "nvarchar(max)",
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
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Contents",
                table: "GameEngines",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);
        }
    }
}
