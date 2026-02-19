using Tic_Tac_Toe.web.model;

namespace Tic_Tac_Toe.web.service;

/// Интерфейс сервиса авторизации
public interface IAuthService
{
    bool Register(SignUpRequest request);

    Guid? Authenticate(string authorizationHeader);
}
