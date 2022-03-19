namespace advisor {
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using advisor.Features;
    using advisor.Persistence;
    using Hangfire;
    using MediatR;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
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

        public static IWebHostBuilder CreateHostBuilder(string[] args) {
            var builder = new WebHostBuilder()
                .ConfigureAppConfiguration((hostingContext, conf) => {
                    conf.Sources.Clear();
                    IHostEnvironment env = hostingContext.HostingEnvironment;

                    conf
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName.ToLower()}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables("ADVISOR_")
                        .AddCommandLine(args ?? new string[0]);
                })
                .ConfigureServices((context, services) => {
                    services.AddLogging(conf => {
                        conf.AddConsole();
                        conf.AddDebug();
                    });
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                })
                .UseStartup<Startup>()
                .UseKestrel((hostingContext, conf) => {
                    conf.Configure(hostingContext.Configuration.GetSection("Kestrel"));
                    conf.AllowSynchronousIO = true;
                })
                .UseWebRoot(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));

            return builder;
        }

        public static async Task RunServerAsync(string[] args) {
            var host = CreateHostBuilder(args).Build();

            await RunDatabaseMigrationsAsync(host);
            await ConfigureRecurringJobsTasks(host);

            await host.RunAsync();
        }

        private static async Task ConfigureRecurringJobsTasks(IWebHost host) {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var conf = services.GetRequiredService<IConfiguration>();
            var db = services.GetRequiredService<Database>();
            var jobs = services.GetRequiredService<IRecurringJobManager>();

            var games = await db.Games
                .Where(x => x.Type == Persistence.GameType.Remote)
                .ToListAsync();

            foreach (var game in games.Where(x => x.Options.Schedule != null)) {
                jobs.AddOrUpdate<RemoteGameServerJobs>(
                    $"game-{game.Id}",
                    x => x.NewOrigins(game.Id),
                    game.Options.Schedule,
                    TimeZoneInfo.FindSystemTimeZoneById(game.Options.TimeZone ?? "America/Los_Angeles")
                );
            }
        }

        private static async Task RunDatabaseMigrationsAsync(IWebHost host) {
            using (var scope = host.Services.CreateScope()) {
                var services = scope.ServiceProvider;
                var conf = services.GetRequiredService<IConfiguration>();

                var db = services.GetService<Database>();
                await db.Database.MigrateAsync();

                if (! await db.Users.AnyAsync()) {
                    var mediator = services.GetService<IMediator>();

                    var email = conf.GetValue<string>("Seed:Email");
                    var password = conf.GetValue<string>("Seed:Password");

                    await mediator.Send(new UserCreate(email, password, Policies.Root));
                }
            }
        }

        public static async Task RunConverterAsync(string[] args) {
            using var reader = File.OpenText(string.Concat(args));
            await ConvertAsync(reader);
        }

        public static async Task ConvertAsync(TextReader reader) {
            using var converter = new AtlantisReportJsonConverter(reader);

            using JsonWriter writer = new JsonTextWriter(Console.Out);
            writer.Formatting = Formatting.Indented;

            await converter.ReadAsJsonAsync(writer);
        }
    }
}
