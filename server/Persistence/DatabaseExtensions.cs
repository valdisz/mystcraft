namespace advisor.Persistence;

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class DatabaseExtensions {
    public static IServiceCollection AddDatabase(this IServiceCollection services, DatabaseOptions options) {
        services.AddTransient<TimeInterceptor>();
        services.AddTransient<UserInterceptor>();

        void configureDatabase(IServiceProvider provider, DbContextOptionsBuilder builder) {
            switch (options.Provider) {
                case DatabaseProvider.SQLite:
                    builder.UseSqlite(options.ConnectionString);
                    break;

                case DatabaseProvider.PgSQL:
                    builder.UseNpgsql(options.ConnectionString);
                    break;

                case DatabaseProvider.MsSQL:
                    builder.UseSqlServer(options.ConnectionString);
                    break;
            }

            builder.AddInterceptors(provider.GetRequiredService<TimeInterceptor>());
            builder.AddInterceptors(provider.GetRequiredService<UserInterceptor>());
            builder.UseLoggerFactory(provider.GetRequiredService<ILoggerFactory>());

            if (!options.IsProduction) {
                builder.EnableDetailedErrors();
                builder.EnableSensitiveDataLogging();
            }

            if (options.IsProduction) {
                builder.ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug)));
            }
        }

        switch (options.Provider) {
            case DatabaseProvider.SQLite:
                services.AddDbContext<Database, SQLiteDatabase>(configureDatabase, ServiceLifetime.Transient);
                break;

            case DatabaseProvider.PgSQL:
                services.AddDbContext<Database, PgSqlDatabase>(configureDatabase, ServiceLifetime.Transient);
                break;

            case DatabaseProvider.MsSQL:
                services.AddDbContext<Database, MsSqlDatabase>(configureDatabase, ServiceLifetime.Transient);
                break;
        }

        services.AddScoped<UnitOfWork>(ctx => ctx.GetRequiredService<Database>());
        services.AddTransient<DatabaseIO>(ctx => ctx.GetRequiredService<Database>());

        return services;
    }
}
