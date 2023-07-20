using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations.sqlite
{
    public partial class audit_for_game_and_engine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Games",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAt",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "Games",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "GameEngines",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedAt",
                table: "GameEngines",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByUserId",
                table: "GameEngines",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_CreatedByUserId",
                table: "Games",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_UpdatedByUserId",
                table: "Games",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameEngines_CreatedByUserId",
                table: "GameEngines",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameEngines_UpdatedByUserId",
                table: "GameEngines",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameEngines_Users_CreatedByUserId",
                table: "GameEngines",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GameEngines_Users_UpdatedByUserId",
                table: "GameEngines",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Users_CreatedByUserId",
                table: "Games",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Users_UpdatedByUserId",
                table: "Games",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameEngines_Users_CreatedByUserId",
                table: "GameEngines");

            migrationBuilder.DropForeignKey(
                name: "FK_GameEngines_Users_UpdatedByUserId",
                table: "GameEngines");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Users_CreatedByUserId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Users_UpdatedByUserId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_CreatedByUserId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_UpdatedByUserId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_GameEngines_CreatedByUserId",
                table: "GameEngines");

            migrationBuilder.DropIndex(
                name: "IX_GameEngines_UpdatedByUserId",
                table: "GameEngines");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "GameEngines");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "GameEngines");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "GameEngines");
        }
    }
}
