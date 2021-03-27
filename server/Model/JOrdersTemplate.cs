namespace advisor.Model {
    using System.Collections.Generic;

    public class JOrdersTemplate {
        public int Faction { get; set; }
        public string Password { get; set; }
        public List<JUnitOrders> Units { get; set; } = new ();
    }
}
