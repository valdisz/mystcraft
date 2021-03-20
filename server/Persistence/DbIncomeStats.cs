namespace advisor.Persistence {
    using HotChocolate;

    [GraphQLName("IncomeStats")]
    public class DbIncomeStats {
        public int Work { get; set; }
        public int Tax { get; set; }
        public int Pillage { get; set; }
        public int Trade { get; set; }
        public int Total => Work + Tax + Pillage + Trade;
    }
}
