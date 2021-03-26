using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.sqlite
{
    public partial class stats_by_region : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegionStats",
                columns: table => new
                {
                    TurnId = table.Column<long>(type: "INTEGER", nullable: false),
                    FactionId = table.Column<long>(type: "INTEGER", nullable: false),
                    RegionId = table.Column<long>(type: "INTEGER", nullable: false),
                    Income_Work = table.Column<int>(type: "INTEGER", nullable: true),
                    Income_Tax = table.Column<int>(type: "INTEGER", nullable: true),
                    Income_Pillage = table.Column<int>(type: "INTEGER", nullable: true),
                    Income_Trade = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegionStats", x => new { x.TurnId, x.FactionId, x.RegionId });
                    table.ForeignKey(
                        name: "FK_RegionStats_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegionStats_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegionStats_Turns_TurnId",
                        column: x => x.TurnId,
                        principalTable: "Turns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FactionStats_Production",
                columns: table => new
                {
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    TurnId = table.Column<long>(type: "INTEGER", nullable: false),
                    FactionId = table.Column<long>(type: "INTEGER", nullable: false),
                    RegionId = table.Column<long>(type: "INTEGER", nullable: false),
                    Amount = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactionStats_Production", x => new { x.TurnId, x.FactionId, x.RegionId, x.Code });
                    table.ForeignKey(
                        name: "FK_FactionStats_Production_RegionStats_TurnId_FactionId_RegionId",
                        columns: x => new { x.TurnId, x.FactionId, x.RegionId },
                        principalTable: "RegionStats",
                        principalColumns: new[] { "TurnId", "FactionId", "RegionId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegionStats_FactionId",
                table: "RegionStats",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RegionStats_RegionId",
                table: "RegionStats",
                column: "RegionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FactionStats_Production");

            migrationBuilder.DropTable(
                name: "RegionStats");
        }
    }
}
