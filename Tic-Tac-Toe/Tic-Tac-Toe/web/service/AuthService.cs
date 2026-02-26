using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Tic_Tac_Toe.domain.service;
using Tic_Tac_Toe.web.model;

namespace Tic_Tac_Toe.web.service;

/// Реализация сервиса авторизации
public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly JwtProvider _jwtProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IUserService userService, JwtProvider jwtProvider, IHttpContextAccessor httpContextAccessor)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
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

        try
        {
            var user = _userService.CreateUser(request.Login, request.Password);
            return user != null;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public JwtResponse? Authenticate(JwtRequest request)
    {
        if (request == null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = _userService.GetUserByLogin(request.Login);
        if (user == null)
        {
            return null;
        }

        if (!_userService.VerifyPassword(user, request.Password))
        {
            return null;
        }

        var accessToken = _jwtProvider.GenerateAccessToken(user);
        var refreshToken = _jwtProvider.GenerateRefreshToken(user);

        return new JwtResponse
        {
            Type = "Bearer",
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public JwtResponse? RefreshAccessToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        if (!_jwtProvider.ValidateRefreshToken(refreshToken))
        {
            return null;
        }

        var claims = _jwtProvider.GetClaims(refreshToken);
        if (claims == null)
        {
            return null;
        }

        var uuidClaim = claims.FindFirst("uuid");
        if (uuidClaim == null || !Guid.TryParse(uuidClaim.Value, out Guid userId))
        {
            return null;
        }

        var user = _userService.GetUserById(userId);
        if (user == null)
        {
            return null;
        }

        var newAccessToken = _jwtProvider.GenerateAccessToken(user);

        return new JwtResponse
        {
            Type = "Bearer",
            AccessToken = newAccessToken,
            RefreshToken = refreshToken
        };
    }

    public JwtResponse? RefreshRefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        if (!_jwtProvider.ValidateRefreshToken(refreshToken))
        {
            return null;
        }

        var claims = _jwtProvider.GetClaims(refreshToken);
        if (claims == null)
        {
            return null;
        }

        var uuidClaim = claims.FindFirst("uuid");
        if (uuidClaim == null || !Guid.TryParse(uuidClaim.Value, out Guid userId))
        {
            return null;
        }

        var user = _userService.GetUserById(userId);
        if (user == null)
        {
            return null;
        }

        var newAccessToken = _jwtProvider.GenerateAccessToken(user);
        var newRefreshToken = _jwtProvider.GenerateRefreshToken(user);

        return new JwtResponse
        {
            Type = "Bearer",
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}
