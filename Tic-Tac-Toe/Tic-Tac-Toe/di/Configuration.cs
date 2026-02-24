using Microsoft.Extensions.DependencyInjection;
using Tic_Tac_Toe.datasource.repository;
using Tic_Tac_Toe.datasource.service;
using Tic_Tac_Toe.domain.service;
using Tic_Tac_Toe.web.service;

namespace Tic_Tac_Toe.di;

/// Класс для описания графа зависимостей
public static class Configuration
{
    public static void ConfigureDependencies(IServiceCollection services)
    {
        services.AddScoped<IGameRepository, GameRepository>();

        // Domain service
        services.AddScoped<IGameService, GameService>();

        // Datasource service (расширенный интерфейс с персистентностью)
        services.AddScoped<IGameServiceDataSource, GameServiceDataSource>();

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IAuthService, AuthService>();
    }
}
