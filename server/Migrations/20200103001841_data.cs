using Microsoft.EntityFrameworkCore.Migrations;

namespace atlantis.Migrations
{
    public partial class data : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FactionNumber",
                table: "Units",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Json",
                table: "Units",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Memory",
                table: "Units",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Units",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Orders",
                table: "Units",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Own",
                table: "Units",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                table: "Units",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "Turns",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Turns",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Json",
                table: "Structures",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Memory",
                table: "Structures",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Structures",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                table: "Structures",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Structures",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Json",
                table: "Regions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Memory",
                table: "Regions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EngineVersion",
                table: "Games",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlayerFactionNumber",
                table: "Games",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RulesetVersion",
                table: "Games",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Factions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<long>(nullable: false),
                    TurnId = table.Column<long>(nullable: false),
                    Number = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Json = table.Column<string>(nullable: true),
                    Memory = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Factions_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Factions_Turns_TurnId",
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
                    GameId = table.Column<long>(nullable: false),
                    TurnId = table.Column<long>(nullable: false),
                    FactionId = table.Column<long>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    Json = table.Column<string>(nullable: true)
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
                        name: "FK_Events_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Events_Turns_TurnId",
                        column: x => x.TurnId,
                        principalTable: "Turns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_FactionId",
                table: "Events",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_GameId",
                table: "Events",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_TurnId",
                table: "Events",
                column: "TurnId");

            migrationBuilder.CreateIndex(
                name: "IX_Factions_GameId",
                table: "Factions",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Factions_TurnId",
                table: "Factions",
                column: "TurnId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Factions");

            migrationBuilder.DropColumn(
                name: "FactionNumber",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "Json",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "Memory",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "Orders",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "Own",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "Month",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "Json",
                table: "Structures");

            migrationBuilder.DropColumn(
                name: "Memory",
                table: "Structures");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Structures");

            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "Structures");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Structures");

            migrationBuilder.DropColumn(
                name: "Json",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "Memory",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "EngineVersion",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "PlayerFactionNumber",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "RulesetVersion",
                table: "Games");
        }
    }
}
