using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.mssql
{
    public partial class battles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Illusion",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Props",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Unfinished",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Battles",
                columns: table => new
                {
                    PlayerId = table.Column<long>(type: "bigint", nullable: false),
                    TurnNumber = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<long>(type: "bigint", nullable: false),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false),
                    Z = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Province = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Terrain = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Attacker_Number = table.Column<int>(type: "int", nullable: false),
                    Attacker_Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Defender_Number = table.Column<int>(type: "int", nullable: false),
                    Defender_Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Battle = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Battles", x => new { x.PlayerId, x.TurnNumber });
                    table.ForeignKey(
                        name: "FK_Battles_Turns_PlayerId_TurnNumber",
                        columns: x => new { x.PlayerId, x.TurnNumber },
                        principalTable: "Turns",
                        principalColumns: new[] { "PlayerId", "Number" },
                        onDelete: ReferentialAction.Restrict);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Battles");

            migrationBuilder.DropColumn(
                name: "Illusion",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Props",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Unfinished",
                table: "Items");
        }
    }
}
