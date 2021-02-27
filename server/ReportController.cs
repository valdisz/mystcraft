namespace atlantis
{
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
    using Model;
    using AutoMapper;
    using System.Diagnostics;

    using RegionDic = System.Collections.Generic.Dictionary<string, Persistence.DbRegion>;
    using FactionsDic = System.Collections.Generic.Dictionary<int, Persistence.DbFaction>;
    using UnitsDic = System.Collections.Generic.Dictionary<int, Persistence.DbUnit>;
    using StructuresDic = System.Collections.Generic.Dictionary<string, Persistence.DbStructure>;

    [Route("report")]
    public class ApiController : ControllerBase {
        public ApiController(Database db, IIdSerializer relayId, IMapper mapper) {
            this.db = db;
            this.relayId = relayId;
            this.mapper = mapper;
        }

        private readonly Database db;
        private readonly IIdSerializer relayId;
        private readonly IMapper mapper;

        [HttpPost("{gameId}")]
        public async Task<IActionResult> UploadReports([Required, FromRoute] string gameId) {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (Request.Form.Files.Count == 0) return UnprocessableEntity();

            DbGame game = await FindGameAsync(db, relayId, gameId);
            if (game == null) return NotFound();

            // 1. upload reports
            int earliestTurn = int.MaxValue;
            List<(int, JReport)> reports = new List<(int, JReport)>();
            foreach (var file in Request.Form.Files) {
                await using var stream = file.OpenReadStream();
                var (turnNumber, report) = await LoadReportFileAsync(db, stream, game);

                earliestTurn = Math.Min(earliestTurn, turnNumber);
                reports.Add((earliestTurn, report));
            }
            await db.SaveChangesAsync();

            // 2. parse & merge reports and update history
            await UpdateTurnsAsync(mapper, db, game.Id, earliestTurn, reports);

            return Ok();
        }

        public const string DEFAULT_LEVEL_LABEL = "surface";
        public const int DEFAULT_LEVEL_Z = 1;

        private static Task<DbGame> FindGameAsync(Database db, IIdSerializer relayId, string gameId) {
            var id = (long) relayId.Deserialize(gameId).Value;

            return db.Games.SingleOrDefaultAsync(x => x.Id == id);
        }

        private static async Task<(int turnNumber, JReport report)> LoadReportFileAsync(Database db, Stream reportStream, DbGame game) {
            using var textReader = new StreamReader(reportStream);

            using var atlantisReader = new AtlantisReportJsonConverter(textReader);
            var json = await atlantisReader.ReadAsJsonAsync();
            var report  = json.ToObject<JReport>();

            var faction = report.Faction;
            var date = report.Date;
            var engine = report.Engine;
            var ordersTemplate = report.OrdersTemplate;

            string factionName = faction.Name;
            int factionNumber = faction.Number;
            int year = date.Year;
            int month = date.Month;
            int turnNumber = month + (year - 1) * 12;

            reportStream.Seek(0, SeekOrigin.Begin);
            var source = await textReader.ReadToEndAsync();

            var turn = await db.Turns
                .FirstOrDefaultAsync(x => x.GameId == game.Id && x.Number == turnNumber);
            DbReport dbReport = null;

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
                dbReport = await db.Reports
                    .FirstOrDefaultAsync(x => x.FactionNumber == factionNumber && x.TurnId == turn.Id);
            }

            if (dbReport == null) {
                dbReport = new DbReport {
                    GameId = game.Id,
                    TurnId = turn.Id
                };

                game.Reports.Add(dbReport);
                turn.Reports.Add(dbReport);
            }

            dbReport.FactionNumber = factionNumber;
            dbReport.FactionName = factionName;
            dbReport.Source = source;
            dbReport.Json = json.ToString(Formatting.Indented);

            game.PlayerFactionNumber ??= dbReport.FactionNumber;
            if (dbReport.FactionNumber == game.PlayerFactionNumber && turnNumber > game.LastTurnNumber) {
                game.PlayerFactionName = dbReport.FactionName;
                game.LastTurnNumber = turnNumber;
            }

            game.EngineVersion ??= engine?.Version;
            game.RulesetName ??= engine?.Ruleset?.Name;
            game.RulesetVersion ??= engine?.Ruleset?.Version;
            game.Password ??= ordersTemplate?.Password;

            return (turnNumber, report);
        }

        private static async Task UpdateTurnsAsync(IMapper mapper, Database db, long gameId, int earliestTurn, List<(int, JReport)> reports) {
            var lastTurn = await db.Turns
                .AsNoTracking()
                .OrderByDescending(x => x.Number)
                .Where(x => x.GameId == gameId)
                .Select(x => x.Number)
                .FirstOrDefaultAsync();

            if (lastTurn > earliestTurn) {
                lastTurn = earliestTurn - 1;
            }

            var turns = await db.Turns
                .AsNoTracking()
                .Where(x => x.GameId == gameId && x.Number >= lastTurn)
                .OrderBy(x => x.Number)
                .Select(x => new { x.Id, x.Number })
                .ToListAsync();

            RegionDic regions = new RegionDic();
            FactionsDic factions = new FactionsDic();
            UnitsDic units = new UnitsDic();
            StructuresDic structures = new StructuresDic();

            if (earliestTurn > 1) {
                regions = (await db.Regions
                        .AsNoTracking()
                        .Include(x => x.Structures.Where(s => s.Number <= GameConsts.MAX_BUILDING_NUMBER))
                        .Where(x => x.TurnId == turns[0].Id)
                        .ToArrayAsync())
                    .ToDictionary(k => k.UID, v => mapper.Map<DbRegion>(v));

                structures = regions.Values
                    .SelectMany(x => x.Structures)
                    .ToDictionary(x => x.UID);

                factions = (await db.Factions
                        .AsNoTracking()
                        .Where(x => x.TurnId == turns[0].Id)
                        .ToArrayAsync())
                    .ToDictionary(k => k.Number, v => mapper.Map<DbFaction>(v));
            }

            foreach (var turn in turns) {
                await MergeReportsAsync(db, reports, turn.Id, turn.Number, regions, factions, structures, units);

                await RemoveEventsAsync(db, turn.Id);
                await RemoveUnitsAsync(db, turn.Id);
                await RemoveStructuresAsync(db, turn.Id);
                await RemoveFactionsAsync(db, turn.Id);
                await RemoveRegionsAsync(db, turn.Id);

                await InsertFactionsAsync(db, turn.Id, factions.Values);
                await InsertRegionsAsync(db, turn.Id, regions.Values);

                await db.SaveChangesAsync();

                foreach (var ( key, value ) in regions) {
                    regions[key] = mapper.Map<DbRegion>(value);
                }

                foreach (var ( key, value ) in factions) {
                    factions[key] = mapper.Map<DbFaction>(value);
                }

                units.Clear();

                structures = regions.Values
                    .SelectMany(x => x.Structures)
                    .ToDictionary(x => x.UID);
            }
        }

        // This method will be memory intensive as it must merge all reports into one
        private static async Task MergeReportsAsync(Database db, List<(int, JReport)> reports, long turnId, int turnNumber,
            RegionDic regions, FactionsDic factions, StructuresDic structures, UnitsDic units) {

            var savedReports = db.Reports
                .Include(x => x.Turn)
                .Where(x => x.TurnId == turnId)
                .AsNoTracking()
                .AsAsyncEnumerable();

            await foreach (var r in savedReports) {
                var report = JsonConvert.DeserializeObject<JReport>(r.Json);

                CreateOrUpdateFaction(factions, report);

                foreach (var region in report.Regions) {
                    CreateOrUpdateRegion(turnNumber, factions, regions, structures, units, region);
                }
            }

            AddRevealedRegionsFromExits(turnNumber, regions);
        }

        public static DbFaction CreateOrUpdateFaction(FactionsDic factions, JFaction f) {
            if (!factions.TryGetValue(f.Number, out var faction)) {
                faction = new DbFaction {
                    Number = f.Number
                };
                factions.Add(faction.Number, faction);
            }

            faction.Name = f.Name;

            return faction;
        }

        public static DbFaction CreateOrUpdateFaction(FactionsDic factions, JReport report) {
            var f = report.Faction;

            var faction = CreateOrUpdateFaction(factions, f);

            foreach (var error in report.Errors) {
                faction.Events.Add(new DbEvent {
                    Type = EventType.Error,
                    Message = error
                });
            }

            foreach (var ev in report.Events) {
                faction.Events.Add(new DbEvent {
                    Type = EventType.Info,
                    Message = ev
                });
            }

            return faction;
        }

        private static DbRegion CreateOrUpdateRegion(int turnNumber, FactionsDic factions, RegionDic regions, StructuresDic structures,
            UnitsDic units, JRegion source) {
            var x = source.Coords.X;
            var y = source.Coords.Y;
            var z = source.Coords.Z ?? DEFAULT_LEVEL_Z;

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
            region.Label = source.Coords.Label ?? DEFAULT_LEVEL_LABEL;
            region.Province = source.Province;
            region.Terrain = source.Terrain;

            if (source.Settlement != null) {
                region.Settlement ??= new DbSettlement();
                region.Settlement.Name = source.Settlement.Name;
                region.Settlement.Size = source.Settlement.Size;
            }
            else {
                region.Settlement = null;
            }

            region.Population = source.Population?.Amount ?? 0;
            region.Race = source.Population?.Race;
            region.Tax = source.Tax ?? 0;
            region.Entertainment = source.Entertainment ?? 0;
            region.Wages = source.Wages;
            region.TotalWages = source.TotalWages ?? 0;

            SetItems(region.Products, source.Products);
            SetMarketItems(region.Wanted, source.Wanted);
            SetMarketItems(region.ForSale, source.ForSale);
            SetExits(region.Exits, source.Exits);

            int unitSeq = 0;
            foreach (var unit in source.Units) {
                CreateOrUpdateUnit(factions, units, region, null, unit, unitSeq++);
            }

            int structureSeq = 0;
            HashSet<string> presentStructures = new HashSet<string>();
            foreach (var str in source.Structures) {
                var dbStr = CreateOrUpdateStructure(structures, region, str, structureSeq++);
                presentStructures.Add(dbStr.UID);

                foreach (var unit in str.Units) {
                    var strUnit = CreateOrUpdateUnit(factions, units, region, dbStr, unit, unitSeq++);
                }
            }

            // remove structures that are not present anymore
            for (int i = region.Structures.Count - 1; i >= 0 ; i--) {
                var str = region.Structures[i];
                var strUid = str.UID;

                if (!presentStructures.Contains(strUid)) {
                    structures.Remove(strUid);

                    if (str.X == region.X && str.Y == region.Y && str.Z == region.Z) {
                        // location of the structure not changed then structure does not exist anymore
                        region.Structures.RemoveAt(i);
                    }
                }
            }

            return region;
        }

        private static DbStructure CreateOrUpdateStructure(StructuresDic structures, DbRegion region, JStructure source, int seq) {
            string uid = DbStructure.GetUID(source.Structure.Number, region.X, region.Y, region.Z);
            if (!structures.TryGetValue(uid, out var str)) {
                str = new DbStructure();
                structures.Add(uid, str);
                region.Structures.Add(str);
            }

            var info = source.Structure;

            str.X = region.X;
            str.Y = region.Y;
            str.Z = region.Z;
            str.Sequence = seq;
            str.Number = info.Number;
            str.Name = info.Name;
            str.Type = info.Type;
            str.Description = info.Description;

            str.Speed = info.Speed ?? str.Speed;
            str.Needs = info.Needs ?? str.Needs;

            if (info.Load != null) {
                str.Load ??= new DbTransportationLoad();
                str.Load.Used = info.Load.Used;
                str.Load.Max = info.Load.Max;
            }

            if (info.Sailors != null) {
                str.Sailors ??= new DbSailors();
                str.Sailors.Current = info.Sailors.Current;
                str.Sailors.Required = info.Sailors.Required;
            }

            SetFleetContent(str.Contents, info.Contents);

            if (info.Flags.Length > 0) {
                str.Flags.Clear();
                str.Flags.AddRange(info.Flags);
            }

            if (info.SailDirections.Length > 0) {
                str.SailDirections.Clear();
                str.SailDirections.AddRange(info.SailDirections);
            }

            return str;
        }

        private static DbUnit CreateOrUpdateUnit(FactionsDic factions, UnitsDic units, DbRegion region, DbStructure structure, JUnit source, int seq) {
            if (!units.TryGetValue(source.Number, out var unit)) {
                unit = new DbUnit {
                    Number = source.Number
                };

                units.Add(unit.Number, unit);
                region.Units.Add(unit);
                if (structure != null) structure.Units.Add(unit);
            }

            if (source.Faction != null && unit.Faction == null) {
                var faction = CreateOrUpdateFaction(factions, source.Faction);
                faction.Units.Add(unit);
                unit.Faction = faction;
            }

            unit.Sequence = seq;
            unit.Name = source.Name;
            unit.Description = source.Description;
            unit.OnGuard = source.OnGuard;
            unit.Flags = unit.Flags.Union(source.Flags).ToList();
            unit.Weight = source.Weight;

            if (source.Capacity != null) {
                unit.Capacity = new DbCapacity {
                    Walking = source.Capacity.Walking,
                    Swimming = source.Capacity.Swimming,
                    Riding = source.Capacity.Riding,
                    Flying = source.Capacity.Flying
                };
            }

            if (source.ReadyItem != null) {
                unit.ReadyItem = new DbItem {
                    Code = source.ReadyItem.Code,
                    Amount = source.ReadyItem.Amount
                };
            }

            if (source.CombatSpell != null) {
                unit.CombatSpell = new DbSkill {
                    Code = source.CombatSpell.Code,
                    Level = source.CombatSpell.Level,
                    Days = source.CombatSpell.Days
                };
            }

            SetItems(unit.Items, source.Items);
            SetSkills(unit.Skills, source.Skills);
            SetSkills(unit.CanStudy, source.CanStudy);

            return unit;
        }

        private static Task RemoveRegionsAsync(Database db, long turnId) {
            return db.Database
                .ExecuteSqlRawAsync($"delete from {db.Model.FindEntityType(typeof(DbRegion)).GetTableName()} where TurnId = {turnId}");
        }

        private static Task RemoveFactionsAsync(Database db, long turnId) {
            return db.Database
                .ExecuteSqlRawAsync($"delete from {db.Model.FindEntityType(typeof(DbFaction)).GetTableName()} where TurnId = {turnId}");
        }

        private static Task RemoveEventsAsync(Database db, long turnId) {
            return db.Database
                .ExecuteSqlRawAsync($"delete from {db.Model.FindEntityType(typeof(DbEvent)).GetTableName()} where TurnId = {turnId}");
        }

        private static Task RemoveUnitsAsync(Database db, long turnId) {
            return db.Database
                .ExecuteSqlRawAsync($"delete from {db.Model.FindEntityType(typeof(DbUnit)).GetTableName()} where TurnId = {turnId}");
        }

        private static Task RemoveStructuresAsync(Database db, long turnId) {
            return db.Database
                .ExecuteSqlRawAsync($"delete from {db.Model.FindEntityType(typeof(DbStructure)).GetTableName()} where TurnId = {turnId}");
        }

        private static async Task InsertRegionsAsync(Database db, long turnId, IEnumerable<DbRegion> regions) {
            foreach (var region in regions) {
                region.TurnId = turnId;

                Debug.Assert(region.Exits.Count <= 6);
                foreach (var exit in region.Exits) {
                    Debug.Assert((int) exit.Direction >= 1 && (int) exit.Direction <= 6);
                }

                foreach (var unit in region.Units) {
                    unit.TurnId = turnId;
                }

                foreach (var str in region.Structures) {
                    str.TurnId = turnId;
                }

                await db.AddAsync(region);
            }
        }

        private static async Task InsertFactionsAsync(Database db, long turnId, IEnumerable<DbFaction> factions) {
            foreach (var faction in factions) {
                faction.TurnId = turnId;

                foreach (var ev in faction.Events) {
                    ev.TurnId = turnId;
                }

                await db.AddAsync(faction);
            }
        }

        public static void SetItems(ICollection<DbItem> dbItems, IEnumerable<JItem> items) {
            var dest = dbItems.ToDictionary(x => x.Code);

            foreach (var item in items) {
                if (!dest.TryGetValue(item.Code, out var d)) {
                    d = new DbItem();
                    dbItems.Add(d);
                }

                d.Code = item.Code;
                d.Amount = item.Amount;
            }
        }

        public static void SetMarketItems(ICollection<DbTradableItem> dbItems, IEnumerable<JTradableItem> items) {
            var dest = dbItems.ToDictionary(x => x.Code);

            foreach (var item in items) {
                if (!dest.TryGetValue(item.Code, out var d)) {
                    d = new DbTradableItem();
                    dbItems.Add(d);
                }

                d.Code = item.Code;
                d.Amount = item.Amount ?? 1;
                d.Price = item.Price;
            }

            // missing items are removed
            foreach (var key in dest.Keys.Except(items.Select(x => x.Code))) {
                dbItems.Remove(dest[key]);
            }
        }

        public static void SetSkills(ICollection<DbSkill> dbItems, IEnumerable<JSkill> items) {
            var dest = dbItems.ToDictionary(x => x.Code);

            foreach (var item in items) {
                if (!dest.TryGetValue(item.Code, out var d)) {
                    d = new DbSkill();
                    dbItems.Add(d);
                }

                d.Code = item.Code;
                d.Level = item.Level;
                d.Days = item.Days;
            }
        }

        public static void SetFleetContent(ICollection<DbFleetContent> dbItems, IEnumerable<JFleetContent> items) {
            var dest = dbItems.ToDictionary(x => x.Type);

            foreach (var item in items) {
                var type = item.Type.TrimEnd('s');
                if (!dest.TryGetValue(type, out var d)) {
                    d = new DbFleetContent {
                        Type = type
                    };

                    dbItems.Add(d);
                }

                d.Count = item.Count;
            }
        }

        public static void SetExits(ICollection<DbExit> dbItems, IEnumerable<JExit> items) {
            var dest = dbItems.ToDictionary(x => x.RegionUID);
            foreach (var item in items) {
                var uid = DbRegion.GetUID(item.Coords.X, item.Coords.Y, item.Coords.Z ?? 1);
                if (!dest.TryGetValue(uid, out var d)) {
                    d = new DbExit();
                    dbItems.Add(d);
                }

                d.Direction = item.Direction;
                d.X = item.Coords.X;
                d.Y = item.Coords.Y;
                d.Z = item.Coords.Z ?? DEFAULT_LEVEL_Z;
                d.Label = item.Coords.Label ?? DEFAULT_LEVEL_LABEL;
                d.Province = item.Province;
                d.Terrain = item.Terrain;

                if (item.Settlement != null) {
                    d.Settlement ??= new DbSettlement();
                    d.Settlement.Name = item.Settlement.Name;
                    d.Settlement.Size = item.Settlement.Size;
                }
                else {
                    d.Settlement = null;
                }
            }
        }

        public static void AddRevealedRegionsFromExits(int turnNumber, RegionDic regions) {
            var exits = regions.Values
                .SelectMany(x => x.Exits)
                .GroupBy(x => x.RegionUID)
                .ToDictionary(x => x.Key, x => x.First());

            foreach (var uid in exits.Keys.Except(regions.Keys)) {
                var exit = exits[uid];

                DbRegion region = new DbRegion();
                regions.Add(uid, region);

                region.UpdatedAtTurn = turnNumber;
                region.X = exit.X;
                region.Y = exit.Y;
                region.Z = exit.Z;
                region.Label = exit.Label;
                region.Province = exit.Province;
                region.Terrain = exit.Terrain;
            }
        }
    }
}
