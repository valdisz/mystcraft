namespace advisor.Model {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class JBaseReport {
        public JFaction Faction { get; set; }
        public List<JFactionStatus> FactionStatus { get; set; } = new ();
        public List<string> Errors { get; set; } = new ();
        public List<JEvent> Events { get; set; } = new ();
        public List<JSkillReport> SkillReports { get; set; } = new ();
        public List<JItemReport> ItemReports { get; set; } = new ();
        public JAttitudes Attitudes { get; set; } = new JAttitudes();
        public int UnclaimedSilver { get; set; } = 0;
    }

    public class JReport : JBaseReport {
        public JDate Date { get; set; }
        public JGameEngine Engine { get; set; }
        public List<JBattle> Battles { get; set; } = new ();
        public List<JRegion> Regions { get; set; } = new ();
        public JOrdersTemplate OrdersTemplate { get; set; }

        public List<JBaseReport> OtherReports { get; set; } = new ();

        /// Merge and update current report with other report data
        public void MergeMap(JReport report) {
            report.Date = null;
            report.Faction = null;

            Merge(report, addStructures: true, addUnits: false);
        }

        public void Merge(JReport report, bool addStructures = true, bool addUnits = true) {
            if (report.Date != null && report.Faction != null) {
                if (Date.Year != report.Date.Year || Date.Month != report.Date.Month) {
                    throw new InvalidOperationException();
                }

                var other = new JBaseReport {
                    Faction = report.Faction,
                    FactionStatus = report.FactionStatus,
                    Errors = report.Errors,
                    Events = report.Events,
                    SkillReports = report.SkillReports,
                    ItemReports = report.ItemReports,
                    Attitudes = report.Attitudes,
                    UnclaimedSilver = report.UnclaimedSilver
                };

                OtherReports.Add(other);
            }

            Dictionary<string, JRegion> regs = Regions.ToDictionary(x => x.Coords.ToString());
            foreach (var otherReg in report.Regions) {
                foreach (var unit in otherReg.Units) {
                    unit.Own = unit.Faction?.Number == Faction.Number;
                }

                foreach (var str in otherReg.Structures) {
                    foreach (var unit in str.Units) {
                        unit.Own = unit.Faction?.Number == Faction.Number;
                    }
                }

                var key = otherReg.Coords.ToString();

                if (regs.TryGetValue(key, out var reg)) {
                    MergeRegion(reg, otherReg, addStructures, addUnits);
                }
                else {
                    if (!addStructures) {
                        otherReg.Structures.Clear();
                    }

                    if (!addUnits) {
                        otherReg.Units.Clear();
                        foreach (var str in otherReg.Structures) {
                            str.Units.Clear();
                        }
                    }

                    otherReg.__Imported = true;

                    Regions.Add(otherReg);
                    regs.Add(key, otherReg);
                }
            }

            if (OrdersTemplate == null) {
                OrdersTemplate = new JOrdersTemplate {
                    Faction = Faction.Number
                };
            }

            OrdersTemplate.Units.AddRange(report.OrdersTemplate?.Units ?? Enumerable.Empty<JUnitOrders>());

            MergeBattles(report.Battles);
        }

        private void MergeBattles(List<JBattle> battles) {
            string getKey(JBattle b) => $"{b.Attacker.Number}-{b.Defender.Number}-{b.Location.Coords.X}-{b.Location.Coords.Y}-{b.Location.Coords.Z}";

            var dest = Battles.ToDictionary(getKey);
            var src = battles.ToDictionary(getKey);

            foreach (var key in src.Keys.Except(dest.Keys)) {
                Battles.Add(src[key]);
            }
        }

        private void MergeRegion(JRegion reg, JRegion other, bool addStructures, bool addUnits) {
            if (other.Products.Count > reg.Products.Count) reg.Products = other.Products;
            if (other.ForSale.Count > reg.ForSale.Count) reg.ForSale = other.ForSale;
            if (other.Wanted.Count > reg.Wanted.Count) reg.Wanted = other.Wanted;

            reg.Gate ??= other.Gate;

            var units = reg.Units
                .Concat(reg.Structures.SelectMany(x => x.Units))
                .ToDictionary(x => x.Number);

            var structs = reg.Structures.ToDictionary(x => x.Structure.Number);

            if (addUnits) {
                foreach (var otherUnit in other.Units) {
                    var key = otherUnit.Number;

                    if (units.TryGetValue(key, out var unit)) {
                        MergeUnit(unit, otherUnit);
                    }
                    else {
                        reg.Units.Add(otherUnit);
                        units.Add(key, otherUnit);
                    }
                }
            }

            if (addStructures) {
                foreach (var otherStruct in other.Structures) {
                    var str = structs[otherStruct.Structure.Number];

                    MergeStructure(str.Structure, otherStruct.Structure);

                    if (addUnits) {
                        foreach (var otherUnit in otherStruct.Units) {
                            var key = otherUnit.Number;

                            if (units.TryGetValue(key, out var unit)) {
                                MergeUnit(unit, otherUnit);
                            }
                            else {
                                str.Units.Add(otherUnit);
                                units.Add(key, otherUnit);
                            }
                        }
                    }
                }
            }
        }

        private void MergeStructure(JStructureInfo str, JStructureInfo other) {
            if (other.Load != null) str.Load = other.Load;
            if (other.SailDirections.Count > 0) str.SailDirections = other.SailDirections;
            if (other.Sailors != null) str.Sailors = other.Sailors;
            if (other.Speed != null) str.Speed = other.Speed;
        }

        private void MergeUnit(JUnit unit, JUnit other) {
            if (other.Faction != null) unit.Faction = other.Faction;
            if (other.Flags.Count > unit.Flags.Count) unit.Flags = other.Flags;
            if (other.Items.Count > unit.Items.Count) unit.Items = other.Items;
            if (other.Weight != null) unit.Weight = other.Weight;
            if (other.Capacity != null) unit.Capacity = other.Capacity;
            if (other.Skills.Count > unit.Skills.Count) unit.Skills = other.Skills;
            if (other.CanStudy.Count > unit.CanStudy.Count) unit.CanStudy = other.CanStudy;
            if (other.ReadyItem != null) unit.ReadyItem = other.ReadyItem;
            if (other.CombatSpell != null) unit.CombatSpell = other.CombatSpell;

            MergeItems(unit, other);
        }

        private static void MergeItems(JUnit unit, JUnit other) {
            var destItems = unit.Items.ToDictionary(x => x.Code);
            var srcItems = other.Items.ToDictionary(x => x.Code);

            foreach (var code in destItems.Keys) {
                if (!srcItems.ContainsKey(code)) {
                    continue;
                }

                var di = destItems[code];
                var si = srcItems[code];

                if ((di.Props ?? "") != (si.Props ?? "") && si.Props?.Length > di.Props?.Length) {
                    di.Props = si.Props;
                }
            }

            foreach (var code in srcItems.Keys.Except(destItems.Keys)) {
                unit.Items.Add(srcItems[code]);
            }
        }
    }
}
