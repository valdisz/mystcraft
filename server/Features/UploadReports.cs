namespace advisor.Features {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Model;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    public record UploadReports(long PlayerId, IEnumerable<string> Reports) : IRequest<long> {

    }

    public class UploadReportsHandler : IRequestHandler<UploadReports, long> {
        public UploadReportsHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<long> Handle(UploadReports request, CancellationToken cancellationToken) {
            DbPlayer player = await db.Players
                .Include(x => x.Game)
                .SingleOrDefaultAsync(x => x.Id == request.PlayerId);

            if (player == null) return -1;

            int earliestTurn = int.MaxValue;
            foreach (var report in request.Reports) {
                var turnNumber = await LoadReportAsync(db, report, player);
                earliestTurn = Math.Min(earliestTurn, turnNumber);
            }

            await db.SaveChangesAsync();

            return earliestTurn;
        }

        private static async Task<int> LoadReportAsync(Database db, string source, DbPlayer player) {
            using var textReader = new StringReader(source);

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

            var turn = await db.Turns
                .FirstOrDefaultAsync(x => x.PlayerId == player.Id && x.Number == turnNumber);
            DbReport dbReport = null;

            if (turn == null) {
                turn = new DbTurn {
                    PlayerId = player.Id,
                    Month = month,
                    Year = year,
                    Number = turnNumber
                };

                player.Turns.Add(turn);
            }
            else {
                dbReport = await db.Reports
                    .FirstOrDefaultAsync(x => x.FactionNumber == factionNumber && x.TurnId == turn.Id);
            }

            if (dbReport == null) {
                dbReport = new DbReport {
                    PlayerId = player.Id,
                    TurnId = turn.Id
                };

                player.Reports.Add(dbReport);
                turn.Reports.Add(dbReport);
            }

            dbReport.FactionNumber = factionNumber;
            dbReport.FactionName = factionName;
            dbReport.Source = source;
            dbReport.Json = json.ToString(Formatting.Indented);

            player.FactionNumber ??= dbReport.FactionNumber;
            if (dbReport.FactionNumber == player.FactionNumber && turnNumber > player.LastTurnNumber) {
                player.FactionName = dbReport.FactionName;
                player.LastTurnNumber = turnNumber;
            }

            player.Password ??= ordersTemplate?.Password;

            player.Game.EngineVersion ??= engine?.Version;
            player.Game.RulesetName ??= engine?.Ruleset?.Name;
            player.Game.RulesetVersion ??= engine?.Ruleset?.Version;

            return turnNumber;
        }
    }
}
