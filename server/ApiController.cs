namespace atlantis {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using atlantis.Persistence;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class Game {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    [Route("api")]
    public class ApiController : ControllerBase {
        public ApiController(Database db) {
            this.db = db;
        }

        private readonly Database db;

        [HttpGet("games")]
        public async Task<List<Game>> GetGamesAsync() {
            var games = (await db.Games.ToListAsync()).Select(x => new Game { Id = x.Id, Name = x.Name }).ToList();
            return games;
        }

        [HttpPost("games")]
        public async Task<long> CreateGameAsync([FromForm] string name) {
            var newGame = await db.Games.AddAsync(new DbGame {
                Name = name
            });
            await db.SaveChangesAsync();

            return newGame.Entity.Id;
        }

        [HttpPost("games/{gameId}/report")]
        public async Task UploadReport([FromRoute] long gameId) {
            using var bodyReader = new StreamReader(Request.Body);
            using var converter = new AtlantisReportJsonConverter(bodyReader);

            using var buffer = new MemoryStream();
            using var bufferWriter = new StreamWriter(buffer);
            using JsonWriter writer = new JsonTextWriter(bufferWriter);
            await converter.ConvertAsync(writer);

            buffer.Seek(0, SeekOrigin.Begin);

            using var bufferReader = new StreamReader(buffer);
            using JsonReader reader = new JsonTextReader(bufferReader);

            JObject report = await JObject.LoadAsync(reader);
            var faction = report["faction"] as JObject; // *
            var date = report["date"] as JObject; // *
            var engine = report["engine"] as JObject; // *
            var factionStatus = report["factionStatus"] as JArray; // *
            var errors = report["errors"] as JArray; // *
            var events = report["events"] as JArray; // *
            var skillReports = report["skillReports"] as JArray; // *
            var objectReports = report["objectReports"] as JArray; // *
            var itemReports = report["itemReports"] as JArray; // *
            var attitudes = report["attitudes"] as JObject; // *
            var unclaimedSilver = report["unclaimedSilver"]; // *
            var regions = report["regions"] as JArray;
            var ordersTemplate = report["ordersTemplate"] as JObject;

            var turn = new DbTurn {
                GameId = gameId,
                Year = date.Value<int>("year"),
                Month = MonthToNumber(date.Value<string>("month")),
                Factions = new List<DbFaction>(),
                Regions = new List<DbRegion>(),
                Structures = new List<DbStructure>(),
                Events = new List<DbEvent>(),
                Units = new List<DbUnit>()
            };
            turn.Number = (turn.Year - 1) * 12 + turn.Month;

            var existingTurn = await db.Turns.FirstOrDefaultAsync(x => x.GameId == gameId && x.Number == turn.Number);
            if (existingTurn != null) {
                db.Remove(existingTurn);
                await db.SaveChangesAsync();
            }

            db.ChangeTracker.AutoDetectChangesEnabled = false;
            var lastTurn = await db.Turns
                .OrderByDescending(x => x.Id)
                .Include(x => x.Factions)
                .Include(x => x.Regions)
                .Include(x => x.Units)
                .Include(x => x.Structures)
                .FirstOrDefaultAsync(x => x.GameId == gameId);

            Dictionary<string, string> regionMemory = lastTurn != null
                ? lastTurn.Regions.ToDictionary(k => k.EmpheralId, v => v.Memory)
                : new Dictionary<string, string>();

            Dictionary<int, string> unitMemory = lastTurn != null
                ? lastTurn.Units.Where(x => x.Own).ToDictionary(k => k.Number, v => v.Memory)
                : new Dictionary<int, string>();

            Dictionary<string, string> structuresMemory = lastTurn != null
                ? lastTurn.Structures.ToDictionary(k => k.EmpheralId, v => v.Memory)
                : new Dictionary<string, string>();

            db.ChangeTracker.AutoDetectChangesEnabled = true;
            var game = await db.Games.FindAsync(gameId);

            turn.Memory = lastTurn?.Memory;

            await db.Turns.AddAsync(turn);
            await db.SaveChangesAsync();

            var f = new DbFaction {
                GameId = gameId,
                TurnId = turn.Id,
                Name = faction.Value<string>("name"),
                Number = faction.Value<int>("number"),
                Json = new JObject(
                    new JProperty("faction", faction),
                    new JProperty("factionStatus", factionStatus),
                    new JProperty("skillReports", skillReports),
                    new JProperty("objectReports", objectReports),
                    new JProperty("itemReports", itemReports),
                    new JProperty("attitudes", attitudes),
                    new JProperty("unclaimedSilver", unclaimedSilver)
                ).ToString(),
            };

            if (!game.PlayerFactionNumber.HasValue) {
                game.PlayerFactionNumber = f.Number;
                game.EngineVersion = engine.Value<string>("version");

                var ruleset = engine["ruleset"] as JObject;
                game.RulesetName = ruleset.Value<string>("name");
                game.RulesetVersion = ruleset.Value<string>("version");
            }

            f.Own = f.Number == game.PlayerFactionNumber;

            f.Events = (events ?? Enumerable.Empty<JToken>())
                .Select(x => new DbEvent { GameId = gameId, TurnId = turn.Id, Type = "event", Json = x.ToString() })
                .Concat(
                    (errors ?? Enumerable.Empty<JToken>())
                        .Select(x => new DbEvent { GameId = gameId, TurnId = turn.Id, Type = "error", Json = x.ToString() })
                )
                .ToList();

            turn.Factions.Add(f);
            turn.Events.AddRange(f.Events);

            var orders = (ordersTemplate["units"] as JArray)
                .ToDictionary(k => k.Value<int>("unit"), v => v.Value<string>("orders"));

            foreach (JObject region in regions)
            {
                var coords = region["coords"] as JObject;

                var r = new DbRegion
                {
                    GameId = gameId,
                    TurnId = turn.Id,
                    X = coords.Value<int>("x"),
                    Y = coords.Value<int>("y"),
                    Z = coords.TryGetValue("z", StringComparison.OrdinalIgnoreCase, out var zCoord)
                        ? zCoord.Value<int>()
                        : 1,
                    Label = coords.TryGetValue("label", StringComparison.OrdinalIgnoreCase, out var label)
                        ? label.Value<string>()
                        : "surface",
                    Province = region.Value<string>("province"),
                    Terrain = region.Value<string>("terrain"),
                    Structures = new List<DbStructure>(),
                    Units = new List<DbUnit>()
                };
                r.Memory = regionMemory.TryGetValue(r.EmpheralId, out var regMem) ? regMem : null;

                var units = region["units"] as JArray;
                if (units != null) AddUnits(game, turn, units, r, null, unitMemory, orders);

                var structures = region["structures"] as JArray;
                if (structures != null) AddStructures(game, turn, structures, r, structuresMemory, unitMemory, orders);

                region.Remove("units");
                region.Remove("structures");
                r.Json = region.ToString();

                turn.Regions.Add(r);
            }

            if (game.PlayerFactionNumber == ordersTemplate.Value<int>("faction")) {
                game.Password = ordersTemplate.Value<string>("password");
            }

            await db.SaveChangesAsync();
        }

        private void AddStructures(DbGame game, DbTurn turn, JArray structures, DbRegion region, Dictionary<string, string> structuresMemory, Dictionary<int, string> unitMemory, Dictionary<int, string> orders)
        {
            int structureOrder = 0;
            foreach (JObject structure in structures)
            {
                var s = new DbStructure {
                    GameId = game.Id,
                    TurnId = turn.Id,
                    Sequence = structureOrder,
                    Number = structure.Value<int>("number"),
                    Type = structure.Value<string>("type"),
                    Name = structure.Value<string>("name"),
                    Units = new List<DbUnit>()
                };
                s.Memory = structuresMemory.TryGetValue(s.EmpheralId, out var structMem)
                    ? structMem
                    : null;

                var units = structure["units"] as JArray;
                if (units != null) AddUnits(game, turn, units, region, s, unitMemory, orders);

                structure.Remove("units");
                s.Json = structure.ToString();

                region.Structures.Add(s);
                turn.Structures.Add(s);

                structureOrder++;
            }
        }

        private static void AddUnits(DbGame game, DbTurn turn, JArray units, DbRegion region, DbStructure structure, Dictionary<int, string> unitMemory, Dictionary<int, string> orders)
        {
            int unitOrder = 0;
            foreach (JObject unit in units)
            {
                var unitFaction = unit["faction"] as JObject;
                var u = new DbUnit
                {
                    GameId = game.Id,
                    TurnId = turn.Id,
                    Number = unit.Value<int>("number"),
                    Name = unit.Value<string>("name"),
                    FactionNumber = unitFaction != null
                        ? unitFaction.Value<int>("number")
                        : (int?)null,
                    Sequence = unitOrder
                };

                u.Own = game.PlayerFactionNumber == u.FactionNumber;
                u.Memory = u.Own && unitMemory.TryGetValue(u.Number, out var unitMem)
                    ? unitMem
                    : null;
                u.Orders = orders.TryGetValue(u.Number, out var unitOrders)
                    ? unitOrders
                    : null;

                unit["own"] = u.Own;    // because could be loaded several reports from different factions
                u.Json = unit.ToString();

                region.Units.Add(u);
                turn.Units.Add(u);
                if (structure != null) structure.Units.Add(u);

                unitOrder++;
            }
        }

        private int MonthToNumber(string monthName) {
            switch (monthName.ToLowerInvariant()) {
                case "january": return 1;
                case "february": return 2;
                case "march": return 3;
                case "april": return 4;
                case "may": return 5;
                case "june": return 6;
                case "july": return 7;
                case "august": return 8;
                case "september": return 9;
                case "october": return 10;
                case "november": return 11;
                case "december": return 12;
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}
