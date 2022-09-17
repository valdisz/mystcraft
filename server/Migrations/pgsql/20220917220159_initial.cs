using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace advisor.Migrations.pgsql
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameEngines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Contents = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameEngines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Salt = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Algorithm = table.Column<string>(type: "text", nullable: false),
                    Digest = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Roles = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EngineId = table.Column<long>(type: "bigint", nullable: true),
                    Options = table.Column<string>(type: "jsonb", nullable: false),
                    Ruleset = table.Column<string>(type: "text", nullable: false),
                    LastTurnNumber = table.Column<int>(type: "integer", nullable: true),
                    NextTurnNumber = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Games_GameEngines_EngineId",
                        column: x => x.EngineId,
                        principalTable: "GameEngines",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Alliances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alliances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alliances_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    GameId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastTurnNumber = table.Column<int>(type: "integer", nullable: false),
                    NextTurnNumber = table.Column<int>(type: "integer", nullable: true),
                    Password = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsQuit = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Players_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Registrations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Password = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Registrations_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Registrations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Turns",
                columns: table => new
                {
                    GameId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PlayerData = table.Column<byte[]>(type: "bytea", nullable: true),
                    GameData = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turns", x => new { x.GameId, x.Number });
                    table.ForeignKey(
                        name: "FK_Turns_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AllianceMembers",
                columns: table => new
                {
                    AllianceId = table.Column<long>(type: "bigint", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    ShareMap = table.Column<bool>(type: "boolean", nullable: false),
                    TeachMages = table.Column<bool>(type: "boolean", nullable: false),
                    Owner = table.Column<bool>(type: "boolean", nullable: false),
                    CanInvite = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllianceMembers", x => new { x.PlayerId, x.AllianceId });
                    table.ForeignKey(
                        name: "FK_AllianceMembers_Alliances_AllianceId",
                        column: x => x.AllianceId,
                        principalTable: "Alliances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AllianceMembers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerTurns",
                columns: table => new
                {
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Ready = table.Column<bool>(type: "boolean", nullable: false),
                    OrdersSubmitted = table.Column<bool>(type: "boolean", nullable: false),
                    TimesSubmitted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerTurns", x => new { x.PlayerId, x.TurnNumber });
                    table.ForeignKey(
                        name: "FK_PlayerTurns_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<long>(type: "bigint", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: true),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Articles_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Articles_Turns_GameId_TurnNumber",
                        columns: x => new { x.GameId, x.TurnNumber },
                        principalTable: "Turns",
                        principalColumns: new[] { "GameId", "Number" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    GameId = table.Column<long>(type: "bigint", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    FactionNumber = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false),
                    Json = table.Column<byte[]>(type: "bytea", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true),
                    Parsed = table.Column<bool>(type: "boolean", nullable: false),
                    Imported = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => new { x.GameId, x.TurnNumber, x.FactionNumber });
                    table.ForeignKey(
                        name: "FK_Reports_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reports_Turns_GameId_TurnNumber",
                        columns: x => new { x.GameId, x.TurnNumber },
                        principalTable: "Turns",
                        principalColumns: new[] { "GameId", "Number" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AditionalReports",
                columns: table => new
                {
                    FactionNumber = table.Column<int>(type: "integer", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    FactionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Json = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AditionalReports", x => new { x.PlayerId, x.TurnNumber, x.FactionNumber });
                    table.ForeignKey(
                        name: "FK_AditionalReports_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AditionalReports_PlayerTurns_PlayerId_TurnNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber },
                        principalTable: "PlayerTurns",
                        principalColumns: new[] { "PlayerId", "TurnNumber" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Battles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    X = table.Column<int>(type: "integer", nullable: false),
                    Y = table.Column<int>(type: "integer", nullable: false),
                    Z = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Province = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Terrain = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Attacker_Number = table.Column<int>(type: "integer", nullable: false),
                    Attacker_Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Defender_Number = table.Column<int>(type: "integer", nullable: false),
                    Defender_Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Battle = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Battles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Battles_PlayerTurns_PlayerId_TurnNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber },
                        principalTable: "PlayerTurns",
                        principalColumns: new[] { "PlayerId", "TurnNumber" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Factions",
                columns: table => new
                {
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DefaultAttitude = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factions", x => new { x.PlayerId, x.TurnNumber, x.Number });
                    table.ForeignKey(
                        name: "FK_Factions_PlayerTurns_PlayerId_TurnNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber },
                        principalTable: "PlayerTurns",
                        principalColumns: new[] { "PlayerId", "TurnNumber" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    UnitNumber = table.Column<int>(type: "integer", nullable: false),
                    Orders = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => new { x.PlayerId, x.TurnNumber, x.UnitNumber });
                    table.ForeignKey(
                        name: "FK_Orders_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_PlayerTurns_PlayerId_TurnNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber },
                        principalTable: "PlayerTurns",
                        principalColumns: new[] { "PlayerId", "TurnNumber" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    X = table.Column<int>(type: "integer", nullable: false),
                    Y = table.Column<int>(type: "integer", nullable: false),
                    Z = table.Column<int>(type: "integer", nullable: false),
                    Explored = table.Column<bool>(type: "boolean", nullable: false),
                    LastVisitedAt = table.Column<int>(type: "integer", nullable: true),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Province = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Terrain = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Settlement_Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Settlement_Size = table.Column<string>(type: "text", nullable: true),
                    Population = table.Column<int>(type: "integer", nullable: false),
                    Race = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Entertainment = table.Column<int>(type: "integer", nullable: false),
                    Tax = table.Column<int>(type: "integer", nullable: false),
                    Wages = table.Column<double>(type: "double precision", nullable: false),
                    TotalWages = table.Column<int>(type: "integer", nullable: false),
                    Gate = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => new { x.PlayerId, x.TurnNumber, x.Id });
                    table.ForeignKey(
                        name: "FK_Regions_PlayerTurns_PlayerId_TurnNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber },
                        principalTable: "PlayerTurns",
                        principalColumns: new[] { "PlayerId", "TurnNumber" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attitudes",
                columns: table => new
                {
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    FactionNumber = table.Column<int>(type: "integer", nullable: false),
                    TargetFactionNumber = table.Column<int>(type: "integer", nullable: false),
                    Stance = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attitudes", x => new { x.PlayerId, x.TurnNumber, x.FactionNumber, x.TargetFactionNumber });
                    table.ForeignKey(
                        name: "FK_Attitudes_Factions_PlayerId_TurnNumber_FactionNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.FactionNumber },
                        principalTable: "Factions",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "Number" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attitudes_PlayerTurns_PlayerId_TurnNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber },
                        principalTable: "PlayerTurns",
                        principalColumns: new[] { "PlayerId", "TurnNumber" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Exits",
                columns: table => new
                {
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    OriginRegionId = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    TargetRegionId = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    Direction = table.Column<string>(type: "text", nullable: false),
                    X = table.Column<int>(type: "integer", nullable: false),
                    Y = table.Column<int>(type: "integer", nullable: false),
                    Z = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Province = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Terrain = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Settlement_Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Settlement_Size = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exits", x => new { x.PlayerId, x.TurnNumber, x.OriginRegionId, x.TargetRegionId });
                    table.ForeignKey(
                        name: "FK_Exits_PlayerTurns_PlayerId_TurnNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber },
                        principalTable: "PlayerTurns",
                        principalColumns: new[] { "PlayerId", "TurnNumber" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exits_Regions_PlayerId_TurnNumber_OriginRegionId",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.OriginRegionId },
                        principalTable: "Regions",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exits_Regions_PlayerId_TurnNumber_TargetRegionId",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.TargetRegionId },
                        principalTable: "Regions",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Markets",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Market = table.Column<string>(type: "text", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    RegionId = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Markets", x => new { x.PlayerId, x.TurnNumber, x.RegionId, x.Market, x.Code });
                    table.ForeignKey(
                        name: "FK_Markets_PlayerTurns_PlayerId_TurnNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber },
                        principalTable: "PlayerTurns",
                        principalColumns: new[] { "PlayerId", "TurnNumber" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Markets_Regions_PlayerId_TurnNumber_RegionId",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.RegionId },
                        principalTable: "Regions",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Production",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    RegionId = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Production", x => new { x.PlayerId, x.TurnNumber, x.RegionId, x.Code });
                    table.ForeignKey(
                        name: "FK_Production_PlayerTurns_PlayerId_TurnNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber },
                        principalTable: "PlayerTurns",
                        principalColumns: new[] { "PlayerId", "TurnNumber" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Production_Regions_PlayerId_TurnNumber_RegionId",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.RegionId },
                        principalTable: "Regions",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stats",
                columns: table => new
                {
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    RegionId = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    Income_Work = table.Column<int>(type: "integer", nullable: true),
                    Income_Tax = table.Column<int>(type: "integer", nullable: true),
                    Income_Pillage = table.Column<int>(type: "integer", nullable: true),
                    Income_Trade = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stats", x => new { x.PlayerId, x.TurnNumber, x.RegionId });
                    table.ForeignKey(
                        name: "FK_Stats_PlayerTurns_PlayerId_TurnNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber },
                        principalTable: "PlayerTurns",
                        principalColumns: new[] { "PlayerId", "TurnNumber" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stats_Regions_PlayerId_TurnNumber_RegionId",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.RegionId },
                        principalTable: "Regions",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Structures",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    RegionId = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    X = table.Column<int>(type: "integer", nullable: false),
                    Y = table.Column<int>(type: "integer", nullable: false),
                    Z = table.Column<int>(type: "integer", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Contents = table.Column<string>(type: "jsonb", nullable: true),
                    Flags = table.Column<string>(type: "jsonb", nullable: true),
                    SailDirections = table.Column<string>(type: "jsonb", nullable: true),
                    Speed = table.Column<int>(type: "integer", nullable: true),
                    Needs = table.Column<int>(type: "integer", nullable: true),
                    Load_Used = table.Column<int>(type: "integer", nullable: true),
                    Load_Max = table.Column<int>(type: "integer", nullable: true),
                    Sailors_Current = table.Column<int>(type: "integer", nullable: true),
                    Sailors_Required = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Structures", x => new { x.PlayerId, x.TurnNumber, x.Id });
                    table.ForeignKey(
                        name: "FK_Structures_PlayerTurns_PlayerId_TurnNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber },
                        principalTable: "PlayerTurns",
                        principalColumns: new[] { "PlayerId", "TurnNumber" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Structures_Regions_PlayerId_TurnNumber_RegionId",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.RegionId },
                        principalTable: "Regions",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "Id" });
                });

            migrationBuilder.CreateTable(
                name: "StatItems",
                columns: table => new
                {
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    RegionId = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    Code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatItems", x => new { x.PlayerId, x.TurnNumber, x.RegionId, x.Code });
                    table.ForeignKey(
                        name: "FK_StatItems_Regions_PlayerId_TurnNumber_RegionId",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.RegionId },
                        principalTable: "Regions",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatItems_Stats_PlayerId_TurnNumber_RegionId",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.RegionId },
                        principalTable: "Stats",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "RegionId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Number = table.Column<int>(type: "integer", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    X = table.Column<int>(type: "integer", nullable: false),
                    Y = table.Column<int>(type: "integer", nullable: false),
                    Z = table.Column<int>(type: "integer", nullable: false),
                    RegionId = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    StrcutureId = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    StructureNumber = table.Column<int>(type: "integer", nullable: true),
                    FactionNumber = table.Column<int>(type: "integer", nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    OnGuard = table.Column<bool>(type: "boolean", nullable: false),
                    Flags = table.Column<string>(type: "jsonb", nullable: true),
                    Weight = table.Column<int>(type: "integer", nullable: true),
                    Capacity_Flying = table.Column<int>(type: "integer", nullable: true),
                    Capacity_Riding = table.Column<int>(type: "integer", nullable: true),
                    Capacity_Walking = table.Column<int>(type: "integer", nullable: true),
                    Capacity_Swimming = table.Column<int>(type: "integer", nullable: true),
                    Skills = table.Column<string>(type: "jsonb", nullable: true),
                    CanStudy = table.Column<string>(type: "jsonb", nullable: true),
                    ReadyItem = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    CombatSpell = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    Orders = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => new { x.PlayerId, x.TurnNumber, x.Number });
                    table.ForeignKey(
                        name: "FK_Units_Factions_PlayerId_TurnNumber_FactionNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.FactionNumber },
                        principalTable: "Factions",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "Number" });
                    table.ForeignKey(
                        name: "FK_Units_PlayerTurns_PlayerId_TurnNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber },
                        principalTable: "PlayerTurns",
                        principalColumns: new[] { "PlayerId", "TurnNumber" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Units_Regions_PlayerId_TurnNumber_RegionId",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.RegionId },
                        principalTable: "Regions",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "Id" });
                    table.ForeignKey(
                        name: "FK_Units_Structures_PlayerId_TurnNumber_StrcutureId",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.StrcutureId },
                        principalTable: "Structures",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "Id" });
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    FactionNumber = table.Column<int>(type: "integer", nullable: false),
                    RegionId = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    UnitNumber = table.Column<int>(type: "integer", nullable: true),
                    UnitName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MissingUnitNumber = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: true),
                    ItemCode = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    ItemName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ItemPrice = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Factions_PlayerId_TurnNumber_FactionNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.FactionNumber },
                        principalTable: "Factions",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "Number" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Events_PlayerTurns_PlayerId_TurnNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber },
                        principalTable: "PlayerTurns",
                        principalColumns: new[] { "PlayerId", "TurnNumber" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Events_Regions_PlayerId_TurnNumber_RegionId",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.RegionId },
                        principalTable: "Regions",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "Id" });
                    table.ForeignKey(
                        name: "FK_Events_Units_PlayerId_TurnNumber_UnitNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.UnitNumber },
                        principalTable: "Units",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "Number" });
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    UnitNumber = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Illusion = table.Column<bool>(type: "boolean", nullable: false),
                    Unfinished = table.Column<bool>(type: "boolean", nullable: false),
                    Props = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => new { x.PlayerId, x.TurnNumber, x.UnitNumber, x.Code });
                    table.ForeignKey(
                        name: "FK_Items_PlayerTurns_PlayerId_TurnNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber },
                        principalTable: "PlayerTurns",
                        principalColumns: new[] { "PlayerId", "TurnNumber" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Items_Units_PlayerId_TurnNumber_UnitNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.UnitNumber },
                        principalTable: "Units",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "Number" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudyPlans",
                columns: table => new
                {
                    UnitNumber = table.Column<int>(type: "integer", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    Target_Code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    Target_Level = table.Column<int>(type: "integer", nullable: true),
                    Study = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Teach = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyPlans", x => new { x.PlayerId, x.TurnNumber, x.UnitNumber });
                    table.ForeignKey(
                        name: "FK_StudyPlans_PlayerTurns_PlayerId_TurnNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber },
                        principalTable: "PlayerTurns",
                        principalColumns: new[] { "PlayerId", "TurnNumber" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudyPlans_Units_PlayerId_TurnNumber_UnitNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber, x.UnitNumber },
                        principalTable: "Units",
                        principalColumns: new[] { "PlayerId", "TurnNumber", "Number" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllianceMembers_AllianceId",
                table: "AllianceMembers",
                column: "AllianceId");

            migrationBuilder.CreateIndex(
                name: "IX_Alliances_GameId",
                table: "Alliances",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_GameId_TurnNumber",
                table: "Articles",
                columns: new[] { "GameId", "TurnNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Battles_PlayerId_TurnNumber",
                table: "Battles",
                columns: new[] { "PlayerId", "TurnNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_PlayerId_TurnNumber_FactionNumber",
                table: "Events",
                columns: new[] { "PlayerId", "TurnNumber", "FactionNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_PlayerId_TurnNumber_RegionId",
                table: "Events",
                columns: new[] { "PlayerId", "TurnNumber", "RegionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_PlayerId_TurnNumber_UnitNumber",
                table: "Events",
                columns: new[] { "PlayerId", "TurnNumber", "UnitNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Exits_PlayerId_TurnNumber_TargetRegionId",
                table: "Exits",
                columns: new[] { "PlayerId", "TurnNumber", "TargetRegionId" });

            migrationBuilder.CreateIndex(
                name: "IX_GameEngines_Name",
                table: "GameEngines",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_EngineId",
                table: "Games",
                column: "EngineId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_Name",
                table: "Games",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_GameId",
                table: "Players",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_UserId",
                table: "Players",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_GameId",
                table: "Registrations",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_UserId",
                table: "Registrations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Structures_PlayerId_TurnNumber_RegionId",
                table: "Structures",
                columns: new[] { "PlayerId", "TurnNumber", "RegionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Units_PlayerId_TurnNumber_FactionNumber",
                table: "Units",
                columns: new[] { "PlayerId", "TurnNumber", "FactionNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Units_PlayerId_TurnNumber_RegionId",
                table: "Units",
                columns: new[] { "PlayerId", "TurnNumber", "RegionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Units_PlayerId_TurnNumber_StrcutureId",
                table: "Units",
                columns: new[] { "PlayerId", "TurnNumber", "StrcutureId" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AditionalReports");

            migrationBuilder.DropTable(
                name: "AllianceMembers");

            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "Attitudes");

            migrationBuilder.DropTable(
                name: "Battles");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Exits");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Markets");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Production");

            migrationBuilder.DropTable(
                name: "Registrations");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "StatItems");

            migrationBuilder.DropTable(
                name: "StudyPlans");

            migrationBuilder.DropTable(
                name: "Alliances");

            migrationBuilder.DropTable(
                name: "Turns");

            migrationBuilder.DropTable(
                name: "Stats");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "Factions");

            migrationBuilder.DropTable(
                name: "Structures");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "PlayerTurns");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "GameEngines");
        }
    }
}
