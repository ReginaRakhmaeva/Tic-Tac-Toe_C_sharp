using Microsoft.AspNetCore.Mvc;
using Tic_Tac_Toe.domain.service;
using Tic_Tac_Toe.web.mapper;
using Tic_Tac_Toe.web.model;

namespace Tic_Tac_Toe.web.controller;

/// Контроллер для работы с пользователями
[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    /// Получение информации о пользователе по UUID
    [HttpGet("{id}")]
    public IActionResult GetUser(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("Invalid user ID"));
        }

        var user = _userService.GetUserById(id);

        if (user == null)
        {
            return NotFound(new ErrorResponse("User not found"));
        }

        var response = UserMapper.ToResponse(user);
        return Ok(response);
    }
}
