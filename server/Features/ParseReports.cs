namespace advisor.Features {
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using AutoMapper;
    using MediatR;
    using Newtonsoft.Json;
    using advisor.Model;
    using advisor.Persistence;

    using RegionDic = System.Collections.Generic.Dictionary<string, Persistence.DbRegion>;
    using FactionsDic = System.Collections.Generic.Dictionary<int, Persistence.DbFaction>;
    using UnitsDic = System.Collections.Generic.Dictionary<int, Persistence.DbUnit>;
    using StructuresDic = System.Collections.Generic.Dictionary<string, Persistence.DbStructure>;

    public record ParseReports(long PlayerId, int EarliestTurn) : IRequest {

    }

    public class ParseReportsHandler : IRequestHandler<ParseReports> {
        public ParseReportsHandler(Database db, IMapper mapper, IMediator mediator) {
            this.db = db;
            this.mapper = mapper;
            this.mediator = mediator;
        }

        private readonly Database db;
        private readonly IMapper mapper;
        private readonly IMediator mediator;

        public const string DEFAULT_LEVEL_LABEL = "surface";
        public const int DEFAULT_LEVEL_Z = 1;

        public async Task<Unit> Handle(ParseReports request, CancellationToken cancellationToken) {
            RegionDic regions = new RegionDic();
            FactionsDic factions = new FactionsDic();
            UnitsDic units = new UnitsDic();
            StructuresDic structures = new StructuresDic();

            var turn = await db.Turns
                .AsSplitQuery()
                .Include(x => x.Regions)
                .Include(x => x.Factions)
                .Include(x => x.Events)
                .Include(x => x.Structures)
                .Include(x => x.Units)
                .SingleOrDefaultAsync(x => x.PlayerId == request.PlayerId && x.Number == request.EarliestTurn);

            if (turn == null) {
                // when llading prev turn data, do not include ships and units as their location is transient
                turn = await db.Turns
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(x => x.Regions)
                    .Include(x => x.Factions)
                    .Include(x => x.Events)
                    .Include(x => x.Structures.Where(s => s.Number <= GameConsts.MAX_BUILDING_NUMBER))
                    .Where(x => x.Number < request.EarliestTurn)
                    .OrderByDescending(x => x.Number)
                    .FirstOrDefaultAsync();

                if (turn != null) {
                    regions = turn.Regions.ToDictionary(k => k.UID, v => mapper.Map<DbRegion>(v));

                    structures = regions.Values
                        .SelectMany(x => x.Structures)
                        .ToDictionary(x => x.UID);

                    factions = turn.Factions.ToDictionary(k => k.Number, v => mapper.Map<DbFaction>(v));
                }
            }
            else {
                regions = turn.Regions.ToDictionary(k => k.UID);
                structures = turn.Structures.ToDictionary(x => x.UID);
                factions = turn.Factions.ToDictionary(k => k.Number);
                units = turn.Units.ToDictionary(x => x.Number);
            }

            var turns = await db.Turns
                .Where(x => x.PlayerId == request.PlayerId && x.Number >= request.EarliestTurn)
                .OrderBy(x => x.Number)
                .Select(x => new { x.Id, x.Number })
                .ToListAsync();
            var lastTurnNumber = turns.Last().Number;

            var player = await db.Players.FindAsync(request.PlayerId);

            for (var i = 0; i < turns.Count; i++) {
                var t = turns[i];
                var report = await MergeReportsAsync(db, request.PlayerId, player.FactionNumber, t.Id, t.Number, regions, factions, structures, units);

                // update player password if latest turn or newer
                if (t.Number >= lastTurnNumber && report.OrdersTemplate?.Password != null) {
                    player.Password = report.OrdersTemplate.Password;
                }

                await InsertFactionsAsync(db, turn.Id, factions.Values);
                await InsertRegionsAsync(db, turn.Id, regions.Values);

                await db.SaveChangesAsync();

                if (i < (turns.Count - 1)) {
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

            return Unit.Value;
        }

        private static async Task<JReport> MergeReportsAsync(Database db, long playerId, int? playerFactionNumber, long turnId, int turnNumber,
            RegionDic regions, FactionsDic factions, StructuresDic structures, UnitsDic units) {

            var otherReports = await db.Reports
                .AsNoTracking()
                .Where(x => x.PlayerId == playerId && x.TurnId == turnId)
                .ToListAsync();

            DbReport ownReport;
            if (playerFactionNumber.HasValue) {
                var i = otherReports.FindIndex(x => x.FactionNumber == playerFactionNumber);
                ownReport = otherReports[i];
                otherReports.RemoveAt(i);
            }
            else {
                ownReport = otherReports[0];
                otherReports.RemoveAt(0);
            }

            JReport report = await GetJsonReportAsync(ownReport);
            foreach (var r in otherReports) {
                var otherReport = await GetJsonReportAsync(r);
                report.Merge(otherReport);
            }

            CreateOrUpdateFaction(factions, report.Faction);

            foreach (var region in report.Regions) {
                CreateOrUpdateRegion(db, turnNumber, factions, regions, structures, units, region);
            }

            AddEvents(factions, regions, report);
            AddRevealedRegionsFromExits(turnNumber, regions);

            return report;
        }

        public static async Task<JReport> GetJsonReportAsync(DbReport rec) {
            if (rec.Json == null) {
                using var textReader = new StringReader(rec.Source);
                using var atlantisReader = new AtlantisReportJsonConverter(textReader);

                var json = await atlantisReader.ReadAsJsonAsync();
                rec.Json = json.ToString(Formatting.None);

                return json.ToObject<JReport>();
            }

            var report = JsonConvert.DeserializeObject<JReport>(rec.Json);
            return report;
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

        public static void AddEvents(FactionsDic factions, RegionDic regions, JReport report) {
            var faction = factions[report.Faction.Number];

            if (faction.Events.Count > 0) return;

            foreach (var error in report.Errors) {
                faction.Events.Add(new DbEvent {
                    Type = EventType.Error,
                    Message = error
                });
            }

            foreach (var ev in report.Events) {
                var region = ev.Coords == null
                    ? null
                    : regions[DbRegion.GetUID(ev.Coords.X, ev.Coords.Y, ev.Coords.Z ?? DEFAULT_LEVEL_Z)];

                var dbEv = new DbEvent {
                    Type = EventType.Info,
                    Category = ev.Category,
                    Amount = ev.Amount,
                    ItemCode = ev.Code,
                    ItemName = ev.Name,
                    ItemPrice = ev.Price,
                    Message = ev.Message,
                    Region = region
                };

                faction.Events.Add(dbEv);
            }
        }

        private static DbRegion CreateOrUpdateRegion(Database db, int turnNumber, FactionsDic factions, RegionDic regions, StructuresDic structures,
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

            region.Explored = true;
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
            region.Tax = source.Tax;
            region.Entertainment = source.Entertainment;
            region.Wages = source.Wages;
            region.TotalWages = source.TotalWages;

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
                    if (str.Id != default(long)) {
                        db.Remove(str);
                    }

                    structures.Remove(strUid);
                    region.Structures.RemoveAt(i);
                }
            }

            return region;
        }

        private static DbStructure CreateOrUpdateStructure(StructuresDic structures, DbRegion region, JStructure source, int seq) {
            string uid = DbStructure.GetUID(source.Structure.Number, region.X, region.Y, region.Z);
            if (!structures.TryGetValue(uid, out var str)) {
                str = new DbStructure();
                str.X = region.X;
                str.Y = region.Y;
                str.Z = region.Z;

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

            if (info.Flags.Count > 0) {
                str.Flags.Clear();
                str.Flags.AddRange(info.Flags);
            }

            if (info.SailDirections.Count > 0) {
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
            unit.Flags = source.Flags.ToList();
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

        private static async Task InsertRegionsAsync(Database db, long turnId, IEnumerable<DbRegion> regions) {
            foreach (var region in regions) {
                region.TurnId = turnId;

                foreach (var unit in region.Units) {
                    unit.TurnId = turnId;
                }

                foreach (var str in region.Structures) {
                    str.TurnId = turnId;
                }

                if (region.Id != default(long)) continue;

                await db.AddAsync(region);
            }
        }

        private static async Task InsertFactionsAsync(Database db, long turnId, IEnumerable<DbFaction> factions) {
            foreach (var faction in factions) {
                faction.TurnId = turnId;

                foreach (var ev in faction.Events) {
                    ev.TurnId = turnId;
                }

                if (faction.Id != default(long)) continue;

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

                region.Explored = false;
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
