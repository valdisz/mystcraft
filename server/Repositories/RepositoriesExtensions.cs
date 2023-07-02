namespace advisor;
using Microsoft.Extensions.DependencyInjection;

public static class RepositoriesExtensions {
    public static IServiceCollection AddRepositories(this IServiceCollection services) {
        services.AddSingleton<ITime, DefaultTime>();

        services.AddScoped<IAllGamesRepository, GameRepository>();
        services.AddScoped<IGameEnginesRepository, GameEnginesRepository>();
        services.AddScoped<IAllianceRepository, AllianceRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IStatisticsRepository, StatisticsRepository>();
        services.AddScoped<IStudyPlanRepository, StudyPlanRepository>();
        services.AddScoped<IPlayerTurnRepository, PlayerTurnRepository>();

        return services;
    }
}
