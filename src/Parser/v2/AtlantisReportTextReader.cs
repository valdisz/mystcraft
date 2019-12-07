namespace atlantis
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Pidgin;

    public class AtlantisReportTextReader {
        public async Task ReadAsync(TextReader report) {
            var blockReader = new AtlantisTextReader(report);
            var classification = new AtlantisReportConverter(blockReader.ReadAllAsync());

            ActionBlock<TextParser> parseUnit = new ActionBlock<TextParser>(p => {
                UnitParser unitParser = new UnitParser(new SkillParser());
                unitParser.Parse(p);
            }, new ExecutionDataflowBlockOptions {
                MaxDegreeOfParallelism = 4
            });

            TransformBlock<string, TextParser> toTextParser = new TransformBlock<string, TextParser>(s =>
                Task.FromResult(new TextParser(1, s)),
                new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = 4,
                    SingleProducerConstrained = true
                }
            );

            toTextParser.LinkTo(parseUnit, new DataflowLinkOptions {
                PropagateCompletion = true
            });

            await foreach (var line in UnitBlocks(classification)) {
                await toTextParser.SendAsync(line);
            }
            toTextParser.Complete();

            await parseUnit.Completion;
        }

        private async IAsyncEnumerable<string> UnitBlocks(AtlantisReportConverter rm) {

            await foreach (var block in rm.CategorizeAsync()) {
                Console.WriteLine($"{block.Type} with {block.Lines.Count()} blocks");
                if (block.Type == ReportState.Region) {
                    foreach (var unitBlock in block.Items.Where(b => b.Type == ReportState.Unit || b.Type == ReportState.OwnUnit)) {
                        foreach (var line in unitBlock.Lines) {
                            yield return line.Text;
                        }
                    }
                }
            }
        }
    }

    public static class AtlantisParsers {
        public static bool? ParseIsOwn(this TextParser p) {
            bool own = false;
            switch (p.Text.Span[0]) {
                case '*':
                    own = true;
                    break;
                case '-':
                    own = false;
                    break;
                default:
                    return null;
            }

            p.Seek(1).SkipWhitespaces();
            return own;
        }

        public static (string, int)? ParseUnitName(this TextParser p) {
            p.PushBookmark();

            var name = p.Before("(")?.ToString();
            if (name == null) {
                p.PopBookmark();
                return null;
            }

            var number = p.Between("(", ")").Integer();
            if (number == null) {
                p.PopBookmark();
                return null;
            }

            p.RemoveBookmark();

            return (name, number.Value);
        }

        public static (string, int)? ParseFactionName(this TextParser p) {
            p.PushBookmark();

            var name = p.Before("(")?.ToString();
            if (name == null) {
                p.PopBookmark();
                return null;
            }

            var number = p.Between("(", ")").Integer();
            if (number == null) {
                p.PopBookmark();
                return null;
            }

            p.RemoveBookmark();

            return (name, number.Value);
        }

        public static void ParseUnit(this TextParser p) {
            p.ParseIsOwn();
            p.ParseUnitName();
            p.After(",").SkipWhitespaces();

            TextParser attr;
            while ((attr = p.Before(",")) != null) {
                attr.ParseFactionName();
            }
        }
    }
}
