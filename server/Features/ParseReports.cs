namespace advisor.Features
{
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
    using UnitOrders = System.Collections.Generic.Dictionary<int, string>;
    using StructuresDic = System.Collections.Generic.Dictionary<string, Persistence.DbStructure>;

    public record ParseReports(long PlayerId, int EarliestTurn, JReport Map = null) : IRequest {

    }

    public class ReportSync : InTurnContext {
        private ReportSync(long playerId, int turnNumber) {
            PlayerId = playerId;
            TurnNumber = turnNumber;
        }

        int InTurnContext.TurnNumber { get => this.TurnNumber; set { } }
        long InPlayerContext.PlayerId { get => this.PlayerId; set { } }

        public long PlayerId { get; }
        public int TurnNumber { get; }
        public RegionDic Regions { get; } = new ();
        public StructuresDic Structures { get; } = new ();
        public UnitsDic Units { get; } = new ();
        public FactionsDic Factions { get; } = new ();

        private void ApplyTurnContext(InTurnContext ctx) {
            ctx.PlayerId = PlayerId;
            ctx.TurnNumber = TurnNumber;
        }

        public static ReportSync Empty(long playerId, int turnNumber) => new ReportSync(playerId, turnNumber);

        public static ReportSync Load(DbTurn turn) {
            var sync = new ReportSync(turn.PlayerId, turn.Number);

            foreach (var region in turn.Regions) {
                sync.Regions.Add(region.Id, region);
            }

            foreach (var str in turn.Structures) {
                sync.Structures.Add(str.Id, str);
            }

            foreach (var fac in turn.Factions) {
                sync.Factions.Add(fac.Number, fac);
            }

            foreach (var unit in turn.Units) {
                sync.Units.Add(unit.Number, unit);
            }

            return sync;
        }

        public static ReportSync Copy(long playerId, int turnNumber, DbTurn turn, IMapper mapper) {
            var sync = new ReportSync(playerId, turnNumber);

            foreach (var region in turn.Regions.Select(mapper.Map<DbRegion>)) {
                region.Units.Clear();
                sync.Regions.Add(region.Id, region);

                foreach (var str in region.Structures.ToList()) {
                    str.Units.Clear();

                    if (str.Number > GameConsts.MAX_BUILDING_NUMBER) {
                        region.Structures.Remove(str);
                        continue;
                    }

                    sync.Structures.Add(str.Id, str);
                }
            }

            foreach (var fac in turn.Factions.Select(mapper.Map<DbFaction>)) {
                fac.Events.Clear();
                fac.Units.Clear();
                fac.Stats.Clear();

                sync.Factions.Add(fac.Number, fac);
            }

            return sync;
        }

        public static ReportSync Copy(long playerId, int turnNumber, ReportSync source, IMapper mapper) {
            var sync = new ReportSync(playerId, turnNumber);

            foreach (var region in source.Regions.Values.Select(mapper.Map<DbRegion>)) {
                region.Units.Clear();
                sync.Regions.Add(region.Id, region);

                foreach (var str in region.Structures.ToList()) {
                    str.Units.Clear();

                    if (str.Number > GameConsts.MAX_BUILDING_NUMBER) {
                        region.Structures.Remove(str);
                        continue;
                    }

                    sync.Structures.Add(str.Id, str);
                }
            }

            foreach (var fac in source.Factions.Values.Select(mapper.Map<DbFaction>)) {
                fac.Events.Clear();
                fac.Units.Clear();
                fac.Stats.Clear();

                sync.Factions.Add(fac.Number, fac);
            }

            return sync;
        }

        public void UpdateWith(DbTurn turn, IMapper mapper) {

        }

        public DbFaction SyncFaction(JFaction source) {
            if (!Factions.TryGetValue(source.Number, out var faction)) {
                faction = new DbFaction {
                    Number = source.Number
                };

                Factions.Add(faction.Number, faction);
            }

            ApplyTurnContext(faction);

            faction.Name = faction.Name;

            return faction;
        }

        public DbUnit SyncUnit(JUnit source, UnitOrders unitOrders, int seq, DbRegion region, DbStructure structure = null) {
            if (!Units.TryGetValue(source.Number, out var unit)) {
                unit = new DbUnit {
                    Number = source.Number
                };

                Units.Add(unit.Number, unit);

                unit.Region = region;
                region.Units.Add(unit);

                if (structure != null) {
                    unit.Structure = structure;
                    structure.Units.Add(unit);
                }
            }

            ApplyTurnContext(unit);

            if (source.Faction != null && unit.Faction == null) {
                var faction = SyncFaction(source.Faction);

                unit.Faction = faction;
                faction.Units.Add(unit);
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

            if (unitOrders.ContainsKey(unit.Number)) {
                unit.Orders = unitOrders[unit.Number];
            }

            return unit;
        }

        public void SyncRegion(JRegion region) {

        }

        public void SyncReport(JReport report) {

        }

        public async Task ApplyChangesAsync(Database db) {
            await ApplyFactionsAsync(db);
            await InsertFactionsAsync(db, t, factions.Values);
            await InsertRegionsAsync(db, t, regions.Values);
        }

        private async Task ApplyFactionsAsync(Database db) {
            foreach (var faction in Factions.Values) {
                faction.TurnId = turnId;

                foreach (var ev in faction.Events) {
                    ev.TurnId = turnId;
                }

                if (faction.Id != default(long)) continue;

                await db.AddAsync(faction);
            }
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
    }

    public record TurnContext : InTurnContext {
        public TurnContext(long playerId, int turnNumber) {
            TurnNumber = turnNumber;
            PlayerId = playerId;
        }

        public TurnContext(TurnContext other) {
            TurnNumber = other.TurnNumber;
            PlayerId = other.PlayerId;
        }

        public long PlayerId { get; set; }
        public int TurnNumber { get; set; }
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

        public async Task<Unit> Handle(ParseReports request, CancellationToken cancellationToken) {
            ReportSync sync;
            var playerId = request.PlayerId;
            var player = await db.Players.FindAsync(playerId);

            // load all turn numbers of the player so that we can properly transfer history
            List<TurnContext> turns = new List<TurnContext>((await db.Turns
                .Where(x => x.PlayerId == playerId)
                .OrderBy(x => x.Number)
                .Select(x => x.Number)
                .ToListAsync())
                .Select(x => new TurnContext(playerId, x))
            );

            // most recent turn stored in database
            var lastTurn = turns.Last();

            // if historical reports are added then we need to reload old turns and start from the earliest turn
            var startingTurnIndex = turns.FindIndex(x => x.TurnNumber == request.EarliestTurn);
            var currentTurn = turns[startingTurnIndex];

            // check if current turn was processed before
            var isLoaded = (await db.Regions.FilterByTurn(currentTurn).CountAsync()) > 0;
            if (isLoaded) {
                // current turn already exists in database so we can just load it and start updating
                var turn = await GetTurnAsync(currentTurn);
                sync = ReportSync.Load(turn);
            }
            else {
                if (startingTurnIndex == 0) {
                    // this is first turn for current player
                    sync = ReportSync.Empty(playerId, currentTurn.TurnNumber);
                }
                else {
                    // no data present for turn and this is not very first turn for this faction
                    // then we need to load previos turn map
                    var prevTurn = turns[startingTurnIndex - 1];

                    var turn = await GetTurnAsync(prevTurn, track: false, addUnits: false, addEvents: false);
                    sync = ReportSync.Copy(playerId, prevTurn.TurnNumber, turn, mapper);
                }
            }

            for (var i = startingTurnIndex; i < turns.Count; i++) {
                var t = turns[i];
                var report = await MergeReportsAsync(db, request.PlayerId, player.FactionNumber, t);

                if (request.Map != null) {
                    report.MergeMap(request.Map);
                }

                // update player password if latest turn or newer
                if (t >= lastTurnNumber && report.OrdersTemplate?.Password != null) {
                    player.Password = report.OrdersTemplate.Password;
                }

                sync.SyncReport(report);

                await sync.ApplyChangesAsync(db);
                await db.SaveChangesAsync();

                if ((turns.Count - 1) > i) {
                    var nextTurn = await GetTurnAsync(playerId, turns[i + 1]);

                    sync = ReportSync.Copy(playerId, nextTurn.Number, sync, mapper);
                    sync.UpdateWith(nextTurn, mapper);

                    // var nextRegions = nextTurn.Regions.ToDictionary(k => k.Id);
                    // var nextStructures = nextTurn.Structures.ToDictionary(x => x.Id);
                    // var nextFactions = nextTurn.Factions.ToDictionary(k => k.Number);
                    // var nextUnits = nextTurn.Units.ToDictionary(x => x.Number);

                    // var missingRegions = regions.Keys.Except(nextRegions.Keys).Select(x => mapper.Map<DbRegion>(regions[x]));
                    // foreach (var reg in missingRegions) {
                    //     foreach (var str in reg.Structures.ToList()) {
                    //         if (str.Number > GameConsts.MAX_BUILDING_NUMBER) {
                    //             reg.Structures.Remove(str);
                    //             continue;
                    //         }

                    //         str.Units.Clear();
                    //         nextStructures.Add(str.Id, str);
                    //     }

                    //     reg.Units.Clear();
                    //     nextRegions.Add(reg.Id, reg);
                    // }

                    // var missingFactions = factions.Keys.Except(nextFactions.Keys).Select(x => mapper.Map<DbFaction>(factions[x]));
                    // foreach (var f in missingFactions) {
                    //     f.Units.Clear();
                    //     f.Events.Clear();
                    //     f.Stats.Clear();

                    //     nextFactions.Add(f.Number, f);
                    // }

                    // regions = nextRegions;
                    // structures = nextStructures;
                    // factions = nextFactions;
                    // units = nextUnits;
                }
            }

            return Unit.Value;
        }

        private Task<DbTurn> GetTurnAsync(InTurnContext context, bool track = true, bool addUnits = true, bool addEvents = true, bool addStructures = true) {
            IQueryable<DbTurn> turns = db.Turns
                .AsSplitQuery()
                .FilterByPlayer(context);

            if (!track) {
                turns = turns.AsNoTracking();
            }

            turns = turns
                .Include(x => x.Regions)
                .Include(x => x.Factions);

            if (addUnits) {
                turns = turns.Include(x => x.Units);
            }

            if (addEvents) {
                turns = turns.Include(x => x.Events);
            }

            if (addStructures) {
                turns = turns.Include(x => x.Structures);
            }

            return turns.SingleOrDefaultAsync(x => x.Number == context.TurnNumber);
        }

        private static async Task<JReport> MergeReportsAsync(Database db, long playerId, int? playerFactionNumber, int turnNumber) {

            var otherReports = await db.Reports
                .AsNoTracking()
                .Where(x => x.PlayerId == playerId && x.TurnNumber == turnNumber)
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

            return report;
        }

        private static void UpdateTurn(Database db, JReport report, int turnNumber,
            RegionDic regions, FactionsDic factions, StructuresDic structures, UnitsDic units) {

            CreateOrUpdateFaction(factions, report.Faction);

            UnitOrders unitOrders = report.OrdersTemplate.Units.ToDictionary(x => x.Unit, x => x.Orders);
            foreach (var region in report.Regions) {
                CreateOrUpdateRegion(db, turnNumber, factions, regions, structures, units, region, unitOrders);
            }

            AddEvents(factions, regions, report);
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
                    : regions[DbRegion.MakeId(ev.Coords.X, ev.Coords.Y, ev.Coords.Z ?? DEFAULT_LEVEL_Z)];

                var dbEv = new DbEvent {
                    Faction = faction,
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

        private static DbRegion CreateOrUpdateRegion(Database db, long playerId, int turnNumber, FactionsDic factions, RegionDic regions, StructuresDic structures,
            UnitsDic units, JRegion source, UnitOrders unitOrders) {
            var x = source.Coords.X;
            var y = source.Coords.Y;
            var z = source.Coords.Z;

            var id = DbRegion.MakeId(x, y, z);

            DbRegion region;
            if (!regions.TryGetValue(id, out region)) {
                region = new DbRegion();
                regions.Add(id, region);
            }

            region.PlayerId = playerId;
            region.TurnNumber = turnNumber;
            region.Explored = true;
            region.UpdatedAtTurn = turnNumber;
            region.X = x;
            region.Y = y;
            region.Z = z;
            region.Label = source.Coords.Label;
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
                CreateOrUpdateUnit(factions, units, region, null, unit, unitSeq++, unitOrders);
            }

            int structureSeq = 0;

            HashSet<string> presentStructures = new HashSet<string>();
            foreach (var str in source.Structures) {
                var dbStr = CreateOrUpdateStructure(structures, region, str, structureSeq++);
                presentStructures.Add(dbStr.Id);

                foreach (var unit in str.Units) {
                    var strUnit = CreateOrUpdateUnit(factions, units, region, dbStr, unit, unitSeq++, unitOrders);
                }
            }

            // remove structures that are not present anymore
            for (int i = region.Structures.Count - 1; i >= 0 ; i--) {
                var str = region.Structures[i];
                var strUid = str.Id;

                if (!presentStructures.Contains(strUid)) {
                    if (str.Id != null) {
                        db.Remove(str);
                    }

                    structures.Remove(strUid);
                    region.Structures.RemoveAt(i);
                }
            }

            // add regions from exits
            foreach (var exit in source.Exits) {
                var targetId = DbRegion.MakeId(exit.Coords.X, exit.Coords.Y, exit.Coords.Z ?? DEFAULT_LEVEL_Z);
                if (!regions.TryGetValue(targetId, out var target)) {
                    target = new DbRegion {
                        PlayerId = playerId,
                        TurnNumber = turnNumber,
                        X = exit.Coords.X,
                        Y = exit.Coords.Y,
                        Z = exit.Coords.Z ?? DEFAULT_LEVEL_Z,
                        Terrain = exit.Terrain,
                        Explored = false,
                        Label = exit.Coords.Label,
                        Province = exit.Province
                    };

                    if (exit.Settlement != null) {
                        target.Settlement = new DbSettlement {
                            Name = exit.Settlement.Name,
                            Size = exit.Settlement.Size
                        };
                    }

                    regions.Add(targetId, target);
                }

                if (!region.Exits.Any(x => x.TargetRegionId == targetId)) {
                    region.Exits.Add(new DbExit {
                        PlayerId = playerId,
                        TurnNumber = turnNumber,
                        Direction = exit.Direction,
                        OriginRegionId = region.Id,
                        TargetRegionId = targetId
                    });
                }


                // region.Explored = false;
                // region.UpdatedAtTurn = turnNumber;
                // region.X = exit.X;
                // region.Y = exit.Y;
                // region.Z = exit.Z;
                // region.Label = exit.Label;
                // region.Province = exit.Province;
                // region.Terrain = exit.Terrain;
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

        private static DbUnit CreateOrUpdateUnit(FactionsDic factions, UnitsDic units, DbRegion region, DbStructure structure, JUnit source,
            int seq, UnitOrders unitOrders) {
            if (!units.TryGetValue(source.Number, out var unit)) {
                unit = new DbUnit {
                    Number = source.Number
                };

                units.Add(unit.Number, unit);

                unit.Region = region;
                region.Units.Add(unit);

                if (structure != null) {
                    unit.Structure = structure;
                    structure.Units.Add(unit);
                }
            }

            if (source.Faction != null && unit.Faction == null) {
                var faction = CreateOrUpdateFaction(factions, source.Faction);

                unit.Faction = faction;
                faction.Units.Add(unit);
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

            if (unitOrders.ContainsKey(unit.Number)) {
                unit.Orders = unitOrders[unit.Number];
            }

            return unit;
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
                .GroupBy(x => x.TargetRegionId)
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
