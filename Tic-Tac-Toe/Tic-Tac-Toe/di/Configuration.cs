using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tic_Tac_Toe.datasource.repository;
using Tic_Tac_Toe.datasource.service;
using Tic_Tac_Toe.domain.service;
using Tic_Tac_Toe.web.service;

namespace Tic_Tac_Toe.di;

/// Класс для описания графа зависимостей
public static class Configuration
{
    public static void ConfigureDependencies(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IGameRepository, GameRepository>();

        services.AddScoped<IGameService, GameService>();

        services.AddScoped<IGameServiceDataSource, GameServiceDataSource>();

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IUserService, UserService>();

        ConfigureJwt(services, configuration);

        services.AddHttpContextAccessor();

        services.AddScoped<IAuthService, AuthService>();
    }

    /// Инкапсулирует всю конфигурацию JWT: чтение настроек, регистрацию JwtProvider и настройку Bearer-аутентификации.
    public static void ConfigureJwt(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecretKey = configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "Tic-Tac-Toe";
        var jwtAudience = configuration["Jwt:Audience"] ?? "Tic-Tac-Toe-Users";
        var accessTokenExpirationMinutes = int.Parse(configuration["Jwt:AccessTokenExpirationMinutes"] ?? "15");
        var refreshTokenExpirationDays = int.Parse(configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

        services.AddSingleton<JwtProvider>(sp => new JwtProvider(
            jwtSecretKey,
            jwtIssuer,
            jwtAudience,
            accessTokenExpirationMinutes,
            refreshTokenExpirationDays));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });
    }
}
