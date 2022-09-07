namespace advisor.Persistence {
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using HotChocolate;

    using ReportId = System.ValueTuple<long, int, int>;

    public class DbReport : InTurnContext {
        [GraphQLIgnore]
        [NotMapped]
        public ReportId CompsiteId => MakeId(this);

        public static ReportId MakeId(long playerId, int turnNumber, int factionNumber) => (playerId, turnNumber, factionNumber);
        public static ReportId MakeId(DbReport report) => (report.PlayerId, report.TurnNumber, report.FactionNumber);

        public static IQueryable<DbReport> FilterById(IQueryable<DbReport> q, ReportId id) {
            var (playerId, turnNumber, factionNumber) = id;
            return q.Where(x =>
                    x.PlayerId == playerId
                && x.TurnNumber == turnNumber
                && x.FactionNumber == factionNumber
            );
        }


        [Required]
        public int FactionNumber { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }



        [Required, MaxLength(100)]
        public string FactionName { get; set; }

        [Required]
        public string Source { get; set; }

        public string Json { get; set; }


        [GraphQLIgnore]
        public DbPlayer Player { get; set; }

        [GraphQLIgnore]
        public DbPlayerTurn Turn { get; set; }
    }
}
