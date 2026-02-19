using System.Text;
using Tic_Tac_Toe.domain.service;
using Tic_Tac_Toe.web.model;

namespace Tic_Tac_Toe.web.service;

/// Реализация сервиса авторизации
public class AuthService : IAuthService
{
    private readonly IUserService _userService;

    public AuthService(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public bool Register(SignUpRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Login))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return false;
        }

        var user = _userService.CreateUser(request.Login, request.Password);
        return user != null;
    }

    public Guid? Authenticate(string authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return null;
        }

        string base64Credentials = authorizationHeader.Trim();
        if (base64Credentials.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            base64Credentials = base64Credentials.Substring(6);
        }

        try
        {
            byte[] credentialBytes = Convert.FromBase64String(base64Credentials);
            string credentials = Encoding.UTF8.GetString(credentialBytes);

            int separatorIndex = credentials.IndexOf(':');
            if (separatorIndex < 0)
            {
                return null;
            }

            string login = credentials.Substring(0, separatorIndex);
            string password = credentials.Substring(separatorIndex + 1);

            var user = _userService.GetUserByLogin(login);
            if (user == null)
            {
                return null;
            }

            if (!_userService.VerifyPassword(user, password))
            {
                return null;
            }

            return user.Id;
        }
        catch (FormatException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
