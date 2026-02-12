using Microsoft.AspNetCore.Mvc;
using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.domain.service;
using Tic_Tac_Toe.datasource.repository;
using Tic_Tac_Toe.web.model;
using Tic_Tac_Toe.web.mapper;
using System.Linq;

namespace Tic_Tac_Toe.web.controller;

/// Контроллер для работы с играми
[ApiController]
[Route("game")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly IGameRepository _repository;

    public GameController(IGameService gameService, IGameRepository repository)
    {
        _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// Получение доступных игр (ожидающих второго игрока)
    [HttpGet("available")]
    public IActionResult GetAvailableGames()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new ErrorResponse("User ID not found in authorization context"));
        }

        var availableGames = _repository.GetAvailableGames();
        
        // Исключаем игры, созданные текущим пользователем
        var gamesForUser = availableGames
            .Where(g => g.Player1Id != userId)
            .ToList();

        var responses = gamesForUser.Select(game =>
        {
            var gameStatus = GameStatus.WaitingForPlayers;
            return GameMapper.ToResponse(game, gameStatus);
        }).ToList();

        return Ok(responses);
    }

    /// Присоединение пользователя к игре
    [HttpPost("{id}/join")]
    public IActionResult JoinGame(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new ErrorResponse("User ID not found in authorization context"));
        }

        var game = _repository.Get(id);
        
        if (game == null)
        {
            return NotFound(new ErrorResponse("Game not found"));
        }

        // Проверка, что игра доступна для присоединения
        if (game.Player1Id == null)
        {
            return BadRequest(new ErrorResponse("Game is not available for joining"));
        }

        if (game.Player2Id != null)
        {
            return BadRequest(new ErrorResponse("Game already has two players"));
        }

        // Проверка, что пользователь не является создателем игры
        if (game.Player1Id == userId)
        {
            return BadRequest(new ErrorResponse("Cannot join your own game"));
        }

        // Присоединение к игре
        game.Player2Id = userId;
        game.CurrentPlayerId = game.Player1Id; // Первый игрок начинает

        _repository.Save(game);

        var gameStatus = GameStatus.PlayerTurn;
        var response = GameMapper.ToResponse(game, gameStatus);
        return Ok(response);
    }

    /// Получение игры по ID
    [HttpGet("{id}")]
    public IActionResult GetGame(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new ErrorResponse("User ID not found in authorization context"));
        }

        var currentGame = _repository.Get(id);
        
        if (currentGame == null)
        {
            return NotFound(new ErrorResponse("Game not found"));
        }

        // Проверка доступа: игрок должен быть владельцем (UserId) или одним из игроков (Player1Id/Player2Id)
        bool hasAccess = currentGame.UserId == userId ||
                        currentGame.Player1Id == userId ||
                        currentGame.Player2Id == userId;
        
        if (!hasAccess)
        {
            return Forbid();
        }
        
        // Если игра ожидает второго игрока, возвращаем статус WaitingForPlayers
        GameStatus gameStatus;
        if (currentGame.Player1Id != null && currentGame.Player2Id == null)
        {
            gameStatus = GameStatus.WaitingForPlayers;
        }
        else
        {
            gameStatus = _gameService.CheckGameEnd(currentGame);
        }
        
        var response = GameMapper.ToResponse(currentGame, gameStatus);
        return Ok(response);
    }

    /// Создание новой игры
    [HttpPost]
    public IActionResult CreateGame([FromBody] CreateGameRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new ErrorResponse("User ID not found in authorization context"));
        }

        if (request == null)
        {
            return BadRequest(new ErrorResponse("Request body is required"));
        }

        var gameType = request.GameType?.ToLower() ?? "computer";
        var gameId = Guid.NewGuid();
        Game newGame;

        if (gameType == "player")
        {
            // Игра с другим игроком
            if (request.Player2Id.HasValue)
            {
                // Если Player2Id указан, создаем игру с двумя игроками сразу
                if (request.Player2Id.Value == userId)
                {
                    return BadRequest(new ErrorResponse("Cannot create game with yourself"));
                }

                newGame = new Game(gameId, userId, new GameBoard())
                {
                    Player1Id = userId,
                    Player2Id = request.Player2Id.Value,
                    CurrentPlayerId = userId,
                };

                var gameStatus = GameStatus.PlayerTurn;
                _repository.Save(newGame);
                var response = GameMapper.ToResponse(newGame, gameStatus);
                return CreatedAtAction(nameof(GetGame), new { id = gameId }, response);
            }
            else
            {
                // Если Player2Id не указан, создаем игру, ожидающую второго игрока
                newGame = new Game(gameId, userId, new GameBoard())
                {
                    Player1Id = userId,
                    Player2Id = null,
                    CurrentPlayerId = null, // Ход еще не начался
                };

                var gameStatus = GameStatus.WaitingForPlayers;
                _repository.Save(newGame);
                var response = GameMapper.ToResponse(newGame, gameStatus);
                return CreatedAtAction(nameof(GetGame), new { id = gameId }, response);
            }
        }
        else
        {
            // Игра с компьютером
            bool computerFirst = (request.FirstMove?.ToLower() == "computer");
            newGame = new Game(gameId, userId, new GameBoard());

            if (computerFirst)
            {
                _gameService.MakeComputerMove(newGame);
            }

            _repository.Save(newGame);
            var gameStatus = _gameService.CheckGameEnd(newGame);
            var response = GameMapper.ToResponse(newGame, gameStatus);
            return CreatedAtAction(nameof(GetGame), new { id = gameId }, response);
        }
    }

    [HttpPost("{id}")]
    public IActionResult MakeMove(Guid id, [FromBody] GameRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new ErrorResponse("User ID not found in authorization context"));
        }

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

        var currentGame = _repository.Get(id);
        
        if (currentGame == null)
        {
            return NotFound(new ErrorResponse("Game not found"));
        }

        // Проверка доступа: игрок должен быть владельцем (UserId) или одним из игроков (Player1Id/Player2Id)
        bool hasAccess = currentGame.UserId == userId ||
                        currentGame.Player1Id == userId ||
                        currentGame.Player2Id == userId;
        
        if (!hasAccess)
        {
            return Forbid();
        }

        if (!_gameService.ValidateBoardBeforeMove(currentGame, gameFromRequest.Board))
        {
            return BadRequest(new ErrorResponse("Invalid game board: previous moves have been changed"));
        }

        if (!_gameService.ProcessPlayerMove(currentGame, gameFromRequest.Board))
        {
            return BadRequest(new ErrorResponse("Invalid player move: no valid move detected"));
        }

        var gameStatus = _gameService.CheckGameEnd(currentGame);
        if (gameStatus != GameStatus.InProgress)
        {
            _repository.Save(currentGame);
            return Ok(GameMapper.ToResponse(currentGame, gameStatus));
        }

        _gameService.MakeComputerMove(currentGame);
        _repository.Save(currentGame);

        var finalStatus = _gameService.CheckGameEnd(currentGame);
        return Ok(GameMapper.ToResponse(currentGame, finalStatus));
    }

    private bool TryGetUserId(out Guid userId)
    {
        if (HttpContext.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is Guid id)
        {
            userId = id;
            return true;
        }
        userId = Guid.Empty;
        return false;
    }
}
