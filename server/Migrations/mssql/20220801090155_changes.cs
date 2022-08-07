using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.mssql
{
    public partial class changes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EngineVersion",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "RulesetName",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "RulesetVersion",
                table: "Games");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Games",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "LastTurnNumber",
                table: "Games",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextTurnNumber",
                table: "Games",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_Id_LastTurnNumber",
                table: "Games",
                columns: new[] { "Id", "LastTurnNumber" },
                unique: true,
                filter: "[LastTurnNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Games_Id_NextTurnNumber",
                table: "Games",
                columns: new[] { "Id", "NextTurnNumber" },
                unique: true,
                filter: "[NextTurnNumber] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_GameTurns_Id_LastTurnNumber",
                table: "Games",
                columns: new[] { "Id", "LastTurnNumber" },
                principalTable: "GameTurns",
                principalColumns: new[] { "GameId", "Number" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_GameTurns_Id_NextTurnNumber",
                table: "Games",
                columns: new[] { "Id", "NextTurnNumber" },
                principalTable: "GameTurns",
                principalColumns: new[] { "GameId", "Number" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_GameTurns_Id_LastTurnNumber",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_GameTurns_Id_NextTurnNumber",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_Id_LastTurnNumber",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_Id_NextTurnNumber",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "LastTurnNumber",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "NextTurnNumber",
                table: "Games");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Games",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "EngineVersion",
                table: "Games",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RulesetName",
                table: "Games",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RulesetVersion",
                table: "Games",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");
        }
    }
}
