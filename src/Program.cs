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
            // if (args == null || args.Length == 0) args = new[] { @"C:\local\var\git-private\atlantis\reports\New-Origins-v5\010_oct_1.rep" };
            if (args == null || args.Length == 0) args = new[] { @"C:\local\bin\games\Atlantis\Advisor\New-Origins\reports\050_feb_5.rep" };

            using var reader = File.OpenText(string.Concat(args));
            using var converter = new AtlantisReportJsonConverter(reader);

            using JsonWriter writer = new JsonTextWriter(Console.Out);
            writer.Formatting = Formatting.Indented;

            await converter.ConvertAsync(writer);
        }
    }
}
