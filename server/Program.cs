namespace atlantis {
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    class Program {
        public static Task Main(string[] args) {
            if (args == null || args.Length == 0) {
                return RunServerAsync(args);
            }
            else {
                return RunConverterAsync(args);
            }
        }

        public static Task RunServerAsync(string[] args) {
            var builder = new WebHostBuilder()
                .ConfigureAppConfiguration(conf => {
                    conf
                        .AddJsonFile("appsettings.json")
                        .AddCommandLine(args);
                })
                .UseStartup<Startup>()
                .UseKestrel();

            var host = builder.Build();

            return host.RunAsync();
        }

        public static async Task RunConverterAsync(string[] args) {
            using var reader = File.OpenText(string.Concat(args));
            using var converter = new AtlantisReportJsonConverter(reader);

            using JsonWriter writer = new JsonTextWriter(Console.Out);
            writer.Formatting = Formatting.Indented;

            await converter.ConvertAsync(writer);
        }
    }
}
