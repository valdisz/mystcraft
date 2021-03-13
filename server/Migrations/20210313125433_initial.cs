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
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    RemoteGameOptions = table.Column<string>(type: "TEXT", nullable: true),
                    EngineVersion = table.Column<string>(type: "TEXT", nullable: true),
                    RulesetName = table.Column<string>(type: "TEXT", nullable: true),
                    RulesetVersion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Salt = table.Column<string>(type: "TEXT", nullable: false),
                    Algorithm = table.Column<string>(type: "TEXT", nullable: false),
                    Digest = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Universities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Universities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Universities_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGames",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    GameId = table.Column<long>(type: "INTEGER", nullable: false),
                    PlayerFactionNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    PlayerFactionName = table.Column<string>(type: "TEXT", nullable: true),
                    LastTurnNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGames_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users_Role",
                columns: table => new
                {
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users_Role", x => new { x.UserId, x.Role });
                    table.ForeignKey(
                        name: "FK_Users_Role_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "University_User",
                columns: table => new
                {
                    UniversityId = table.Column<long>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_University_User", x => new { x.UserId, x.UniversityId });
                    table.ForeignKey(
                        name: "FK_University_User_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_University_User_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Turns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserGameId = table.Column<long>(type: "INTEGER", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turns_UserGames_UserGameId",
                        column: x => x.UserGameId,
                        principalTable: "UserGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Factions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TurnId = table.Column<long>(type: "INTEGER", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
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
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    TurnId = table.Column<long>(type: "INTEGER", nullable: false),
                    X = table.Column<int>(type: "INTEGER", nullable: false),
                    Y = table.Column<int>(type: "INTEGER", nullable: false),
                    Z = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAtTurn = table.Column<int>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    Province = table.Column<string>(type: "TEXT", nullable: false),
                    Terrain = table.Column<string>(type: "TEXT", nullable: false),
                    Settlement_Name = table.Column<string>(type: "TEXT", nullable: true),
                    Settlement_Size = table.Column<string>(type: "TEXT", nullable: true),
                    Population = table.Column<int>(type: "INTEGER", nullable: false),
                    Race = table.Column<string>(type: "TEXT", nullable: true),
                    Entertainment = table.Column<int>(type: "INTEGER", nullable: false),
                    Tax = table.Column<int>(type: "INTEGER", nullable: false),
                    Wages = table.Column<double>(type: "REAL", nullable: false),
                    TotalWages = table.Column<int>(type: "INTEGER", nullable: false)
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
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserGameId = table.Column<long>(type: "INTEGER", nullable: false),
                    TurnId = table.Column<long>(type: "INTEGER", nullable: false),
                    FactionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    FactionName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    Json = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_Turns_TurnId",
                        column: x => x.TurnId,
                        principalTable: "Turns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reports_UserGames_UserGameId",
                        column: x => x.UserGameId,
                        principalTable: "UserGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TurnId = table.Column<long>(type: "INTEGER", nullable: false),
                    FactionId = table.Column<long>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false)
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
                    Direction = table.Column<string>(type: "TEXT", nullable: false),
                    RegionId = table.Column<long>(type: "INTEGER", nullable: false),
                    X = table.Column<int>(type: "INTEGER", nullable: false),
                    Y = table.Column<int>(type: "INTEGER", nullable: false),
                    Z = table.Column<int>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    Province = table.Column<string>(type: "TEXT", nullable: false),
                    Terrain = table.Column<string>(type: "TEXT", nullable: false),
                    Settlement_Name = table.Column<string>(type: "TEXT", nullable: true),
                    Settlement_Size = table.Column<string>(type: "TEXT", nullable: true)
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
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    RegionId = table.Column<long>(type: "INTEGER", nullable: false),
                    Amount = table.Column<int>(type: "INTEGER", nullable: false),
                    Price = table.Column<int>(type: "INTEGER", nullable: false)
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
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    RegionId = table.Column<long>(type: "INTEGER", nullable: false),
                    Amount = table.Column<int>(type: "INTEGER", nullable: true)
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
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    RegionId = table.Column<long>(type: "INTEGER", nullable: false),
                    Amount = table.Column<int>(type: "INTEGER", nullable: false),
                    Price = table.Column<int>(type: "INTEGER", nullable: false)
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
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    TurnId = table.Column<long>(type: "INTEGER", nullable: false),
                    RegionId = table.Column<long>(type: "INTEGER", nullable: false),
                    X = table.Column<int>(type: "INTEGER", nullable: false),
                    Y = table.Column<int>(type: "INTEGER", nullable: false),
                    Z = table.Column<int>(type: "INTEGER", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Flags = table.Column<string>(type: "TEXT", nullable: true),
                    SailDirections = table.Column<string>(type: "TEXT", nullable: true),
                    Speed = table.Column<int>(type: "INTEGER", nullable: true),
                    Needs = table.Column<int>(type: "INTEGER", nullable: true),
                    Load_Used = table.Column<int>(type: "INTEGER", nullable: true),
                    Load_Max = table.Column<int>(type: "INTEGER", nullable: true),
                    Sailors_Current = table.Column<int>(type: "INTEGER", nullable: true),
                    Sailors_Required = table.Column<int>(type: "INTEGER", nullable: true)
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
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    StructureId = table.Column<long>(type: "INTEGER", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false)
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
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    TurnId = table.Column<long>(type: "INTEGER", nullable: false),
                    RegionId = table.Column<long>(type: "INTEGER", nullable: false),
                    StrcutureId = table.Column<long>(type: "INTEGER", nullable: true),
                    FactionId = table.Column<long>(type: "INTEGER", nullable: true),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    OnGuard = table.Column<bool>(type: "INTEGER", nullable: false),
                    Flags = table.Column<string>(type: "TEXT", nullable: true),
                    Weight = table.Column<int>(type: "INTEGER", nullable: true),
                    Capacity_Flying = table.Column<int>(type: "INTEGER", nullable: true),
                    Capacity_Riding = table.Column<int>(type: "INTEGER", nullable: true),
                    Capacity_Walking = table.Column<int>(type: "INTEGER", nullable: true),
                    Capacity_Swimming = table.Column<int>(type: "INTEGER", nullable: true),
                    ReadyItem_Code = table.Column<string>(type: "TEXT", nullable: true),
                    ReadyItem_Amount = table.Column<int>(type: "INTEGER", nullable: true),
                    CombatSpell_Code = table.Column<string>(type: "TEXT", nullable: true),
                    CombatSpell_Level = table.Column<int>(type: "INTEGER", nullable: true),
                    CombatSpell_Days = table.Column<int>(type: "INTEGER", nullable: true)
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
                name: "StudyPlans",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    UniversityId = table.Column<long>(type: "INTEGER", nullable: false),
                    TurnId = table.Column<long>(type: "INTEGER", nullable: false),
                    UnitId = table.Column<long>(type: "INTEGER", nullable: false),
                    Target_Code = table.Column<string>(type: "TEXT", nullable: true),
                    Target_Level = table.Column<int>(type: "INTEGER", nullable: true),
                    Target_Days = table.Column<int>(type: "INTEGER", nullable: true),
                    Learn = table.Column<string>(type: "TEXT", nullable: true),
                    Teach = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyPlans_Turns_TurnId",
                        column: x => x.TurnId,
                        principalTable: "Turns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudyPlans_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudyPlans_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Unit_CanStudy",
                columns: table => new
                {
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    UnitId = table.Column<long>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: true),
                    Days = table.Column<int>(type: "INTEGER", nullable: true)
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
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    UnitId = table.Column<long>(type: "INTEGER", nullable: false),
                    Amount = table.Column<int>(type: "INTEGER", nullable: true)
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
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    UnitId = table.Column<long>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: true),
                    Days = table.Column<int>(type: "INTEGER", nullable: true)
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
                name: "IX_Reports_TurnId",
                table: "Reports",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserGameId",
                table: "Reports",
                column: "UserGameId");

            migrationBuilder.CreateIndex(
                name: "IX_Structures_RegionId",
                table: "Structures",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Structures_TurnId",
                table: "Structures",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlans_TurnId",
                table: "StudyPlans",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlans_UnitId",
                table: "StudyPlans",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyPlans_UniversityId",
                table: "StudyPlans",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_UserGameId",
                table: "Turns",
                column: "UserGameId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Universities_GameId",
                table: "Universities",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_University_User_UniversityId",
                table: "University_User",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGames_GameId",
                table: "UserGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGames_UserId",
                table: "UserGames",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
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
                name: "StudyPlans");

            migrationBuilder.DropTable(
                name: "Unit_CanStudy");

            migrationBuilder.DropTable(
                name: "Unit_Items");

            migrationBuilder.DropTable(
                name: "Unit_Skills");

            migrationBuilder.DropTable(
                name: "University_User");

            migrationBuilder.DropTable(
                name: "Users_Role");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "Universities");

            migrationBuilder.DropTable(
                name: "Factions");

            migrationBuilder.DropTable(
                name: "Structures");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "Turns");

            migrationBuilder.DropTable(
                name: "UserGames");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
