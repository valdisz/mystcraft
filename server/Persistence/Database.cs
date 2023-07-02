namespace advisor.Persistence;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public abstract class Database : DbContext, DatabaseIO, UnitOfWorkIO {
    protected Database() : base() {

    }

    protected Database(DbContextOptions options) : base(options) {

    }

    /// <summary>
    /// Database provider used by the system
    /// </summary>
    public DatabaseProvider Provider {
        get {
            if (Database.IsNpgsql()) { return DatabaseProvider.PgSQL; }
            if (Database.IsSqlServer()) { return DatabaseProvider.MsSQL;}
            if (Database.IsSqlite()) { return DatabaseProvider.SQLite; }

            throw new System.Exception();
        }
    }

    /// <summary>
    /// List of registered users who can access the system depending on their roles
    /// </summary>
    public DbSet<DbUser> Users { get; set; }

    /// <summary>
    /// List of game engines which are supported by the system
    /// </summary>
    public DbSet<DbGameEngine> GameEngines { get; set; }

    /// <summary>
    /// List of local and remote games which are registered in the system
    /// </summary>
    public DbSet<DbGame> Games { get; set; }


    /////////////////////////////////////////////
    ///// Game turn data as the engine returns it

    /// <summary>
    /// List of turns for the game
    /// </summary>
    public DbSet<DbTurn> Turns { get; set; }

    /// <summary>
    /// List of reports for the game
    /// </summary>
    public DbSet<DbReport> Reports { get; set; }

    /// <summary>
    /// List of articles for the game written by player of by the game engine itself
    /// </summary>
    public DbSet<DbArticle> Articles{ get; set; }


    /////////////////////////////////////////////
    ///// Player controled factions and data which is modified during the game

    /// <summary>
    /// List of pending players for the game
    /// </summary>
    public DbSet<DbRegistration> Registrations { get; set; }

    /// <summary>
    /// List of players for the game
    /// </summary>
    public DbSet<DbPlayer> Players { get; set; }

    /// <summary>
    /// List of player turns for the game
    /// </summary>
    public DbSet<DbPlayerTurn> PlayerTurns { get; set; }

    /// <summary>
    /// List of additional reports that playes added additionally for the game
    /// </summary>
    public DbSet<DbAdditionalReport> AditionalReports { get; set; }

    /// <summary>
    /// List of orders that players submitted for the game
    /// </summary>
    public DbSet<DbOrders> Orders { get; set; }


    /////////////////////////////////////////////
    ///// Parsed and normalized game state for the each player based on their reports

    /// <summary>
    /// List of factions for the game turn as seen by the player
    /// </summary>
    public DbSet<DbFaction> Factions { get; set; }

    public DbSet<DbAttitude> Attitudes { get; set; }
    public DbSet<DbEvent> Events { get; set; }
    public DbSet<DbRegion> Regions { get; set; }
    public DbSet<DbProductionItem> Production { get; set; }
    public DbSet<DbTradableItem> Markets { get; set; }
    public DbSet<DbExit> Exits { get; set; }
    public DbSet<DbStructure> Structures { get; set; }
    public DbSet<DbUnit> Units { get; set; }
    public DbSet<DbUnitItem> Items { get; set; }
    public DbSet<DbBattle> Battles { get; set; }

    // Additional features not provided by the game engine
    public DbSet<DbAlliance> Alliances { get; set; }
    public DbSet<DbAllianceMember> AllianceMembers { get; set; }
    public DbSet<DbStudyPlan> StudyPlans { get; set; }
    public DbSet<DbTurnStatisticsItem> TurnStatistics { get; set; }
    public DbSet<DbRegionStatisticsItem> RegionStatistics { get; set; }
    public DbSet<DbTreasuryItem> Treasury { get; set; }

    protected override void OnModelCreating(ModelBuilder model) {
        model.ApplyConfiguration(new DbUserConfiguration(this));
        model.ApplyConfiguration(new DbGameEngineConfiguration(this));
        model.ApplyConfiguration(new DbGameConfiguration(this));
        model.ApplyConfiguration(new DbRegistrationConfiguration(this));
        model.ApplyConfiguration(new DbTurnConfiguration(this));
        model.ApplyConfiguration(new DbReportConfiguration(this));
        model.ApplyConfiguration(new DbArticleConfiguration(this));
        model.ApplyConfiguration(new DbPlayerConfiguration(this));
        model.ApplyConfiguration(new DbPlayerTurnConfiguration(this));
        model.ApplyConfiguration(new DbAdditionalReportConfiguration(this));
        model.ApplyConfiguration(new DbOrdersConfiguration(this));
        model.ApplyConfiguration(new DbRegionConfiguration(this));
        model.ApplyConfiguration(new DbTradableItemConfiguration(this));
        model.ApplyConfiguration(new DbProductionItemConfiguration(this));
        model.ApplyConfiguration(new DbTreasuryItemConfiguration(this));
        model.ApplyConfiguration(new DbUnitItemConfiguration(this));
        model.ApplyConfiguration(new DbTurnStatisticsItemConfiguration(this));
        model.ApplyConfiguration(new DbRegionStatisticsItemConfiguration(this));
        model.ApplyConfiguration(new DbExitConfiguration(this));
        model.ApplyConfiguration(new DbFactionConfiguration(this));
        model.ApplyConfiguration(new DbAttitudeConfiguration(this));
        model.ApplyConfiguration(new DbEventConfiguration(this));
        model.ApplyConfiguration(new DbStructureConfiguration(this));
        model.ApplyConfiguration(new DbUnitConfiguration(this));
        model.ApplyConfiguration(new DbStudyPlanConfiguration(this));
        model.ApplyConfiguration(new DbAllianceConfiguration(this));
        model.ApplyConfiguration(new DbAllianceMemberConfiguration(this));
        model.ApplyConfiguration(new DbBattleConfiguration(this));
    }

    private int txCounter = 0;
    private bool willRollback = false;
    private IDbContextTransaction transaction;

    async ValueTask<Unit> UnitOfWorkIO.Save(CancellationToken ct) {
        await SaveChangesAsync(ct);
        return unit;
    }

    async ValueTask<Unit> UnitOfWorkIO.Begin(CancellationToken ct) {
        if (txCounter++ != 0) {
            return Success(unit);
        }

        transaction = await Database.BeginTransactionAsync(ct);
        willRollback = false;

        return Success(unit);
    }

    public AsyncIO<Unit> CommitTransaction(CancellationToken cancellation = default)
        => async () => {
            if (!willRollback) {
                await SaveChangesAsync(cancellation);
            }

            if (txCounter == 0) {
                return Success(unit);
            }

            if (txCounter == 1) {
                if (willRollback) {
                    await transaction.RollbackAsync(cancellation);
                }
                else {
                    await transaction.CommitAsync(cancellation);
                }

                await transaction.DisposeAsync();
                transaction = null;
                txCounter = 0;
            }
            else {
                txCounter--;
            }

            return !willRollback ? Success(unit) : Failure<Unit>("Failed to rollback transaction.");
        };

    public AsyncIO<Unit> RollbackTransaction(CancellationToken cancellation = default)
        => async () => {
            if (txCounter != 0) {
                willRollback = true;

                if (--txCounter == 0) {
                    await transaction.RollbackAsync(cancellation);
                    await transaction.DisposeAsync();
                }
            }

            return Success(unit);
        };

    public override ValueTask DisposeAsync() {
        return base.DisposeAsync();
    }

    public ValueTask<DbGame> Add(DbGame game, CancellationToken ct)
    {
        throw new System.NotImplementedException();
    }
}
