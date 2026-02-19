using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tic_Tac_Toe.web.model;
using Tic_Tac_Toe.web.service;

namespace Tic_Tac_Toe.web.controller;

/// Контроллер для авторизации и регистрации пользователей
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _environment;

    public AuthController(IAuthService authService, IWebHostEnvironment environment)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
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
            var details = _environment.IsDevelopment() ? ex.ToString() : ex.Message;
            return StatusCode(500, new ErrorResponse("Internal server error", details));
        }
    }

    /// Авторизация пользователя
    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login()
    {
        try
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Unauthorized(new ErrorResponse("Authorization header is required"));
            }

            string authorizationHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                return Unauthorized(new ErrorResponse("Authorization header is empty"));
            }

            Guid? userId = _authService.Authenticate(authorizationHeader);

            if (userId == null)
            {
                return Unauthorized(new ErrorResponse("Invalid login or password"));
            }

            return Ok(new AuthResponse(userId.Value));
        }
        catch (Exception ex)
        {
            var details = _environment.IsDevelopment() ? ex.ToString() : ex.Message;
            return StatusCode(500, new ErrorResponse("Internal server error", details));
        }
    }
}
