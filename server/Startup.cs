namespace advisor
{
    using System;
    using advisor.Authorization;
    using advisor.Persistence;
    using Hangfire;
    using Hangfire.Console;
    using Hangfire.Console.Extensions;
    using Hangfire.Heartbeat;
    using Hangfire.Heartbeat.Server;
    using Hangfire.PostgreSql;
    using Hangfire.RecurringJobAdmin;
    using Hangfire.RecurringJobExtensions;
    using Hangfire.Server;
    using Hangfire.States;
    using Hangfire.Storage.SQLite;
    using HotChocolate;
    using HotChocolate.Execution;
    using MediatR;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Startup {
        public Startup(IConfiguration configuration, IWebHostEnvironment env) {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        public DatabaseProvider DbProvider => Configuration.GetValue<DatabaseProvider>("Provider");
        public string DbConnectionString => Configuration.GetConnectionString("database");
        public string HangfireConnectionString => Configuration.GetConnectionString("hangfire");

        public void ConfigureServices(IServiceCollection services) {
            services
                .AddSingleton<IApiKeyStore, ConfigurationApiKeyStore>()
                .ConfigureApiKeys(Configuration)
                .ConfigureApplicationCookie(options => Configuration.Bind("CookieSettings", options));

            services
                .AddAuthentication()
                .AddApiKeys()
                .AddCookie();

            services.AddHttpsRedirection(options => {
                options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                options.HttpsPort = 443;
            });

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


            services.AddHttpClient();

            services
                .AddOptions()
                .AddCors(opt => {
                    opt.AddDefaultPolicy(builder => builder
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin());
                });

            services
                .Configure<DatabaseOptions>(opt => {
                    opt.Provider = DbProvider;
                    opt.ConnectionString = DbConnectionString;
                    opt.IsProduction = Env.IsProduction();
                });

            switch (DbProvider) {
                case DatabaseProvider.SQLite:
                    services.AddDbContext<Database, SQLiteDatabase>(ServiceLifetime.Transient);
                    break;

                case DatabaseProvider.PgSQL:
                    services.AddDbContext<Database, PgSqlDatabase>(ServiceLifetime.Transient);
                    break;

                case DatabaseProvider.MsSQL:
                    services.AddDbContext<Database, MsSqlDatabase>(ServiceLifetime.Transient);
                    break;
            }

            services.AddHangfire(conf => {
                conf.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
                conf.UseSimpleAssemblyNameTypeSerializer();
                conf.UseRecommendedSerializerSettings();

                switch (DbProvider) {
                    case DatabaseProvider.SQLite:
                        conf.UseSQLiteStorage(HangfireConnectionString);
                        break;

                    case DatabaseProvider.PgSQL:
                        conf.UsePostgreSqlStorage(HangfireConnectionString);
                        break;

                    case DatabaseProvider.MsSQL:
                        conf.UseSqlServerStorage(HangfireConnectionString);
                        break;
                }

                conf.UseConsole();
                conf.UseRecurringJobAdmin(typeof(Startup).Assembly);
                conf.UseHeartbeatPage(TimeSpan.FromSeconds(5));

                conf.UseRecurringJob(typeof(RemoteGameServerJobs));
            });

            GlobalJobFilters.Filters.Add(new JoiningSupportAttribute(new BackgroundJobStateChanger()));
            GlobalStateHandlers.Handlers.Add(new JoiningState.Handler());

            // services
            //     .AddHangfireServer()
            //     .AddHangfireConsoleExtensions()
            //     .AddSingleton<IBackgroundProcess, ProcessMonitor>(_ => new ProcessMonitor(TimeSpan.FromSeconds(5)));

            services
                .AddAutoMapper(typeof(MappingProfile));

            services
                .AddGraphQLServer()
                .AddHttpRequestInterceptor<GraphQLHttpRequestInterceptor>()
                .ModifyRequestOptions(opt => {
                    opt.IncludeExceptionDetails = !Env.IsProduction();
                })
                .ModifyOptions(opt => {
                    opt.DefaultResolverStrategy = ExecutionStrategy.Serial;
                })
                .ConfigureResolverCompiler(r => {
                    r.AddService<Database>();
                    r.AddService<IMediator>();
                    r.AddService<IAuthorizationService>();
                })
                .AddApolloTracing()
                .AddQueryType<QueryType>()
                .AddMutationType<MutationType>()
                .AddGlobalObjectIdentification()
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
                .AddType<EventType>()
                .AddType<MutationResult<string>>()
                // .AddType<UniversityType>()
                //     .AddType<UniversityResolvers>()
                // .AddType<StudyPlanType>()
                // .AddType<UniversityClassType>()
                .BindRuntimeType<Item, ItemType>()
                .BindRuntimeType<DbUnitItem, ItemType>()
                .BindRuntimeType<DbProductionItem, ItemType>()
                .BindRuntimeType<DbStatItem, ItemType>()
                ;

                // .AddDataLoaderRegistry()
                // .AddGraphQL(SchemaBuilder.New()
                //     .EnableRelaySupport()
                //     .Create(),
                //     new QueryExecutionOptions() {
                //         IncludeExceptionDetails = true,
                //         ForceSerialExecution = true
                //     }
                // )
                // .AddSingleton<IIdSerializer, IdSerializer>()

            services
                .AddSingleton<AccessControl>()
                .AddScoped<IAuthorizationHandler, OwnPlayerAuthorizationHandler>()
                .AddMediatR(typeof(Startup))
                .AddMvcCore()
                    .AddDataAnnotations()
                    .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services
                .AddMemoryCache();
        }

        public void Configure(IApplicationBuilder app) {
            if (!Env.IsProduction()) {
                app.UseDeveloperExceptionPage();
            }

            // if (Env.IsProduction()) {
            //     app.UseHsts();
            //     app.UseHttpsRedirection();
            // }

            app
                .UseMiddleware<DefaultFilesMiddleware>()
                .UseStaticFiles()
                .UseRouting()
                .UseCors()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints => {
                    endpoints.MapControllers();
                    endpoints
                        .MapGraphQL();
                    // endpoints.MapHangfireDashboard(new DashboardOptions {
                    //     Authorization = new[] {
                    //         new RoleBasedDashboardAuthorizationFilter(Roles.Root)
                    //     }
                    // });
                });
        }
    }
}
