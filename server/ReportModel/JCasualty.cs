namespace advisor.Model {
    using System.Collections.Generic;
    using HotChocolate;

    [GraphQLName("Casualties")]
    public class JCasualties {
        public JParticipant Army { get; set; }
        public int Lost { get; set; }
        public List<int> DamagedUnits { get; set; }
    }
}
