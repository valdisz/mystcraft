using Microsoft.EntityFrameworkCore.Migrations;

namespace atlantis.Migrations
{
    public partial class relationships : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RegionId",
                table: "Units",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "StrcutureId",
                table: "Units",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RegionId",
                table: "Structures",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "X",
                table: "Regions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Y",
                table: "Regions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Z",
                table: "Regions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Units_GameId",
                table: "Units",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_RegionId",
                table: "Units",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_StrcutureId",
                table: "Units",
                column: "StrcutureId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_TurnId",
                table: "Units",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_GameId",
                table: "Turns",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Structures_GameId",
                table: "Structures",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Structures_RegionId",
                table: "Structures",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Structures_TurnId",
                table: "Structures",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_GameId",
                table: "Regions",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_TurnId",
                table: "Regions",
                column: "TurnId");

            migrationBuilder.AddForeignKey(
                name: "FK_Regions_Games_GameId",
                table: "Regions",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Regions_Turns_TurnId",
                table: "Regions",
                column: "TurnId",
                principalTable: "Turns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Structures_Games_GameId",
                table: "Structures",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Structures_Regions_RegionId",
                table: "Structures",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Structures_Turns_TurnId",
                table: "Structures",
                column: "TurnId",
                principalTable: "Turns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Turns_Games_GameId",
                table: "Turns",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Games_GameId",
                table: "Units",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Regions_RegionId",
                table: "Units",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Structures_StrcutureId",
                table: "Units",
                column: "StrcutureId",
                principalTable: "Structures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Units_Turns_TurnId",
                table: "Units",
                column: "TurnId",
                principalTable: "Turns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Regions_Games_GameId",
                table: "Regions");

            migrationBuilder.DropForeignKey(
                name: "FK_Regions_Turns_TurnId",
                table: "Regions");

            migrationBuilder.DropForeignKey(
                name: "FK_Structures_Games_GameId",
                table: "Structures");

            migrationBuilder.DropForeignKey(
                name: "FK_Structures_Regions_RegionId",
                table: "Structures");

            migrationBuilder.DropForeignKey(
                name: "FK_Structures_Turns_TurnId",
                table: "Structures");

            migrationBuilder.DropForeignKey(
                name: "FK_Turns_Games_GameId",
                table: "Turns");

            migrationBuilder.DropForeignKey(
                name: "FK_Units_Games_GameId",
                table: "Units");

            migrationBuilder.DropForeignKey(
                name: "FK_Units_Regions_RegionId",
                table: "Units");

            migrationBuilder.DropForeignKey(
                name: "FK_Units_Structures_StrcutureId",
                table: "Units");

            migrationBuilder.DropForeignKey(
                name: "FK_Units_Turns_TurnId",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_GameId",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_RegionId",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_StrcutureId",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_TurnId",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Turns_GameId",
                table: "Turns");

            migrationBuilder.DropIndex(
                name: "IX_Structures_GameId",
                table: "Structures");

            migrationBuilder.DropIndex(
                name: "IX_Structures_RegionId",
                table: "Structures");

            migrationBuilder.DropIndex(
                name: "IX_Structures_TurnId",
                table: "Structures");

            migrationBuilder.DropIndex(
                name: "IX_Regions_GameId",
                table: "Regions");

            migrationBuilder.DropIndex(
                name: "IX_Regions_TurnId",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "StrcutureId",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "Structures");

            migrationBuilder.DropColumn(
                name: "X",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "Z",
                table: "Regions");
        }
    }
}
