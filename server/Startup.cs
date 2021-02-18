namespace atlantis {
    using System;
    using atlantis.Persistence;
    using HotChocolate;
    using HotChocolate.AspNetCore;
    using HotChocolate.Execution.Configuration;
    using HotChocolate.Types.Relay;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class Startup {
        public Startup(IConfiguration configuration, IWebHostEnvironment env) {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        public void ConfigureServices(IServiceCollection services) {
            services
                .AddLogging(builder => builder
                    .AddConsole()
                    .AddDebug()
                )
                .AddOptions()
                .AddCors(opt => {
                    opt.AddDefaultPolicy(builder => builder
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin());
                })
                .AddDbContext<Database>(opt => {
                    opt.UseSqlite(Configuration.GetConnectionString("database"));
                    opt.EnableDetailedErrors();
                    opt.EnableSensitiveDataLogging();
                })
                .AddGraphQL(SchemaBuilder.New()
                    .EnableRelaySupport()
                    .AddType<GameType>()
                    .AddType<GameResolvers>()
                    .AddType<ReportType>()
                    .AddType<TurnType>()
                    .AddType<TurnResolvers>()
                    .AddType<RegionType>()
                    .AddQueryType<QueryType>()
                    .AddMutationType<Mutation>()
                    .Create(),
                    new QueryExecutionOptions() {
                        IncludeExceptionDetails = true,
                        ForceSerialExecution = true
                    }
                )
                .AddSingleton<IIdSerializer, IdSerializer>()
                .AddMvcCore()
                    .AddDataAnnotations()
                    .SetCompatibilityVersion(CompatibilityVersion.Latest);
        }

        public void Configure(IApplicationBuilder app, IServiceProvider services) {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetService<Database>();
            db.Database.Migrate();

            app
                .UseDeveloperExceptionPage()
                .UseRouting()
                .UseCors()
                .UseGraphQL("/graphql")
                .UsePlayground("/graphql")
                .UseEndpoints(endpoints => {
                    endpoints.MapControllers();
                });
        }
    }
}
