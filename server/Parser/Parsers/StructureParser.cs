namespace atlantis {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    // + Building [5] : Stables; Imperial Stables.
    // + AE Mayfield [286] : Fleet, 2 Cogs; Load: 485/1000; Sailors: 12/12; MaxSpeed: 4.
    // + AE Mayfield [286] : Fleet, 2 Cogs, 1 Longship; Load: 485/1000; Sailors: 12/12; MaxSpeed: 4.
    // + AE Empire [246] : Fleet, 10 Corsairs; Load: 5593/10000; Sailors: 135/150; MaxSpeed: 0; Sail directions: S, SW.
    // + AE Triangulum [329] : Fleet, 3 Corsairs; Sail directions: S, SW; Shiny new corsairs ready to engage any enemy. Built bay Imperial Shipyards..
    // + Shaft [1] : Shaft, contains an inner location.
    // + Lair [1] : Lair, closed to player units.
    // + Tower [1] : Tower, needs 10.
    // + Tower [1] : Tower, engraved with Runes of Warding.
    // + Ruin [1] : Ruin, closed to player units.
    // + The Kings Highway [1] : Road N.
    // + Trade Academy [NIMB] [Nort Triders] [2] : Tower; comment.
    // + {Name} [{Number}] : {Type}, {Int} {Type}, {Flag}; {Key}: {Value}; {Description}.
    public class StructureParser : BaseParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var nameAndNumber = p.After("+").SkipWhitespaces().Before("] : ");  // lets hope noone will use this combination in their building names
            if (!nameAndNumber) return Error(nameAndNumber);

            var name = nameAndNumber.BeforeBackwards("[").SkipWhitespacesBackwards().AsString();
            if (!name) return Error(name);

            var number = nameAndNumber.After("[").Integer();
            if (!number) return Error(number);

            p.After("] :").SkipWhitespaces();

            Queue<TextParser> props = new Queue<TextParser>();
            if (!p.EOF) {
                var props1 = p
                    .Before(";")
                    .RecoverWith(() => p.BeforeBackwards("."))
                    .List(",", item => item.SkipWhitespaces());

                var props2 = p
                    .After(";")
                    .BeforeBackwards(".")
                    .List(";", item => item.SkipWhitespaces());

                if (props1)
                    foreach (var prop in props1.Value)
                        props.Enqueue(prop);

                if (props2)
                    foreach (var prop in props2.Value)
                        props.Enqueue(prop);
            }

            var type = props.Dequeue().AsString();

            var structure = ReportNode.Object(
                ReportNode.Str("name", name),
                ReportNode.Int("number", number),
                ReportNode.Str("type", type)
            );

            if (type.Equals("fleet", StringComparison.OrdinalIgnoreCase)) {
                var contents = ReportNode.Array();
                structure.Add(ReportNode.Key("contents", contents));

                while (props.Count > 0) {
                    var item = props.Peek();
                    item.PushBookmark();

                    var objCount = item.Integer();
                    var objType = item.SkipWhitespaces().AsString();

                    if (!objCount || !objType) {
                        item.PopBookmark();
                        break;
                    }
                    else {
                        contents.Add(ReportNode.Object(
                            ReportNode.Int("count", objCount),
                            ReportNode.Str("type", objType)
                        ));
                        props.Dequeue();
                    }
                }
            }

            var flags = ReportNode.Array();
            structure.Add(ReportNode.Key("flags", flags));

            List<TextParser> unknownProps = new List<TextParser>();
            foreach (var prop in props) {
                var knownProp = prop.OneOf(
                    x => x.After("needs")
                        .SkipWhitespaces()
                        .Integer()
                        .Map(v => ReportNode.Int("needs", v)),
                    x => {
                        var targetProp = x.After("Load:").SkipWhitespaces();
                        if (!targetProp) return targetProp.Convert<IReportNode>();

                        var value = targetProp.Before("/").Integer();
                        if (!value) return value.Convert<IReportNode>();

                        var max = targetProp.After("/").Integer();
                        if (!max) return max.Convert<IReportNode>();

                        return new Maybe<IReportNode>(ReportNode.Key("load", ReportNode.Object(
                            ReportNode.Int("used", value),
                            ReportNode.Int("max", max)
                        )));
                    },
                    x => {
                        var targetProp = x.After("Sailors:").SkipWhitespaces();
                        if (!targetProp) return targetProp.Convert<IReportNode>();

                        var value = targetProp.Before("/").Integer();
                        if (!value) return value.Convert<IReportNode>();

                        var max = targetProp.After("/").Integer();
                        if (!max) return max.Convert<IReportNode>();

                        return new Maybe<IReportNode>(ReportNode.Key("sailors", ReportNode.Object(
                            ReportNode.Int("current", value),
                            ReportNode.Int("required", max)
                        )));
                    },
                    x => x.After("MaxSpeed:")
                        .SkipWhitespaces()
                        .Integer()
                        .Map(v => ReportNode.Int("speed", v)),
                    x => x.After("Sail directions:")
                        .SkipWhitespaces()
                        .List(",", item => item.SkipWhitespaces())
                        .Map(v => ReportNode.Key("sailDirections",ReportNode.Array(
                            v.Select(sd => ReportNode.Str(sd.AsString()))
                        )))
                );
                if (knownProp) {
                    structure.Add(knownProp.Value);
                    continue;
                }

                var knownFlag = prop.OneOf(
                    x => x.Match("closed to player units"),
                    x => x.Match("contains an inner location"),
                    x => x.Match("engraved with runes of warding")
                ).AsString();
                if (knownFlag) {
                    flags.Add(ReportNode.Str(knownFlag.Value));
                    continue;
                }

                unknownProps.Add(prop);
            }

            if (unknownProps.Count > 0) {
                structure.Add(ReportNode.Str("description", string.Join("; ", unknownProps.Select(x => x.AsString()))));
            }

            return Ok(structure);
        }
    }
}
