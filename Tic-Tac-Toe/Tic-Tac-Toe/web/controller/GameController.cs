using Microsoft.AspNetCore.Mvc;
using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.datasource.service;
using Tic_Tac_Toe.web.model;
using Tic_Tac_Toe.web.mapper;

namespace Tic_Tac_Toe.web.controller;

/// Контроллер для работы с играми
[ApiController]
[Route("game")]
public class GameController : ControllerBase
{
    private readonly IGameServiceDataSource _gameService;

    public GameController(IGameServiceDataSource gameService)
    {
        _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
    }

    [HttpGet("{id}")]
    public IActionResult GetGame(Guid id, [FromQuery] string firstMove = "player")
    {
        try
        {
            var currentGame = _gameService.GetGame(id);
            
            if (currentGame == null)
            {
                bool computerFirst = (firstMove?.ToLower() == "computer");
                
                currentGame = new Game { Id = id };
                
                if (computerFirst)
                {
                    _gameService.MakeComputerMove(currentGame);
                }
                else
                {
                    _gameService.SaveGame(currentGame);
                }
            }
            
            var gameStatus = _gameService.CheckGameEnd(currentGame);
            var response = GameMapper.ToResponse(currentGame, gameStatus);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse("Internal server error", ex.Message));
        }
    }

    [HttpPost("{id}")]
    public IActionResult MakeMove(Guid id, [FromBody] GameRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new ErrorResponse("Request body is required"));
            }

            if (request.Id != id)
            {
                return BadRequest(new ErrorResponse("Game ID in URL does not match ID in request body"));
            }

            if (request.Board == null)
            {
                return BadRequest(new ErrorResponse("Board is required"));
            }

            Game gameFromRequest;
            try
            {
                gameFromRequest = GameMapper.ToDomain(request);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse("Invalid game data", ex.Message));
            }

            Game currentGame = _gameService.GetGame(id) ?? new Game { Id = id };

            if (!_gameService.ProcessPlayerMove(currentGame, gameFromRequest.Board))
            {
                return BadRequest(new ErrorResponse("Invalid player move: no valid move detected"));
            }

            if (!_gameService.ValidateBoard(currentGame))
            {
                return BadRequest(new ErrorResponse("Invalid game board: previous moves have been changed"));
            }

            // Сохраняем ход игрока
            _gameService.SaveGame(currentGame);

            var gameStatus = _gameService.CheckGameEnd(currentGame);
            if (gameStatus != GameStatus.InProgress)
            {
                var gameResponse = GameMapper.ToResponse(currentGame, gameStatus);
                return Ok(gameResponse);
            }

            _gameService.MakeComputerMove(currentGame);

            var finalStatus = _gameService.CheckGameEnd(currentGame);
            var finalResponse = GameMapper.ToResponse(currentGame, finalStatus);

            return Ok(finalResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse("Internal server error", ex.Message));
        }
    }
}
