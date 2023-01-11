using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace advisor.Migrations.sqlite
{
    public partial class changes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AditionalReports",
                table: "AditionalReports");

            migrationBuilder.DropColumn(
                name: "FactionName",
                table: "AditionalReports");

            migrationBuilder.RenameColumn(
                name: "Data",
                table: "Reports",
                newName: "Source");

            migrationBuilder.RenameColumn(
                name: "FactionNumber",
                table: "AditionalReports",
                newName: "Type");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Source",
                table: "AditionalReports",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Json",
                table: "AditionalReports",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "AditionalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<string>(
                name: "Error",
                table: "AditionalReports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AditionalReports",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AditionalReports",
                table: "AditionalReports",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AditionalReports_PlayerId_TurnNumber",
                table: "AditionalReports",
                columns: new[] { "PlayerId", "TurnNumber" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AditionalReports",
                table: "AditionalReports");

            migrationBuilder.DropIndex(
                name: "IX_AditionalReports_PlayerId_TurnNumber",
                table: "AditionalReports");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AditionalReports");

            migrationBuilder.DropColumn(
                name: "Error",
                table: "AditionalReports");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AditionalReports");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "Reports",
                newName: "Data");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "AditionalReports",
                newName: "FactionNumber");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "AditionalReports",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<string>(
                name: "Json",
                table: "AditionalReports",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FactionName",
                table: "AditionalReports",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AditionalReports",
                table: "AditionalReports",
                columns: new[] { "PlayerId", "TurnNumber", "FactionNumber" });
        }
    }
}
