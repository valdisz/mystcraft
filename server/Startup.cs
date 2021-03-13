namespace atlantis {
    using System;
    using System.Threading.Tasks;
    using atlantis.Features;
    using atlantis.Persistence;
    using HotChocolate;
    using HotChocolate.AspNetCore;
    using HotChocolate.Execution.Configuration;
    using HotChocolate.Types.Relay;
    using MediatR;
    using Microsoft.AspNetCore.Authentication;
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
                .AddAuthorization(conf => {
                    conf.AddPolicyAny(Roles.Root, Roles.Root);
                    conf.AddPolicyAny(Roles.GameMaster, Roles.Root, Roles.GameMaster);
                    conf.AddPolicyAny(Roles.UserManager, Roles.Root, Roles.UserManager);
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
                .AddMediatR(typeof(Startup))
                .AddMvcCore()
                    .AddDataAnnotations()
                    .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddQueryRequestInterceptor(async (context, builder, ct) => {
                if (!context.User.Identity.IsAuthenticated) {
                    return;
                }

                var userId = context.User.FindFirst(WellKnownClaimTypes.UserId)?.Value;
                if (userId == null) {
                    await context.SignOutAsync();
                    return;
                }

                builder.AddProperty("currentUserId", long.Parse(userId));
                builder.AddProperty("currentUserEmail", context.User.FindFirst(WellKnownClaimTypes.Email)?.Value);
            });
        }

        public void Configure(IApplicationBuilder app, IServiceProvider services) {
            Task.Run(async () => {
                using var scope = services.CreateScope();
                await BeforeStartAsync(scope.ServiceProvider);
            }).Wait();

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

        public async Task BeforeStartAsync(IServiceProvider services) {
            var db = services.GetService<Database>();
            await db.Database.MigrateAsync();

            if (! await db.Users.AnyAsync()) {
                var mediator = services.GetService<IMediator>();

                var email = Configuration.GetValue<string>("Seed:Email");
                var password = Configuration.GetValue<string>("Seed:Password");

                await mediator.Send(new CreateUser(email, password, Roles.Root));
            }
        }
    }
}
