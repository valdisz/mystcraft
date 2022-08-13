namespace advisor {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using advisor.Authorization;
    using advisor.Features;
    using advisor.Model;
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
    using HotChocolate.Types;
    using HotChocolate.Types.Pagination;
    using MediatR;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
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
            var discord = Configuration.GetSection("Discord");
            var discordOAuth = discord.GetSection("OAuth");

            var proxy = Configuration.GetSection("Proxy");

            services.Configure<DiscordOptions>(discord);
            services.Configure<ForwardedHeadersOptions>(opt => {
                opt.ForwardedHeaders = proxy.GetValue<ForwardedHeaders>("ForwardedHeaders");

                opt.AllowedHosts.Clear();
                var allowedHosts = proxy.GetSection("AllowedHosts").Get<string[]>();
                foreach (var h in (allowedHosts ?? Enumerable.Empty<string>())) {
                    opt.AllowedHosts.Add(h);
                }

                IPAddress resolveProxy(string ipOrDomain) {
                    if (IPAddress.TryParse(ipOrDomain, out var ip)) {
                        return ip;
                    }

                    var rec = Dns.GetHostEntry(ipOrDomain);
                    return rec.AddressList[0];
                }

                opt.KnownProxies.Clear();
                var knownProxies = proxy.GetSection("KnownProxies").Get<string[]>();
                foreach (var ip in (knownProxies ?? Enumerable.Empty<string>()).Select(resolveProxy)) {
                    opt.KnownProxies.Add(ip);
                }

                IPNetwork parseNetwork(string s) {
                    var parts = s.Split("/");

                    var ip = IPAddress.Parse(parts[0]);
                    var len = int.Parse(parts[1]);

                    return new IPNetwork(ip, len);
                }

                opt.KnownNetworks.Clear();
                var knownNetworks = proxy.GetSection("KnownNetworks").Get<string[]>();
                foreach (var network in (knownNetworks ?? Enumerable.Empty<string>()).Select(parseNetwork)) {
                    opt.KnownNetworks.Add(network);
                }
            });

            services.AddResponseCompression(opt => {
                opt.EnableForHttps = true;
            });

            services
                .AddSingleton<IApiKeyStore, ConfigurationApiKeyStore>()
                .ConfigureApiKeys(Configuration)
                .ConfigureApplicationCookie(options => Configuration.Bind("CookieSettings", options));

            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opt => {
                    opt.SlidingExpiration = true;
                    opt.ExpireTimeSpan = TimeSpan.FromDays(30);
                    opt.Cookie.MaxAge = TimeSpan.FromDays(30);
                })
                .AddCookie(ExternalAuthentication.AuthenticationScheme, opt => {
                    opt.Cookie.MaxAge = null;
                    opt.SlidingExpiration = false;
                })
                .AddApiKeys()
                .AddOAuth(DiscordDefaults.AuthenticationScheme, opt => {
                    discordOAuth.Bind(opt);

                    opt.SignInScheme = ExternalAuthentication.AuthenticationScheme;
                    opt.SaveTokens = true;
                });

            services.AddDataProtection()
                .PersistKeysToFileSystem(new System.IO.DirectoryInfo(Configuration.GetValue<string>("DataProtection:Path")));

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

                // conf.UseRecurringJob(typeof(RemoteGameServerJobs));
                conf.UseRecurringJob(typeof(MaintenanceJobs));
            });

            // GlobalJobFilters.Filters.Add(new JoiningSupportAttribute(new BackgroundJobStateChanger()));
            // GlobalStateHandlers.Handlers.Add(new JoiningState.Handler());

            services
                .AddHangfireServer()
                .AddHangfireConsoleExtensions()
                .AddSingleton<IBackgroundProcess, ProcessMonitor>(_ => new ProcessMonitor(TimeSpan.FromSeconds(5)));

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
                .SetPagingOptions(new PagingOptions {
                    DefaultPageSize = 100,
                    MaxPageSize = 100,
                    IncludeTotalCount = true
                })
                .AddQueryType<QueryType>()
                .AddMutationType<Mutation>()
                .AddGlobalObjectIdentification()
                .AddType<UploadType>()
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
                .AddType<AllianceType>()
                .AddType<AllianceMemberType>()
                    .AddType<AllianceMemberResolvers>()
                .AddType<GameEngineType>()
                .BindRuntimeType<Item, ItemType>()
                .BindRuntimeType<DbUnitItem, ItemType>()
                .BindRuntimeType<DbProductionItem, ItemType>()
                .BindRuntimeType<DbStatItem, ItemType>()
                .BindRuntimeType<JItem, ItemType>()
                .BindRuntimeType<DbSkill, SkillType>()
                .BindRuntimeType<JSkill, SkillType>()
                ;

            services
                .AddSingleton<AccessControl>()
                .AddScoped<IAuthorizationHandler, OwnPlayerAuthorizationHandler>()
                .AddMediatR(typeof(Startup))
                .AddMvcCore()
                    .AddDataAnnotations()
                    .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services
                .AddMemoryCache();

            services.AddTransient<TurnReProcessJob>();
        }

        public void Configure(IApplicationBuilder app) {
            app.UseForwardedHeaders();

            app.UseResponseCompression();

            if (!Env.IsProduction()) {
                app.UseDeveloperExceptionPage();
            }

            if (Env.IsProduction()) {
                app.UseHsts();
            }

            app
                .UseMiddleware<DefaultFilesMiddleware>()
                .UseStaticFiles()
                .UseRouting()
                .UseCors()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints => {
                    endpoints.MapControllers();

                    endpoints.MapGraphQL();

                    endpoints.MapHangfireDashboard(new DashboardOptions {
                        Authorization = new[] {
                            new RoleBasedDashboardAuthorizationFilter(Roles.Root)
                        }
                    });
                });
        }
    }
}
