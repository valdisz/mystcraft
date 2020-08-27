namespace atlantis {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using atlantis.Persistence;
    using HotChocolate.Types.Relay;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using RegionDic = System.Collections.Generic.Dictionary<string, Persistence.DbRegion>;

    public class JsonRegion {
        public string Terrain;
        public JsonCoords Coords;
        public string Province;
    }

    public class JsonCoords {
        public int X;
        public int Y;
        public int? Z;
        public string Label;
    }

    [Route("report")]
    public class ApiController : ControllerBase {
        public ApiController(Database db, IIdSerializer relayId) {
            this.db = db;
            this.relayId = relayId;
        }

        private readonly Database db;
        private readonly IIdSerializer relayId;

        [HttpPost("{gameId}")]
        public async Task<IActionResult> UploadReports([Required, FromRoute] string gameId) {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (Request.Form.Files.Count == 0) return UnprocessableEntity();

            int earliestTurn = int.MaxValue;
            DbGame game;
            if ((game = await FindGameAsync(db, relayId, gameId)) == null) return NotFound();

            foreach (var file in Request.Form.Files) {
                await using var stream = file.OpenReadStream();
                var turnNumber = await LoadReportFileAsync(db, stream, game);

                earliestTurn = Math.Min(earliestTurn, turnNumber);
            }
            await db.SaveChangesAsync();

            await UpdateTurnsAsync(db, game.Id, earliestTurn);

            return Ok();
        }

        private static Task<DbGame> FindGameAsync(Database db, IIdSerializer relayId, string gameId) {
            var id = (long) relayId.Deserialize(gameId).Value;

            return db.Games.SingleAsync(x => x.Id == id);
        }

        private static async Task<int> LoadReportFileAsync(Database db, Stream reportStream, DbGame game) {
            using var textReader = new StreamReader(reportStream);

            using var atlantisReader = new AtlantisReportJsonConverter(textReader);
            var jObj = await atlantisReader.ReadAsJsonAsync();
            dynamic json = jObj;

            string factionName = json.faction.name;
            int factionNumber = json.faction.number;
            int year = json.date.year;
            int month = json.date.month;
            int turnNumber = month + (year - 1) * 12;

            reportStream.Seek(0, SeekOrigin.Begin);
            var source = await textReader.ReadToEndAsync();

            var turn = await db.Turns
                .FirstOrDefaultAsync(x => x.GameId == game.Id && x.Number == turnNumber);
            DbReport report = null;

            if (turn == null) {
                turn = new DbTurn {
                    GameId = game.Id,
                    Month = month,
                    Year = year,
                    Number = turnNumber
                };

                game.Turns.Add(turn);
            }
            else {
                report = await db.Reports
                    .FirstOrDefaultAsync(x => x.FactionNumber == factionNumber && x.TurnId == turn.Id);
            }

            if (report == null) {
                report = new DbReport {
                    GameId = game.Id,
                    TurnId = turn.Id
                };

                game.Reports.Add(report);
                turn.Reports.Add(report);
            }

            report.FactionNumber = factionNumber;
            report.FactionName = factionName;
            report.Source = source;
            report.Json = jObj.ToString(Formatting.Indented);

            game.PlayerFactionNumber ??= report.FactionNumber;
            if (report.FactionNumber == game.PlayerFactionNumber && turnNumber > game.LastTurnNumber) {
                game.PlayerFactionName = report.FactionName;
                game.LastTurnNumber = turnNumber;
            }

            game.EngineVersion ??= json.engine?.version;
            game.RulesetName ??= json.engine?.ruleset?.name;
            game.RulesetVersion ??= json.engine?.ruleset?.version;
            game.Password ??= json.ordersTemplate?.password;

            return turnNumber;
        }

        private static async Task UpdateTurnsAsync(Database db, long gameId, int earliestTurn) {
            var lastTurn = await db.Turns
                .OrderByDescending(x => x.Number)
                .Where(x => x.GameId == gameId)
                .Select(x => x.Number)
                .FirstOrDefaultAsync();

            if (lastTurn > earliestTurn) {
                lastTurn = earliestTurn - 1;
            }

            var turns = await db.Turns
                .Where(x => x.GameId == gameId && x.Number >= lastTurn)
                .OrderBy(x => x.Number)
                .Select(x => new { x.Id, x.Number })
                .AsNoTracking()
                .ToListAsync();

            RegionDic regions = new RegionDic();
            if (earliestTurn > 1) {
                regions = (await db.Regions
                        .Where(x => x.TurnId == turns[0].Id)
                        .AsNoTracking()
                        .ToArrayAsync())
                    .ToDictionary(k => k.UID);
            }

            foreach (var turn in turns) {
                regions = await UpdateTurnAsync(db, turn.Id, turn.Number, regions);

                await RemoveRegions(db, turn.Id);
                await InsertRegions(db, turn.Id, regions.Values);
            }
        }

        private static async Task<RegionDic> UpdateTurnAsync(Database db, long turnId, int turnNumber, RegionDic regionsIn) {
            RegionDic regions = regionsIn
                .ToDictionary(k => k.Key, v => CopyRegion(v.Value));

            await foreach (var report in db.Reports.Where(x => x.TurnId == turnId).AsNoTracking().AsAsyncEnumerable()) {
                var json = JObject.Parse(report.Json);
                var jsonRegions = json["regions"] as JArray;

                foreach (JObject region in jsonRegions) {
                    CreateOrUpdateRegion(turnNumber, regions, region.ToObject<JsonRegion>());
                }
            }

            return regions;
        }

        private static DbRegion CopyRegion(DbRegion region) {
            return new DbRegion {
                X = region.X,
                Y = region.Y,
                Z = region.Z,
                Label = region.Label,
                UpdatedAtTurn = region.UpdatedAtTurn,
                Province = region.Province,
                Terrain = region.Terrain
            };
        }

        private static void CreateOrUpdateRegion(int turnNumber, RegionDic regions, JsonRegion source) {
            var x = source.Coords.X;
            var y = source.Coords.Y;
            var z = source.Coords.Z ?? 1;

            var uid = DbRegion.GetUID(x, y, z);

            DbRegion region;
            if (!regions.TryGetValue(uid, out region)) {
                region = new DbRegion();
                regions.Add(uid, region);
            }

            region.UpdatedAtTurn = turnNumber;
            region.X = x;
            region.Y = y;
            region.Z = z;
            region.Label = source.Coords.Label ?? "surface";
            region.Province = source.Province;
            region.Terrain = source.Terrain;
        }

        private static Task RemoveRegions(Database db, long turnId) {
            return db.Database
                .ExecuteSqlRawAsync($"delete from {db.Model.FindEntityType(typeof(DbRegion)).GetTableName()} where TurnId = {turnId}");
        }

        private static async Task InsertRegions(Database db, long turnId, IEnumerable<DbRegion> regions) {
            foreach (var region in regions) {
                region.TurnId = turnId;
                await db.AddAsync(region);
            }

            await db.SaveChangesAsync();
        }

        // [HttpGet("games/{gameId}/regions/{turnNumber?}")]
        // public async Task<IActionResult> GetRegionsAsync([Required, FromRoute] long gameId, [FromRoute] int? turnNumber) {
        //     IQueryable<DbTurn> q = db.Turns
        //         .Include(x => x.Regions)
        //         .Where(x => x.GameId == gameId);

        //     if (turnNumber.HasValue) {
        //         q = q.Where(x => x.Number == turnNumber.Value);
        //     }
        //     else {
        //         q = q.OrderByDescending(x => x.Id);
        //     }

        //     var turn = await q.FirstOrDefaultAsync();
        //     if (turn == null) return NotFound();

        //     using var ms = new MemoryStream();
        //     using var bodyWriter = new StreamWriter(ms);
        //     using var writer = new JsonTextWriter(bodyWriter);

        //     writer.WriteStartArray();
        //     foreach (var r in turn.Regions) {
        //         var json = JObject.Parse(r.Json);
        //         json["_id"] = r.Id;
        //         json["_updatedAtTurn"] = r.UpdatedAtTurn;
        //         json.WriteTo(writer);
        //     }
        //     writer.WriteEndArray();

        //     Response.ContentType = "application/json; charset=utf-8";
        //     Response.ContentLength = ms.Length;

        //     ms.Seek(0, SeekOrigin.Begin);
        //     await ms.CopyToAsync(Response.Body, 0x10000);

        //     return Ok();
        // }

        // [HttpPost("games/{gameId}/report")]
        // public async Task UploadReport([Required, FromRoute] long gameId)
        // {
        //     using var bodyReader = new StreamReader(Request.Body);
        //     using var converter = new AtlantisReportJsonConverter(bodyReader);

        //     using var buffer = new MemoryStream();
        //     using var bufferWriter = new StreamWriter(buffer);
        //     using JsonWriter writer = new JsonTextWriter(bufferWriter);
        //     await converter.ConvertAsync(writer);

        //     buffer.Seek(0, SeekOrigin.Begin);

        //     using var bufferReader = new StreamReader(buffer);
        //     using JsonReader reader = new JsonTextReader(bufferReader);

        //     JObject report = await JObject.LoadAsync(reader);
        //     var faction = report["faction"] as JObject;
        //     var date = report["date"] as JObject;
        //     var engine = report["engine"] as JObject;
        //     var factionStatus = report["factionStatus"] as JArray;
        //     var errors = report["errors"] as JArray;
        //     var events = report["events"] as JArray;
        //     var skillReports = report["skillReports"] as JArray;
        //     var objectReports = report["objectReports"] as JArray;
        //     var itemReports = report["itemReports"] as JArray;
        //     var attitudes = report["attitudes"] as JObject;
        //     var unclaimedSilver = report["unclaimedSilver"];
        //     var regions = report["regions"] as JArray;
        //     var ordersTemplate = report["ordersTemplate"] as JObject;

        //     var turn = new DbTurn
        //     {
        //         GameId = gameId,
        //         Year = date.Value<int>("year"),
        //         Month = MonthToNumber(date.Value<string>("month")),
        //         Factions = new List<DbFaction>(),
        //         Regions = new List<DbRegion>(),
        //         Structures = new List<DbStructure>(),
        //         Events = new List<DbEvent>(),
        //         Units = new List<DbUnit>()
        //     };
        //     turn.Number = (turn.Year - 1) * 12 + turn.Month;

        //     // ensure that there are no turn with the same number
        //     await RemoveTurn(gameId, turn.Number);

        //     db.ChangeTracker.AutoDetectChangesEnabled = false;
        //     var lastTurn = await db.Turns
        //         .OrderByDescending(x => x.Id)
        //         .Include(x => x.Factions)
        //         .Include(x => x.Units)
        //         .Include(x => x.Structures)
        //         .Include(x => x.Regions)
        //         .ThenInclude(x => x.Structures)
        //         .FirstOrDefaultAsync(x => x.GameId == gameId);

        //     Dictionary<string, string> regionMemory = lastTurn != null
        //         ? lastTurn.Regions
        //             .ToDictionary(k => k.EmpheralId, v => v.Memory)
        //         : new Dictionary<string, string>();

        //     Dictionary<int, string> unitMemory = lastTurn != null
        //         ? lastTurn.Units
        //             .Where(x => x.Own)
        //             .ToDictionary(k => k.Number, v => v.Memory)
        //         : new Dictionary<int, string>();

        //     Dictionary<string, string> structuresMemory = lastTurn != null
        //         ? lastTurn.Structures
        //             .ToDictionary(k => DbStructure.GetEmpheralId(k.Region.X, k.Region.Y, k.Region.Z, k.Number, k.Type), v => v.Memory)
        //         : new Dictionary<string, string>();

        //     db.ChangeTracker.AutoDetectChangesEnabled = true;
        //     var game = await db.Games.FindAsync(gameId);

        //     turn.Memory = lastTurn?.Memory;

        //     await db.Turns.AddAsync(turn);
        //     await db.SaveChangesAsync();

        //     var f = new DbFaction
        //     {
        //         GameId = gameId,
        //         TurnId = turn.Id,
        //         Name = faction.Value<string>("name"),
        //         Number = faction.Value<int>("number"),
        //         Json = new JObject(
        //             new JProperty("faction", faction),
        //             new JProperty("factionStatus", factionStatus),
        //             new JProperty("skillReports", skillReports),
        //             new JProperty("objectReports", objectReports),
        //             new JProperty("itemReports", itemReports),
        //             new JProperty("attitudes", attitudes),
        //             new JProperty("unclaimedSilver", unclaimedSilver)
        //         ).ToString(),
        //     };

        //     if (!game.PlayerFactionNumber.HasValue)
        //     {
        //         game.PlayerFactionNumber = f.Number;
        //         game.EngineVersion = engine.Value<string>("version");

        //         var ruleset = engine["ruleset"] as JObject;
        //         game.RulesetName = ruleset.Value<string>("name");
        //         game.RulesetVersion = ruleset.Value<string>("version");
        //     }

        //     f.Own = f.Number == game.PlayerFactionNumber;

        //     f.Events = (events ?? Enumerable.Empty<JToken>())
        //         .Select(x => new DbEvent { GameId = gameId, TurnId = turn.Id, Type = "event", Json = x.ToString() })
        //         .Concat(
        //             (errors ?? Enumerable.Empty<JToken>())
        //                 .Select(x => new DbEvent { GameId = gameId, TurnId = turn.Id, Type = "error", Json = x.ToString() })
        //         )
        //         .ToList();

        //     turn.Factions.Add(f);
        //     turn.Events.AddRange(f.Events);

        //     var orders = (ordersTemplate["units"] as JArray)
        //         .ToDictionary(k => k.Value<int>("unit"), v => v.Value<string>("orders"));

        //     foreach (JObject region in regions)
        //     {
        //         var coords = region["coords"] as JObject;

        //         var r = new DbRegion
        //         {
        //             GameId = gameId,
        //             TurnId = turn.Id,
        //             UpdatedAtTurn = turn.Number,
        //             X = coords.Value<int>("x"),
        //             Y = coords.Value<int>("y"),
        //             Z = coords.TryGetValue("z", StringComparison.OrdinalIgnoreCase, out var zCoord)
        //                 ? zCoord.Value<int>()
        //                 : 1,
        //             Label = coords.TryGetValue("label", StringComparison.OrdinalIgnoreCase, out var label)
        //                 ? label.Value<string>()
        //                 : "surface",
        //             Province = region.Value<string>("province"),
        //             Terrain = region.Value<string>("terrain"),
        //             Structures = new List<DbStructure>(),
        //             Units = new List<DbUnit>()
        //         };
        //         r.Memory = regionMemory.TryGetValue(r.EmpheralId, out var regMem) ? regMem : null;

        //         var units = region["units"] as JArray;
        //         if (units != null) AddUnits(game, turn, units, r, null, unitMemory, orders);

        //         var structures = region["structures"] as JArray;
        //         if (structures != null) AddStructures(game, turn, structures, r, structuresMemory, unitMemory, orders);

        //         region.Remove("units");
        //         region.Remove("structures");
        //         r.Json = region.ToString();

        //         turn.Regions.Add(r);
        //     }

        //     if (game.PlayerFactionNumber == ordersTemplate.Value<int>("faction")) {
        //         game.Password = ordersTemplate.Value<string>("password");
        //     }

        //     // copy missing regions from previous turn
        //     if (lastTurn != null) {
        //         CopyInvisibleRegions(gameId, turn, lastTurn, game);
        //     }

        //     await db.SaveChangesAsync();
        // }

        // private static void CopyInvisibleRegions(long gameId, DbTurn turn, DbTurn lastTurn, DbGame game)
        // {
        //     var visibleRegions = turn.Regions.ToDictionary(k => k.EmpheralId);
        //     var allRegions = lastTurn.Regions.ToDictionary(k => k.EmpheralId);

        //     foreach (var key in allRegions.Keys.Except(visibleRegions.Keys))
        //     {
        //         var invisibleRegion = allRegions[key];
        //         var regCopy = new DbRegion
        //         {
        //             GameId = gameId,
        //             TurnId = turn.Id,
        //             UpdatedAtTurn = invisibleRegion.UpdatedAtTurn,
        //             X = invisibleRegion.X,
        //             Y = invisibleRegion.Y,
        //             Z = invisibleRegion.Z,
        //             Label = invisibleRegion.Label,
        //             Province = invisibleRegion.Province,
        //             Terrain = invisibleRegion.Terrain,
        //             Structures = new List<DbStructure>(),
        //             Units = new List<DbUnit>(),
        //             Json = invisibleRegion.Json,
        //             Memory = invisibleRegion.Memory
        //         };

        //         if (invisibleRegion.Structures != null) {
        //             foreach (var str in invisibleRegion.Structures)
        //             {
        //                 var strCopy = new DbStructure
        //                 {
        //                     GameId = game.Id,
        //                     TurnId = turn.Id,
        //                     Sequence = str.Sequence,
        //                     Number = str.Number,
        //                     Type = str.Type,
        //                     Name = str.Name,
        //                     Units = new List<DbUnit>(),
        //                     Json = str.Json
        //                 };

        //                 regCopy.Structures.Add(strCopy);
        //                 turn.Structures.Add(strCopy);
        //             }
        //         }

        //         turn.Regions.Add(regCopy);
        //     }
        // }

        // private async Task RemoveTurn(long gameId, int turnNumber) {
        //     var existingTurn = await db.Turns.FirstOrDefaultAsync(x => x.GameId == gameId && x.Number == turnNumber);
        //     if (existingTurn != null)
        //     {
        //         var tablesToDelete = new[] {
        //             db.Model.FindEntityType(typeof(DbEvent)).GetTableName(),
        //             db.Model.FindEntityType(typeof(DbUnit)).GetTableName(),
        //             db.Model.FindEntityType(typeof(DbFaction)).GetTableName(),
        //             db.Model.FindEntityType(typeof(DbStructure)).GetTableName(),
        //             db.Model.FindEntityType(typeof(DbRegion)).GetTableName()
        //         };

        //         foreach (var table in tablesToDelete)
        //         {
        //             await db.Database.ExecuteSqlRawAsync($"delete from {table} where TurnId = {existingTurn.Id}");
        //         }

        //         db.Remove(existingTurn);
        //         await db.SaveChangesAsync();
        //     }
        // }

        // private void AddStructures(DbGame game, DbTurn turn, JArray structures, DbRegion region, Dictionary<string, string> structuresMemory, Dictionary<int, string> unitMemory, Dictionary<int, string> orders)
        // {
        //     int structureOrder = 0;
        //     foreach (JObject structure in structures)
        //     {
        //         var str = structure["structure"] as JObject;
        //         var s = new DbStructure {
        //             GameId = game.Id,
        //             TurnId = turn.Id,
        //             Sequence = structureOrder,
        //             Number = str.Value<int>("number"),
        //             Type = str.Value<string>("type"),
        //             Name = str.Value<string>("name"),
        //             Units = new List<DbUnit>(),
        //             Json = str.ToString()
        //         };

        //         var emphId = DbStructure.GetEmpheralId(region.X, region.Y, region.Z, s.Number, s.Type);
        //         s.Memory = structuresMemory.TryGetValue(emphId, out var structMem)
        //             ? structMem
        //             : null;

        //         var units = structure["units"] as JArray;
        //         if (units != null) AddUnits(game, turn, units, region, s, unitMemory, orders);

        //         region.Structures.Add(s);
        //         turn.Structures.Add(s);

        //         structureOrder++;
        //     }
        // }

        // private static void AddUnits(DbGame game, DbTurn turn, JArray units, DbRegion region, DbStructure structure, Dictionary<int, string> unitMemory, Dictionary<int, string> orders)
        // {
        //     int unitOrder = 0;
        //     foreach (JObject unit in units)
        //     {
        //         var unitFaction = unit["faction"] as JObject;
        //         var u = new DbUnit
        //         {
        //             GameId = game.Id,
        //             TurnId = turn.Id,
        //             Number = unit.Value<int>("number"),
        //             Name = unit.Value<string>("name"),
        //             FactionNumber = unitFaction != null
        //                 ? unitFaction.Value<int>("number")
        //                 : (int?)null,
        //             Sequence = unitOrder
        //         };

        //         u.Own = game.PlayerFactionNumber == u.FactionNumber;
        //         u.Memory = u.Own && unitMemory.TryGetValue(u.Number, out var unitMem)
        //             ? unitMem
        //             : null;
        //         u.Orders = orders.TryGetValue(u.Number, out var unitOrders)
        //             ? unitOrders
        //             : null;

        //         unit["own"] = u.Own;    // because could be loaded several reports from different factions
        //         u.Json = unit.ToString();

        //         region.Units.Add(u);
        //         turn.Units.Add(u);
        //         if (structure != null) structure.Units.Add(u);

        //         unitOrder++;
        //     }
        // }

        // private int MonthToNumber(string monthName) {
        //     switch (monthName.ToLowerInvariant()) {
        //         case "january": return 1;
        //         case "february": return 2;
        //         case "march": return 3;
        //         case "april": return 4;
        //         case "may": return 5;
        //         case "june": return 6;
        //         case "july": return 7;
        //         case "august": return 8;
        //         case "september": return 9;
        //         case "october": return 10;
        //         case "november": return 11;
        //         case "december": return 12;
        //     }

        //     throw new ArgumentOutOfRangeException();
        // }
    }
}
