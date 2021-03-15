namespace advisor {
    using System;
    using System.Threading.Tasks;
    using advisor.Authorization;
    using advisor.Persistence;
    using HotChocolate;
    using HotChocolate.AspNetCore;
    using HotChocolate.Execution.Configuration;
    using HotChocolate.Types.Relay;
    using MediatR;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
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
                .AddSingleton<IApiKeyStore, ConfigurationApiKeyStore>()
                .ConfigureApiKeys(Configuration)
                .ConfigureApplicationCookie(options => Configuration.Bind("CookieSettings", options));

            services
                .AddAuthentication()
                .AddApiKeys()
                .AddCookie();

            services
                .AddAuthorization(conf => {
                    var builder = new AuthorizationPolicyBuilder(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        ApiKeyDefaults.AuthenticationScheme
                    );
                    builder.RequireAuthenticatedUser();

                    conf.DefaultPolicy = builder.Build();
                    conf.FallbackPolicy = conf.DefaultPolicy;

                    conf.AddPolicyAnyRole(Policies.Root, Roles.Root);
                    conf.AddPolicyAnyRole(Policies.GameMasters, Roles.Root, Roles.GameMaster);
                    conf.AddPolicyAnyRole(Policies.UserManagers, Roles.Root, Roles.UserManager);
                    conf.AddOwnPlayerPolicy();
                });

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
                });

            services
                .Configure<DatabaseOptions>(opt => {
                    var provider = Configuration.GetValue<DatabaseProvider>("Provider");
                    var connString = Configuration.GetConnectionString("database");

                    opt.Provider = provider;
                    opt.ConnectionString = connString;
                    opt.IsProduction = Env.IsProduction();
                });

            var provider = Configuration.GetValue<DatabaseProvider>("Provider");
            switch (provider) {
                case DatabaseProvider.SQLite:
                    services.AddDbContext<Database, SQLiteDatabase>();
                    break;

                case DatabaseProvider.PgSQL:
                    services.AddDbContext<Database, PgSqlDatabase>();
                    break;

                case DatabaseProvider.MsSQL:
                    services.AddDbContext<Database, MsSqlDatabase>();
                    break;
            }

            services
                .AddAutoMapper(typeof(MappingProfile))
                .AddDataLoaderRegistry()
                .AddGraphQL(SchemaBuilder.New()
                    .EnableRelaySupport()
                    .AddAuthorizeDirectiveType()
                    .AddType<UserType>()
                        .AddType<UserResolvers>()
                    .AddType<GameType>()
                        .AddType<GameResolvers>()
                    .AddType<PlayerType>()
                        .AddType<PlayerResolvers>()
                    .AddType<ReportType>()
                    .AddType<TurnType>()
                        .AddType<TurnResolvers>()
                    .AddType<RegionType>()
                        .AddType<RegionResolvers>()
                    .AddType<UnitType>()
                        .AddType<UnitResolvers>()
                    .AddType<StructureType>()
                        .AddType<StructureResolvers>()
                    .AddType<FactionType>()
                        .AddType<FactionResolvers>()
                    .AddType<UniversityType>()
                        .AddType<UniversityResolvers>()
                    .AddType<StudyPlanType>()
                    .AddType<UniversityClassType>()
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
                .AddScoped<IAuthorizationHandler, OwnPlayerAuthorizationHandler>()
                .AddMediatR(typeof(Startup))
                .AddMvcCore()
                    .AddDataAnnotations()
                    .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddQueryRequestInterceptor((context, builder, ct) => {
                if (!context.User.Identity.IsAuthenticated) {
                    return Task.CompletedTask;
                }

                var userId = context.User.FindFirst(WellKnownClaimTypes.UserId)?.Value;
                if (userId == null) {
                    return Task.CompletedTask;
                }

                builder.AddProperty("currentUserId", long.Parse(userId));
                builder.AddProperty("currentUserEmail", context.User.FindFirst(WellKnownClaimTypes.Email)?.Value);

                return Task.CompletedTask;
            });
        }

        public void Configure(IApplicationBuilder app) {
            if (!Env.IsProduction()) {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseDefaultFiles()
                .UseStaticFiles()
                .UseRouting()
                .UseCors()
                .UseAuthentication()
                .UseAuthorization()
                .UseGraphQL("/graphql")
                .UseEndpoints(endpoints => {
                    endpoints.MapControllers();
                });
        }
    }
}
