namespace atlantis {
    using atlantis.Persistence;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup {
        public Startup(IConfiguration configuration, IWebHostEnvironment env) {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        public void ConfigureServices(IServiceCollection services) {
            services
                .AddOptions()
                .AddDbContext<Database>(opt => {
                    opt.UseSqlite(Configuration.GetConnectionString("database"));
                    opt.EnableDetailedErrors();
                    opt.EnableSensitiveDataLogging();
                })
                .AddMvcCore()
                    .SetCompatibilityVersion(CompatibilityVersion.Latest);
        }

        public void Configure(IApplicationBuilder app) {
            app
                .UseDeveloperExceptionPage()
                .UseRouting()
                .UseEndpoints(endpoints => {
                    endpoints.MapControllers();
                });
        }
    }
}
