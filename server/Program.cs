namespace advisor {
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using advisor.Features;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
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

        public static async Task RunServerAsync(string[] args) {
            var builder = new WebHostBuilder()
                .ConfigureAppConfiguration(conf => {
                    conf
                        .AddJsonFile("appsettings.json")
                        .AddEnvironmentVariables("ADVISOR_")
                        .AddCommandLine(args);
                })
                .UseStartup<Startup>()
                .ConfigureKestrel(conf => {
                    conf.AllowSynchronousIO = true;
                })
                .UseKestrel();

            var host = builder.Build();

            using (var scope = host.Services.CreateScope()) {
                var services = scope.ServiceProvider;
                var config = services.GetRequiredService<IConfiguration>();

                await BeforeStartAsync(services, config);
            }

            await host.RunAsync();
        }

        public static async Task RunConverterAsync(string[] args) {
            using var reader = File.OpenText(string.Concat(args));
            using var converter = new AtlantisReportJsonConverter(reader);

            using JsonWriter writer = new JsonTextWriter(Console.Out);
            writer.Formatting = Formatting.Indented;

            await converter.ReadAsJsonAsync(writer);
        }

        public static async Task BeforeStartAsync(IServiceProvider services, IConfiguration conf) {
            var db = services.GetService<Database>();
            await db.Database.MigrateAsync();

            if (! await db.Users.AnyAsync()) {
                var mediator = services.GetService<IMediator>();

                var email = conf.GetValue<string>("Seed:Email");
                var password = conf.GetValue<string>("Seed:Password");

                await mediator.Send(new CreateUser(email, password, Policies.Root));
            }
        }
    }
}
