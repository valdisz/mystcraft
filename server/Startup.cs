namespace atlantis
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using atlantis.Persistence;
    using HotChocolate;
    using HotChocolate.AspNetCore;
    using HotChocolate.Execution.Configuration;
    using HotChocolate.Types.Relay;
    using Microsoft.AspNetCore.Authentication.Cookies;
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
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => Configuration.Bind("CookieSettings", options));

            services
                .AddAuthorization();

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
                .AddAutoMapper(typeof(MappingProfile))
                .AddDataLoaderRegistry()
                .AddGraphQL(SchemaBuilder.New()
                    .EnableRelaySupport()
                    .AddAuthorizeDirectiveType()
                    .AddType<UserType>()
                    .AddType<GameType>()
                        .AddType<GameResolvers>()
                    .AddType<UserGameType>()
                        .AddType<UserGameResolvers>()
                    .AddType<ReportType>()
                    .AddType<TurnType>()
                        .AddType<TurnResolvers>()
                    .AddType<RegionType>()
                        .AddType<RegionResolvers>()
                    .AddType<UnitType>()
                    .AddType<StructureType>()
                        .AddType<StructureResolvers>()
                    .AddType<FactionType>()
                        .AddType<FactionResolvers>()
                    .AddQueryType<QueryType>()
                    .AddMutationType<Mutation>()
                    .Create(),
                    new QueryExecutionOptions() {
                        IncludeExceptionDetails = true,
                        ForceSerialExecution = true
                    }
                )
                .AddSingleton<IIdSerializer, IdSerializer>()
                .AddSingleton<AccessControl>()
                .AddMvcCore()
                    .AddDataAnnotations()
                    .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddQueryRequestInterceptor((context, builder, ct) => {
                if (context.User.Identity.IsAuthenticated) {
                    var userId = long.Parse(context.User.FindFirst(WellKnownClaimTypes.UserId).Value);
                    var userName = context.User.FindFirst(WellKnownClaimTypes.Email).Value;

                    builder.AddProperty("currentUserId", userId);
                    builder.AddProperty("currentUserName", userName);
                }

                return Task.CompletedTask;
            });
        }

        public void Configure(IApplicationBuilder app, IServiceProvider services) {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetService<Database>();
            db.Database.Migrate();

            app
                .UseDeveloperExceptionPage()
                .UseRouting()
                .UseCors()
                .UseAuthentication()
                .UseAuthorization()
                .UseGraphQL("/graphql")
                .UsePlayground("/graphql")
                .UseEndpoints(endpoints => {
                    endpoints.MapControllers();
                });
        }
    }
}
