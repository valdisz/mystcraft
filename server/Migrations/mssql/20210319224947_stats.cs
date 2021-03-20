using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.mssql
{
    public partial class stats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FactionStats",
                columns: table => new
                {
                    FactionId = table.Column<long>(type: "bigint", nullable: false),
                    TurnId = table.Column<long>(type: "bigint", nullable: false),
                    Income_Work = table.Column<int>(type: "int", nullable: true),
                    Income_Tax = table.Column<int>(type: "int", nullable: true),
                    Income_Pillage = table.Column<int>(type: "int", nullable: true),
                    Income_Trade = table.Column<int>(type: "int", nullable: true)
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
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TurnId = table.Column<long>(type: "bigint", nullable: false),
                    FactionId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: true)
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FactionStats_Production");

            migrationBuilder.DropTable(
                name: "FactionStats");
        }
    }
}
