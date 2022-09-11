namespace advisor.Features {
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

    public record PlayerReportParse(long PlayerId, int EarliestTurn, JReport Map = null) : IRequest;

    public class PlayerReportParseHandler : IRequestHandler<PlayerReportParse> {
        public PlayerReportParseHandler(Database db, IMapper mapper, IMediator mediator) {
            this.db = db;
            this.mapper = mapper;
            this.mediator = mediator;
        }

        private readonly Database db;
        private readonly IMapper mapper;
        private readonly IMediator mediator;

        public async Task<Unit> Handle(PlayerReportParse request, CancellationToken cancellationToken) {
            var playerId = request.PlayerId;
            var player = await db.Players
                .SingleOrDefaultAsync(x => x.Id == playerId);

            // load all turn numbers of the player so that we can properly transfer history
            List<TurnContext> turns = new List<TurnContext>((await db.PlayerTurns
                .AsNoTracking()
                .Where(x => x.PlayerId == playerId)
                .OrderBy(x => x.Number)
                .Select(x => x.Number)
                .ToListAsync())
                .Select(x => new TurnContext(player.GameId, playerId, x))
            );

            // if historical reports are added then we need to reload old turns and start from the earliest turn
            var startIndex = turns.FindIndex(x => x.TurnNumber == request.EarliestTurn);

            // go thoruh all turns and update them
            ReportSync prev = null;
            for (var i = startIndex; i < turns.Count; i++) {
                var currentTurn = turns[i];
                bool isFirstIteration = i == startIndex;
                bool isLastIteration = i == turns.Count - 1;

                // load and merge
                var report = await MergeReportsAsync(db, request.PlayerId, currentTurn.TurnNumber, player.Number);
                if (report == null) {
                    // no own report, break
                    break;
                }

                //import map if this is last turn in the list
                if (isLastIteration && request.Map != null) {
                    report.MergeMap(request.Map);
                }

                ReportSync sync;
                if (isFirstIteration) {
                    // fist iteration
                    // check if current turn was processed before
                    var isLoaded = (await db.Regions.InTurn(currentTurn).CountAsync()) > 0;
                    if (isLoaded) {
                        // current turn already exists in database so we can just load it and start updating
                        var turn = await GetTurnAsync(currentTurn);
                        sync = new ReportSync(db, currentTurn.PlayerId, currentTurn.TurnNumber, report);
                        sync.Load(turn);
                    }
                    else {
                        if (startIndex == 0) {
                            // this is first turn for current player
                            sync = new ReportSync(db, currentTurn.PlayerId, currentTurn.TurnNumber, report);
                            sync.LoadOrdersFromReport();
                        }
                        else {
                            // no data present for turn and this is not very first turn for this faction
                            // then we need to load previos turn map
                            var prevTurn = turns[startIndex - 1];

                            var turn = await GetTurnAsync(prevTurn, track: false, addUnits: false, addEvents: false);
                            sync = new ReportSync(db, currentTurn.PlayerId, currentTurn.TurnNumber, report);
                            sync.Copy(turn, mapper);
                        }
                    }
                }
                else {
                    var turn = await GetTurnAsync(currentTurn);
                    sync = new ReportSync(db, currentTurn.PlayerId, currentTurn.TurnNumber, report);
                    sync.Load(turn);
                }

                // synchornize report with database
                await sync.SyncReportAsync();

                // update player password if latest turn or it is empty
                bool updatePassword = player.Password == null || currentTurn.TurnNumber == player.LastTurnNumber;

                // and report contains password
                updatePassword = updatePassword && !string.IsNullOrWhiteSpace(report.OrdersTemplate.Password);

                if (updatePassword) {
                    player.Password = report.OrdersTemplate.Password;
                }

                // save changes to database
                await db.SaveChangesAsync();

                prev = sync;
            }

            return Unit.Value;
        }

        private Task<DbPlayerTurn> GetTurnAsync(InTurnContext context, bool track = true, bool addUnits = true, bool addEvents = true,
            bool addStructures = true, bool addFactions = true) {
            IQueryable<DbPlayerTurn> turns = db.PlayerTurns
                .AsSplitQuery()
                .OnlyPlayer(context);

            if (!track) {
                turns = turns.AsNoTrackingWithIdentityResolution();
            }

            turns = turns
                .Include(x => x.Regions)
                .Include(x => x.Exits)
                .Include(x => x.Production)
                .Include(x => x.Markets);

            if (addFactions) {
                turns = turns
                    .Include(x => x.Factions)
                    .Include(x => x.Attitudes);
            }

            if (addUnits) {
                turns = turns
                    .Include(x => x.Units)
                    .Include(x => x.Items);
            }

            if (addEvents) {
                turns = turns
                    .Include(x => x.Events)
                    .Include(x => x.Stats);
            }

            if (addStructures || addUnits) {
                turns = turns
                    .Include(x => x.Structures);
            }

            return turns.SingleOrDefaultAsync(x => x.Number == context.TurnNumber);
        }

        private static async Task<JReport> MergeReportsAsync(Database db, long playerId, int turnNumber, int? playerFactionNumber) {
            JReport report = null;

            var reportsQuery = db.AditionalReports
                .AsNoTracking()
                .InTurn(playerId, turnNumber)
                .Select(x => new { x.FactionNumber });

            if (playerFactionNumber != null) {
                reportsQuery = reportsQuery.Where(x => x.FactionNumber != playerFactionNumber);

                var ownJson = await db.AditionalReports
                    .AsNoTracking()
                    .InTurn(playerId, turnNumber)
                    .Where(x => x.FactionNumber == playerFactionNumber)
                    .Select(x => x.Json)
                    .SingleOrDefaultAsync();

                if (ownJson == null) {
                    var ownReport = await db.AditionalReports
                        .InTurn(playerId, turnNumber)
                        .Where(x => x.FactionNumber == playerFactionNumber)
                        .SingleOrDefaultAsync();

                    if (ownReport == null) {
                        // own report is not loaded for this turn
                        return null;
                    }

                    report = await ParseJsonReportAsync(ownReport);
                }
                else {
                    report = JsonConvert.DeserializeObject<JReport>(ownJson);
                }
            }

            var reports = await reportsQuery.ToListAsync();
            foreach (var rep in reports) {
                JReport next;

                var json = await db.AditionalReports
                    .AsNoTracking()
                    .InTurn(playerId, turnNumber)
                    .Where(x => x.FactionNumber == rep.FactionNumber)
                    .Select(x => x.Json)
                    .SingleOrDefaultAsync();

                if (json == null) {
                    var unprasedReport = await db.AditionalReports
                        .InTurn(playerId, turnNumber)
                        .Where(x => x.FactionNumber == rep.FactionNumber)
                        .SingleOrDefaultAsync();
                    next = await ParseJsonReportAsync(unprasedReport);
                }
                else {
                    next = JsonConvert.DeserializeObject<JReport>(json);
                }

                if (report == null) {
                    report = next;
                }
                else {
                    report.Merge(next);
                }
            }

            return report;
        }

        public static async Task<JReport> ParseJsonReportAsync(DbAditionalReport rec) {
            using var textReader = new StringReader(rec.Source);
            using var atlantisReader = new AtlantisReportJsonConverter(textReader);

            var json = await atlantisReader.ReadAsJsonAsync();
            rec.Json = json.ToString(Formatting.None);

            return json.ToObject<JReport>();
        }
    }
}
