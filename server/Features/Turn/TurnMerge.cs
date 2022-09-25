namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using advisor.TurnProcessing;
using advisor.Model;
using System.IO;
using System.Collections.Generic;
using AutoMapper;
using System;
using System.Diagnostics;

public record TurnMerge(DbGame Game, int TurnNumber, bool Force = false, long[] PlayerIds = null): IRequest<TurnMergeResult>;

public record TurnMergeResult(bool IsSuccess, string Error = null, DbTurn Turn = null) : MutationResult(IsSuccess, Error);

public class TurnMergeHandler : IRequestHandler<TurnMerge, TurnMergeResult> {
    public TurnMergeHandler(IReportParser parser, IMapper mapper, IUnitOfWork unit) {
        this.parser = parser;
        this.mapper = mapper;
        this.unit = unit;
        this.db = unit.Database;
    }

    private readonly IReportParser parser;
    private readonly IMapper mapper;
    private readonly IUnitOfWork unit;
    private readonly Database db;

    public async Task<TurnMergeResult> Handle(TurnMerge request, CancellationToken cancellationToken) {
        Debug.Assert(Object.ReferenceEquals(unit.Database, db));

        var game = request.Game;

        await unit.BeginTransactionAsync(cancellationToken);

        var turnNumber = request.TurnNumber;
        var nextTurnNumber = turnNumber + 1;
        var prevTurnNumber = turnNumber - 1;

        var turnsRepo = unit.Turns(game);
        var turn = await turnsRepo.GetOneAsync(turnNumber, cancellationToken);
        if (turn == null) {
            return new TurnMergeResult(false, "Turn not found.");
        }

        var reports = turnsRepo.GetReports(turnNumber)
            .Where(x => !x.IsMerged)
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);

        var playersRepo = unit.Players(game);
        await foreach (var report in reports) {
            try {
                JReport doc = ReadReport(report.Json);

                await foreach (var sharedReport in GetSharedReportsAsync(request.Game.Id, report.PlayerId, request.TurnNumber)) {
                    doc.Merge(sharedReport);
                }

                foreach (var unit in doc.OrdersTemplate?.Units ?? Enumerable.Empty<JUnitOrders>()) {
                    var orders = new DbOrders {
                        PlayerId = report.PlayerId,
                        TurnNumber = nextTurnNumber,
                        UnitNumber = unit.Unit,
                        Orders = unit.Orders,
                    };
                    await db.Orders.AddAsync(orders);
                }

                var player = await playersRepo.GetOneAsync(report.PlayerId);
                var thisTurn = await db.PlayerTurns
                    .Where(x => x.PlayerId == report.PlayerId && x.TurnNumber == turnNumber)
                    .SingleOrDefaultAsync();
                var prevTurn = await GetTurnAsync(report.PlayerId, prevTurnNumber, track: false, addUnits: false, addEvents: false);

                ReportSync sync = new ReportSync(db, report.PlayerId, request.TurnNumber, doc);
                if (prevTurn != null) {
                    sync.Copy(prevTurn, mapper);
                }

                sync.LoadOrders();
                await sync.SyncReportAsync();

                player.Name = doc.Faction.Name;
                player.Password = doc.OrdersTemplate.Password ?? player.Password;
                thisTurn.Name = player.Name;
                report.IsMerged = true;

                await unit.SaveChangesAsync();
            }
            catch (Exception ex) {
                await unit.RollbackTransactionAsync(cancellationToken);

                return new TurnMergeResult(false, $"Cannot merge reports. {ex}");
            }
        }

        await unit.CommitTransactionAsync(cancellationToken);

        return new TurnMergeResult(true, Turn: turn);
    }

    private async IAsyncEnumerable<JReport> GetSharedReportsAsync(long gameId, long playerId, int turnNumber) {
        var memberships = db.AllianceMembers
            .AsNoTracking()
            .Where(x => x.PlayerId == playerId)
            .AsAsyncEnumerable();

        await foreach (var membership in memberships) {
            var allianceMembers = db.AllianceMembers
                .AsNoTracking()
                .Where(x => x.AllianceId == membership.AllianceId && x.PlayerId != playerId && x.ShareMap)
                .AsAsyncEnumerable();

            await foreach (var member in allianceMembers) {
                var json = await db.Reports
                    .AsNoTracking()
                    .InGame(gameId)
                    .Where(x => x.PlayerId == member.PlayerId)
                    .Select(x => x.Json)
                    .SingleOrDefaultAsync();

                JReport doc = ReadReport(json);

                yield return doc;
            }
        }
    }

    private JReport ReadReport(byte[] json) {
        using var ms = new MemoryStream(json);
        using var reader = new StreamReader(ms);
        JReport doc = parser.Read(reader);

        return doc;
    }

    private Task<DbPlayerTurn> GetTurnAsync(long playerId, int turnNumber, bool track = true, bool addUnits = true, bool addEvents = true,
        bool addStructures = true, bool addFactions = true) {
        IQueryable<DbPlayerTurn> turns = db.PlayerTurns
            .AsSplitQuery()
            .OnlyPlayer(playerId);

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

        return turns.SingleOrDefaultAsync(x => x.TurnNumber == turnNumber);
    }
}
