using Microsoft.EntityFrameworkCore.Migrations;

namespace atlantis.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    PlayerFactionNumber = table.Column<int>(nullable: true),
                    PlayerFactionName = table.Column<string>(nullable: true),
                    LastTurnNumber = table.Column<int>(nullable: false),
                    EngineVersion = table.Column<string>(nullable: true),
                    RulesetName = table.Column<string>(nullable: true),
                    RulesetVersion = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Turns",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<long>(nullable: false),
                    Number = table.Column<int>(nullable: false),
                    Month = table.Column<int>(nullable: false),
                    Year = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turns_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TurnId = table.Column<long>(nullable: false),
                    X = table.Column<int>(nullable: false),
                    Y = table.Column<int>(nullable: false),
                    Z = table.Column<int>(nullable: false),
                    UpdatedAtTurn = table.Column<int>(nullable: false),
                    Label = table.Column<string>(nullable: false),
                    Province = table.Column<string>(nullable: false),
                    Terrain = table.Column<string>(nullable: false),
                    Entertainment = table.Column<int>(nullable: false),
                    Tax = table.Column<int>(nullable: false),
                    Wages = table.Column<double>(nullable: false),
                    TotalWages = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Regions_Turns_TurnId",
                        column: x => x.TurnId,
                        principalTable: "Turns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<long>(nullable: false),
                    TurnId = table.Column<long>(nullable: false),
                    FactionNumber = table.Column<int>(nullable: false),
                    FactionName = table.Column<string>(maxLength: 100, nullable: false),
                    Source = table.Column<string>(nullable: false),
                    Json = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reports_Turns_TurnId",
                        column: x => x.TurnId,
                        principalTable: "Turns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Regions_Exits",
                columns: table => new
                {
                    RegionUID = table.Column<string>(nullable: false),
                    RegionId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions_Exits", x => new { x.RegionId, x.RegionUID });
                    table.ForeignKey(
                        name: "FK_Regions_Exits_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Regions_ForSale",
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false),
                    RegionId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    Price = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions_ForSale", x => new { x.RegionId, x.Code });
                    table.ForeignKey(
                        name: "FK_Regions_ForSale_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Regions_Products",
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false),
                    RegionId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Count = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions_Products", x => new { x.RegionId, x.Code });
                    table.ForeignKey(
                        name: "FK_Regions_Products_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Regions_Wanted",
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false),
                    RegionId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    Price = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions_Wanted", x => new { x.RegionId, x.Code });
                    table.ForeignKey(
                        name: "FK_Regions_Wanted_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Regions_TurnId",
                table: "Regions",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_GameId",
                table: "Reports",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TurnId",
                table: "Reports",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_GameId",
                table: "Turns",
                column: "GameId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Regions_Exits");

            migrationBuilder.DropTable(
                name: "Regions_ForSale");

            migrationBuilder.DropTable(
                name: "Regions_Products");

            migrationBuilder.DropTable(
                name: "Regions_Wanted");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "Turns");

            migrationBuilder.DropTable(
                name: "Games");
        }
    }
}
