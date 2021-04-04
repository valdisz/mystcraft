using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.sqlite
{
    public partial class unique_indices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Units_TurnId",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Structures_TurnId",
                table: "Structures");

            migrationBuilder.DropIndex(
                name: "IX_Regions_TurnId",
                table: "Regions");

            migrationBuilder.DropIndex(
                name: "IX_Factions_TurnId",
                table: "Factions");

            migrationBuilder.CreateIndex(
                name: "IX_Units_TurnId_Number",
                table: "Units",
                columns: new[] { "TurnId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Structures_TurnId_RegionId_Number",
                table: "Structures",
                columns: new[] { "TurnId", "RegionId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_TurnId_X_Y_Z",
                table: "Regions",
                columns: new[] { "TurnId", "X", "Y", "Z" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Factions_TurnId_Number",
                table: "Factions",
                columns: new[] { "TurnId", "Number" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Units_TurnId_Number",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Structures_TurnId_RegionId_Number",
                table: "Structures");

            migrationBuilder.DropIndex(
                name: "IX_Regions_TurnId_X_Y_Z",
                table: "Regions");

            migrationBuilder.DropIndex(
                name: "IX_Factions_TurnId_Number",
                table: "Factions");

            migrationBuilder.CreateIndex(
                name: "IX_Units_TurnId",
                table: "Units",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_Structures_TurnId",
                table: "Structures",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_TurnId",
                table: "Regions",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_Factions_TurnId",
                table: "Factions",
                column: "TurnId");
        }
    }
}
