namespace atlantis
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using Pidgin;
    using static Tokens;

    class Program
    {
        public static void Parse() {
string input = @"+ Fleet [121] : Fleet, 1 Longship, 1 Cog; Load: 580/600; Sailors:
  10/10; MaxSpeed: 4.
  - captain fleet 121 (3940), Anuii (44), avoiding, behind, revealing
    faction, weightless battle spoils, 2 gnomes [GNOM], 11 stone
    [STON]. Weight: 560. Capacity: 0/0/18/0. Skills: sailing [SAIL] 2
    (90).
  - Unit (6795), Anuii (44), avoiding, behind, revealing faction,
    weightless battle spoils, 2 lizardmen [LIZA]. Weight: 20.
    Capacity: 0/0/30/30. Skills: sailing [SAIL] 3 (195).
";

            var result = AtlantisParser.StructureWithUnits.Parse(input);
            AssertParsed(result);

            if (result.Success) {
                Console.WriteLine(result.Value.ToString());
                Console.WriteLine();
                Console.WriteLine("OK");
            }
        }

        static void Main(string[] args)
        {
            if (args == null || args.Length == 0) {
                Console.WriteLine("Give path to the report file as an argument");
                return;
            }

            var input = File.ReadAllText(string.Concat(args));

            Result<char, IReportNode> result = null;
            result = AtlantisParser.Report.Parse(input);
            var report = AssertParsed(result);

            using JsonWriter writer = new JsonTextWriter(Console.Out);
            writer.Formatting = Formatting.Indented;

            report.WriteJson(writer);

            writer.Flush();
        }

        public static IReportNode AssertParsed(Result<char, IReportNode> res) {
            if (!res.Success) {
                if (!string.IsNullOrWhiteSpace(res.Error.Message)) {
                    Console.WriteLine(res.Error.Message);
                }

                if (res.Error.EOF) {
                    Console.WriteLine("Reached EOF");
                }

                var unex = res.Error.Unexpected;
                var pos = res.Error.ErrorPos;

                Console.WriteLine($"Error at (Ln {pos.Line}, Col {pos.Col})");

                foreach (var e in res.Error.Expected) {
                    var tokens = e.Tokens ?? Enumerable.Empty<char>();
                    var expected = tokens.Any()
                        ? string.Join("", tokens)
                        : e.Label;

                    Console.WriteLine($"Expected \"{expected}\"");
                }

                if (unex.HasValue) {
                    Console.WriteLine($"Found \"{unex.Value}\"");
                }

                Environment.Exit(1);
            }

            return res.Value;
        }
    }
}
