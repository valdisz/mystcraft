namespace advisor.Persistence;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public abstract class Database : DbContext, IUnitOfWork {
    protected Database() : base() {

    }

    protected Database(DbContextOptions options) : base(options) {

    }

    public DatabaseProvider Provider {
        get {
            if (Database.IsSqlite()) { return DatabaseProvider.SQLite; }
            if (Database.IsSqlServer()) { return DatabaseProvider.MsSQL;}
            if (Database.IsNpgsql()) { return DatabaseProvider.PgSQL; }

            throw new System.Exception();
        }
    }

    // Registered users
    public DbSet<DbUser> Users { get; set; }

    // Game engines
    public DbSet<DbGameEngine> GameEngines { get; set; }

    // Local and remote games
    public DbSet<DbGame> Games { get; set; }

    // Game turn data as the engine returns it
    public DbSet<DbTurn> Turns { get; set; }
    public DbSet<DbReport> Reports { get; set; }
    public DbSet<DbArticle> Articles{ get; set; }

    // Player controled factions and data which is modified during the game
    public DbSet<DbRegistration> Registrations { get; set; }
    public DbSet<DbPlayer> Players { get; set; }
    public DbSet<DbPlayerTurn> PlayerTurns { get; set; }
    public DbSet<DbAdditionalReport> AditionalReports { get; set; }
    public DbSet<DbOrders> Orders { get; set; }

    // Parsed and normalized game state for the each player based on their reports
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

    public async Task BeginTransactionAsync(CancellationToken cancellation = default) {
        if (txCounter == 0) {
            transaction = await Database.BeginTransactionAsync(cancellation);
            willRollback = false;
        }

        txCounter++;
    }

    public async Task<bool> CommitTransactionAsync(CancellationToken cancellation = default) {
        if (!willRollback) {
            await SaveChangesAsync(cancellation);
        }

        if (txCounter == 0) {
            return true;
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

        return !willRollback;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellation = default) {
        if (txCounter == 0) {
            return;
        }

        if (txCounter == 1) {
            await transaction.RollbackAsync(cancellation);
            await transaction.DisposeAsync();
            transaction = null;
            txCounter = 0;
        }
        else {
            txCounter--;
        }

        willRollback = true;
    }

    public override ValueTask DisposeAsync() {
        return base.DisposeAsync();
    }
}
