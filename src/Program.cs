namespace atlantis
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Pidgin;
    using static Tokens;



    class Program
    {
        static void Main(string[] args)
        {
            var input = File.ReadAllText(@"C:\local\var\git-private\atlantis\report");

            Result<char, IReportNode> result = null;
            result = RegionParser.Report.Parse(input);
            AssertParsed(result);

            if (result.Success) {
                Console.WriteLine(result.Value.ToString());
                Console.WriteLine();
                Console.WriteLine("OK");
            }
        }

        public static void AssertParsed<T>(Result<char, T> res) {
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
            }
        }
    }
}
