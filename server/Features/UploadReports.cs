namespace advisor.Features {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Model;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record UploadReports(long PlayerId, IEnumerable<string> Reports) : IRequest<int>;

    public class UploadReportsHandler : IRequestHandler<UploadReports, int> {
        public UploadReportsHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<int> Handle(UploadReports request, CancellationToken cancellationToken) {
            DbPlayer player = await db.Players
                .Include(x => x.Game)
                .Include(x => x.AllianceMembererships)
                .SingleOrDefaultAsync(x => x.Id == request.PlayerId);

            if (player == null) return -1;

            var playerAlliances = player.AllianceMembererships.Select(x => x.AllianceId).ToList();

            var allies = (await db.Alliances
                .Where(x => playerAlliances.Contains(x.Id))
                .Include(x => x.Members.Where(m => m.PlayerId != player.Id))
                .ThenInclude(x => x.Player)
                .SelectMany(x => x.Members)
                .Select(x => x.Player)
                .ToListAsync())
                .Where(x => x.Id != player.Id)
                .ToList();

            int earliestTurn = int.MaxValue;
            foreach (var report in request.Reports) {
                var turnNumber = await LoadReportAsync(report, player, allies);
                earliestTurn = Math.Min(earliestTurn, turnNumber);
            }

            return earliestTurn;
        }

        private async Task SaveReportAsync(DbPlayer player, int year, int month, int turnNumber, int factionNumber, string factionName, string source) {
            var turn = await db.Turns
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PlayerId == player.Id && x.Number == turnNumber);

            if (turn == null) {
                turn = new DbTurn {
                    PlayerId = player.Id,
                    Month = month,
                    Year = year,
                    Number = turnNumber
                };

                await db.AddAsync(turn);
            }

            DbReport report = await db.Reports
                .FirstOrDefaultAsync(x => x.PlayerId == player.Id && x.FactionNumber == factionNumber && x.TurnNumber == turn.Number);

            if (report == null) {
                report = new DbReport {
                    PlayerId = player.Id,
                    TurnNumber = turn.Number,
                    FactionNumber = factionNumber,
                    FactionName = factionName
                };

                turn.Reports.Add(report);
                await db.AddAsync(report);
            }

            report.Source = source;

            await db.SaveChangesAsync();
        }

        private async Task<int> LoadReportAsync(string source, DbPlayer player, List<DbPlayer> allies) {
            using var textReader = new StringReader(source);

            using var atlantisReader = new AtlantisReportJsonConverter(textReader,
                new ReportFactionSection(),
                new RulesetSection()
            );
            var json = await atlantisReader.ReadAsJsonAsync();
            var report  = json.ToObject<JReport>();

            var faction = report.Faction;
            var date = report.Date;
            var engine = report.Engine;

            string factionName = faction.Name;
            int factionNumber = faction.Number;
            int year = date.Year;
            int month = date.Month;
            int turnNumber = month + (year - 1) * 12;

            await SaveReportAsync(player, year, month, turnNumber, factionNumber, factionName, source);
            foreach (var ally in allies) {
                await SaveReportAsync(ally, year, month, turnNumber, factionNumber, factionName, source);
            }

            player.Number ??= factionNumber;
            if (factionNumber == player.Number && turnNumber >= player.LastTurnNumber) {
                player.Name = factionName;
                player.LastTurnNumber = turnNumber;
            }

            player.Game.EngineVersion ??= engine?.Version;
            player.Game.RulesetName ??= engine?.Ruleset?.Name;
            player.Game.RulesetVersion ??= engine?.Ruleset?.Version;

            await db.SaveChangesAsync();

            return turnNumber;
        }
    }
}
