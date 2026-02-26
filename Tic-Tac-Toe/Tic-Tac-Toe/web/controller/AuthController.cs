using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tic_Tac_Toe.domain.service;
using Tic_Tac_Toe.web.mapper;
using Tic_Tac_Toe.web.model;
using Tic_Tac_Toe.web.service;

namespace Tic_Tac_Toe.web.controller;

/// Контроллер для авторизации и регистрации пользователей
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _environment;

    public AuthController(IAuthService authService, IUserService userService, IWebHostEnvironment environment)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// Регистрация нового пользователя
    [AllowAnonymous]
    [HttpPost("register")]
    public IActionResult Register([FromBody] SignUpRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new ErrorResponse("Request body is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Login))
            {
                return BadRequest(new ErrorResponse("Login is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ErrorResponse("Password is required"));
            }

            bool success = _authService.Register(request);

            if (success)
            {
                return Ok(new RegisterResponse(true, "User registered successfully"));
            }
            else
            {
                return Conflict(new ErrorResponse("User with this login already exists"));
            }
        }
        catch (Exception ex)
        {
            var logger = HttpContext.RequestServices.GetService<ILogger<AuthController>>();
            logger?.LogError(ex, "Ошибка при регистрации пользователя: {Message}", ex.Message);
            
            var details = _environment.IsDevelopment() ? ex.ToString() : ex.Message;
            return StatusCode(500, new ErrorResponse("Internal server error", details));
        }
    }

    /// Авторизация пользователя
    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] JwtRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new ErrorResponse("Request body is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Login))
            {
                return BadRequest(new ErrorResponse("Login is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ErrorResponse("Password is required"));
            }

            var response = _authService.Authenticate(request);

            if (response == null)
            {
                return Unauthorized(new ErrorResponse("Invalid login or password"));
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            var details = _environment.IsDevelopment() ? ex.ToString() : ex.Message;
            return StatusCode(500, new ErrorResponse("Internal server error", details));
        }
    }

    /// Обновление accessToken
    [AllowAnonymous]
    [HttpPost("refresh-access")]
    public IActionResult RefreshAccessToken([FromBody] RefreshJwtRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new ErrorResponse("Request body is required"));
            }

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new ErrorResponse("RefreshToken is required"));
            }

            var response = _authService.RefreshAccessToken(request.RefreshToken);

            if (response == null)
            {
                return Unauthorized(new ErrorResponse("Invalid refresh token"));
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            var details = _environment.IsDevelopment() ? ex.ToString() : ex.Message;
            return StatusCode(500, new ErrorResponse("Internal server error", details));
        }
    }

    /// Обновление refreshToken
    [AllowAnonymous]
    [HttpPost("refresh-refresh")]
    public IActionResult RefreshRefreshToken([FromBody] RefreshJwtRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new ErrorResponse("Request body is required"));
            }

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new ErrorResponse("RefreshToken is required"));
            }

            var response = _authService.RefreshRefreshToken(request.RefreshToken);

            if (response == null)
            {
                return Unauthorized(new ErrorResponse("Invalid refresh token"));
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            var details = _environment.IsDevelopment() ? ex.ToString() : ex.Message;
            return StatusCode(500, new ErrorResponse("Internal server error", details));
        }
    }

    /// Получение информации о пользователе по accessToken
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        try
        {
            var uuidClaim = User.FindFirst("uuid");
            if (uuidClaim == null || !Guid.TryParse(uuidClaim.Value, out Guid userId))
            {
                return Unauthorized(new ErrorResponse("Invalid token"));
            }

            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(new ErrorResponse("User not found"));
            }

            var response = UserMapper.ToResponse(user);
            return Ok(response);
        }
        catch (Exception ex)
        {
            var details = _environment.IsDevelopment() ? ex.ToString() : ex.Message;
            return StatusCode(500, new ErrorResponse("Internal server error", details));
        }
    }
}
