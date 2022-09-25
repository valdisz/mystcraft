namespace advisor.Persistence {

    public class DbIncome {
        public int Work { get; set; }
        public int Entertain { get; set; }
        public int Tax { get; set; }
        public int Pillage { get; set; }
        public int Trade { get; set; }
        public int Claim { get; set; }
        public int Total => Work + Entertain + Tax + Pillage + Trade + Claim;
    }


    public class DbExpenses {
        public int Trade { get; set; }
        public int Study { get; set; }
        public int Consume { get; set; }
        public int Total => Trade + Study;
    }
}
