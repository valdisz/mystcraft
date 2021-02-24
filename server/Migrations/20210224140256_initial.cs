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
                name: "Factions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TurnId = table.Column<long>(nullable: false),
                    Number = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Factions_Turns_TurnId",
                        column: x => x.TurnId,
                        principalTable: "Turns",
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
                    Settlement_Name = table.Column<string>(nullable: true),
                    Settlement_Size = table.Column<string>(nullable: true),
                    Population = table.Column<int>(nullable: false),
                    Race = table.Column<string>(nullable: true),
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
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TurnId = table.Column<long>(nullable: false),
                    FactionId = table.Column<long>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    Message = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Events_Turns_TurnId",
                        column: x => x.TurnId,
                        principalTable: "Turns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Regions_Exits",
                columns: table => new
                {
                    Direction = table.Column<string>(nullable: false),
                    RegionId = table.Column<long>(nullable: false),
                    X = table.Column<int>(nullable: false),
                    Y = table.Column<int>(nullable: false),
                    Z = table.Column<int>(nullable: false),
                    Label = table.Column<string>(nullable: false),
                    Province = table.Column<string>(nullable: false),
                    Terrain = table.Column<string>(nullable: false),
                    Settlement_Name = table.Column<string>(nullable: true),
                    Settlement_Size = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions_Exits", x => new { x.RegionId, x.Direction });
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
                    Amount = table.Column<int>(nullable: false),
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
                    Amount = table.Column<int>(nullable: true)
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
                    Amount = table.Column<int>(nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Structures",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TurnId = table.Column<long>(nullable: false),
                    RegionId = table.Column<long>(nullable: false),
                    X = table.Column<int>(nullable: false),
                    Y = table.Column<int>(nullable: false),
                    Z = table.Column<int>(nullable: false),
                    Sequence = table.Column<int>(nullable: false),
                    Number = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Flags = table.Column<string>(nullable: true),
                    SailDirections = table.Column<string>(nullable: true),
                    Speed = table.Column<int>(nullable: true),
                    Needs = table.Column<int>(nullable: true),
                    Load_Used = table.Column<int>(nullable: true),
                    Load_Max = table.Column<int>(nullable: true),
                    Sailors_Current = table.Column<int>(nullable: true),
                    Sailors_Required = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Structures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Structures_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Structures_Turns_TurnId",
                        column: x => x.TurnId,
                        principalTable: "Turns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Structures_Contents",
                columns: table => new
                {
                    Type = table.Column<string>(nullable: false),
                    StructureId = table.Column<long>(nullable: false),
                    Count = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Structures_Contents", x => new { x.StructureId, x.Type });
                    table.ForeignKey(
                        name: "FK_Structures_Contents_Structures_StructureId",
                        column: x => x.StructureId,
                        principalTable: "Structures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TurnId = table.Column<long>(nullable: false),
                    RegionId = table.Column<long>(nullable: false),
                    StrcutureId = table.Column<long>(nullable: true),
                    FactionId = table.Column<long>(nullable: true),
                    Sequence = table.Column<int>(nullable: false),
                    Number = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    OnGuard = table.Column<bool>(nullable: false),
                    Flags = table.Column<string>(nullable: true),
                    Weight = table.Column<int>(nullable: true),
                    Capacity_Flying = table.Column<int>(nullable: true),
                    Capacity_Riding = table.Column<int>(nullable: true),
                    Capacity_Walking = table.Column<int>(nullable: true),
                    Capacity_Swimming = table.Column<int>(nullable: true),
                    ReadyItem_Code = table.Column<string>(nullable: true),
                    ReadyItem_Amount = table.Column<int>(nullable: true),
                    CombatSpell_Code = table.Column<string>(nullable: true),
                    CombatSpell_Level = table.Column<int>(nullable: true),
                    CombatSpell_Days = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Units_Factions_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Factions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Units_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Units_Structures_StrcutureId",
                        column: x => x.StrcutureId,
                        principalTable: "Structures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Units_Turns_TurnId",
                        column: x => x.TurnId,
                        principalTable: "Turns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Unit_CanStudy",
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false),
                    UnitId = table.Column<long>(nullable: false),
                    Level = table.Column<int>(nullable: true),
                    Days = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unit_CanStudy", x => new { x.UnitId, x.Code });
                    table.ForeignKey(
                        name: "FK_Unit_CanStudy_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Unit_Items",
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false),
                    UnitId = table.Column<long>(nullable: false),
                    Amount = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unit_Items", x => new { x.UnitId, x.Code });
                    table.ForeignKey(
                        name: "FK_Unit_Items_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Unit_Skills",
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false),
                    UnitId = table.Column<long>(nullable: false),
                    Level = table.Column<int>(nullable: true),
                    Days = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unit_Skills", x => new { x.UnitId, x.Code });
                    table.ForeignKey(
                        name: "FK_Unit_Skills_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_FactionId",
                table: "Events",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_TurnId",
                table: "Events",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_Factions_TurnId",
                table: "Factions",
                column: "TurnId");

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
                name: "IX_Structures_RegionId",
                table: "Structures",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Structures_TurnId",
                table: "Structures",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_GameId",
                table: "Turns",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_FactionId",
                table: "Units",
                column: "FactionId");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");

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
                name: "Structures_Contents");

            migrationBuilder.DropTable(
                name: "Unit_CanStudy");

            migrationBuilder.DropTable(
                name: "Unit_Items");

            migrationBuilder.DropTable(
                name: "Unit_Skills");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "Factions");

            migrationBuilder.DropTable(
                name: "Structures");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "Turns");

            migrationBuilder.DropTable(
                name: "Games");
        }
    }
}
