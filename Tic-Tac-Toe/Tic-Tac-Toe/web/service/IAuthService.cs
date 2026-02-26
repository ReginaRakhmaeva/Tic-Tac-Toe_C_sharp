using Tic_Tac_Toe.web.model;

namespace Tic_Tac_Toe.web.service;

/// Интерфейс сервиса авторизации
public interface IAuthService
{
    bool Register(SignUpRequest request);

    JwtResponse? Authenticate(JwtRequest request);

    JwtResponse? RefreshAccessToken(string refreshToken);

    JwtResponse? RefreshRefreshToken(string refreshToken);
}
