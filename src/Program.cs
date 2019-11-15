namespace atlantis
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Pidgin;

    class Program
    {
        public static async Task Main(string[] args)
        {
            // var ip = new ItemParser();
            // var result = ip.Parse(new TextParser("2 high elves [HELF] at $48"));


            // if (args == null || args.Length == 0) {
            //     Console.WriteLine("Give path to the report file as an argument");
            //     return;
            // }

            // string filePath = @"C:\local\var\git-private\atlantis\reports\New-Origins-v5\010_oct_1.rep";
            // string filePath = string.Concat(args);
            string filePath = @"C:\local\var\git-private\atlantis\reports\15_merged_0309.rep";
            using var input = File.OpenText(filePath);
            var repReader = new AtlantisReportReader();

            DateTime started = DateTime.Now;
            await repReader.ReadAsync(input);
            Console.WriteLine($"Duration {(DateTime.Now - started).TotalMilliseconds}ms");

            // var input = File.ReadAllText(string.Concat(args));

            // Result<char, IReportNode> result = null;
            // result = AtlantisParser.Report.Parse(input);
            // var report = AssertParsed(result);

            // using JsonWriter writer = new JsonTextWriter(Console.Out);
            // writer.Formatting = Formatting.Indented;

            // report.WriteJson(writer);

            // writer.Flush();
        }

        public static IReportNodeOld AssertParsed(Result<char, IReportNodeOld> res) {
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
