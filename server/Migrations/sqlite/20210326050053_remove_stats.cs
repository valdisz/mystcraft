using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.sqlite
{
    public partial class remove_stats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FactionStats_Production");

            migrationBuilder.DropTable(
                name: "FactionStats");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Terrain",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "X",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "Z",
                table: "Events",
                newName: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_RegionId",
                table: "Events",
                column: "RegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Regions_RegionId",
                table: "Events",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Regions_RegionId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_RegionId",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "RegionId",
                table: "Events",
                newName: "Z");

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Events",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Events",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Terrain",
                table: "Events",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "X",
                table: "Events",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Y",
                table: "Events",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FactionStats",
                columns: table => new
                {
                    TurnId = table.Column<long>(type: "INTEGER", nullable: false),
                    FactionId = table.Column<long>(type: "INTEGER", nullable: false),
                    Income_Pillage = table.Column<int>(type: "INTEGER", nullable: true),
                    Income_Tax = table.Column<int>(type: "INTEGER", nullable: true),
                    Income_Trade = table.Column<int>(type: "INTEGER", nullable: true),
                    Income_Work = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactionStats", x => new { x.TurnId, x.FactionId });
                    table.ForeignKey(
                        name: "FK_FactionStats_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FactionStats_Turns_TurnId",
                        column: x => x.TurnId,
                        principalTable: "Turns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FactionStats_Production",
                columns: table => new
                {
                    TurnId = table.Column<long>(type: "INTEGER", nullable: false),
                    FactionId = table.Column<long>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactionStats_Production", x => new { x.TurnId, x.FactionId, x.Code });
                    table.ForeignKey(
                        name: "FK_FactionStats_Production_FactionStats_TurnId_FactionId",
                        columns: x => new { x.TurnId, x.FactionId },
                        principalTable: "FactionStats",
                        principalColumns: new[] { "TurnId", "FactionId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FactionStats_FactionId",
                table: "FactionStats",
                column: "FactionId",
                unique: true);
        }
    }
}
