using Microsoft.Extensions.DependencyInjection;
using Tic_Tac_Toe.datasource.repository;
using Tic_Tac_Toe.datasource.service;
using Tic_Tac_Toe.domain.service;

namespace Tic_Tac_Toe.di;

/// Класс для описания графа зависимостей
public static class Configuration
{
    /// Регистрирует все зависимости для работы с играми
    public static void ConfigureDependencies(IServiceCollection services)
    {
        services.AddSingleton<GameStorage>();

        services.AddScoped<IGameRepository, GameRepository>();

        services.AddScoped<IGameService, GameService>();

        services.AddScoped<IGameServiceDataSource, GameServiceDataSource>();
    }
}
