using Microsoft.AspNetCore.Mvc;
using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.datasource.service;
using Tic_Tac_Toe.web.model;
using Tic_Tac_Toe.web.mapper;
using System.Linq;

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

    /// Получение доступных игр (ожидающих второго игрока)
    [HttpGet("available")]
    public IActionResult GetAvailableGames()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new ErrorResponse("User ID not found in authorization context"));
        }

        var availableGames = _gameService.GetAvailableGames();
        
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

        var game = _gameService.GetGame(id);
        
        if (game == null)
        {
            return NotFound(new ErrorResponse("Game not found"));
        }

        if (game.Player1Id == null)
        {
            return BadRequest(new ErrorResponse("Game is not available for joining"));
        }

        if (game.Player2Id != null)
        {
            return BadRequest(new ErrorResponse("Game already has two players"));
        }

        if (game.Player1Id == userId)
        {
            return BadRequest(new ErrorResponse("Cannot join your own game"));
        }

        game.Player2Id = userId;
        game.CurrentPlayerId = game.Player1Id;

        _gameService.SaveGame(game);

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

        var currentGame = _gameService.GetGame(id);
        
        if (currentGame == null)
        {
            return NotFound(new ErrorResponse("Game not found"));
        }

        bool hasAccess = currentGame.UserId == userId ||
                        currentGame.Player1Id == userId ||
                        currentGame.Player2Id == userId;
        
        if (!hasAccess)
        {
            return Forbid();
        }
        
        GameStatus gameStatus;
        
        bool hasPlayerLeft = currentGame.GameType == GameType.TwoPlayer && 
                            (currentGame.Player1Id == null || currentGame.Player2Id == null) &&
                            (currentGame.MoveHistory != null && currentGame.MoveHistory.Count > 0);
        
        if (hasPlayerLeft)
        {
            gameStatus = GameStatus.PlayerLeft;
        }
        else if (currentGame.GameType == GameType.TwoPlayer && currentGame.Player2Id == null)
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
        
        if (gameType == "player")
        {
            _gameService.DeleteInactiveGamesByPlayer1Id(userId);
        }
        
        var gameId = Guid.NewGuid();
        Game newGame;

        if (gameType == "player")
        {
            if (request.Player2Id.HasValue)
            {
                if (request.Player2Id.Value == userId)
                {
                    return BadRequest(new ErrorResponse("Cannot create game with yourself"));
                }

                newGame = new Game
                {
                    Id = gameId,
                    UserId = userId,
                    GameType = GameType.TwoPlayer,
                    Player1Id = userId,
                    Player2Id = request.Player2Id.Value,
                    CurrentPlayerId = userId,
                    Board = new GameBoard(),
                    MoveHistory = new List<Move>()
                };

                var gameStatus = GameStatus.PlayerTurn;
                _gameService.SaveGame(newGame);
                var response = GameMapper.ToResponse(newGame, gameStatus);
                return CreatedAtAction(nameof(GetGame), new { id = gameId }, response);
            }
            else
            {
                newGame = new Game
                {
                    Id = gameId,
                    UserId = userId,
                    GameType = GameType.TwoPlayer,
                    Player1Id = userId,
                    Player2Id = null,
                    CurrentPlayerId = null,
                    Board = new GameBoard(),
                    MoveHistory = new List<Move>()
                };

                var gameStatus = GameStatus.WaitingForPlayers;
                _gameService.SaveGame(newGame);
                var response = GameMapper.ToResponse(newGame, gameStatus);
                return CreatedAtAction(nameof(GetGame), new { id = gameId }, response);
            }
        }
        else
        {
            bool computerFirst = (request.FirstMove?.ToLower() == "computer");
            newGame = new Game
            {
                Id = gameId,
                UserId = userId,
                GameType = GameType.Computer,
                Player1Id = userId,
                Player2Id = GameConstants.ComputerId,
                Board = new GameBoard(),
                MoveHistory = new List<Move>()
            };

            if (computerFirst)
            {
                _gameService.MakeComputerMove(newGame);
            }

            _gameService.SaveGame(newGame);
            var gameStatus = _gameService.CheckGameEnd(newGame);
            var response = GameMapper.ToResponse(newGame, gameStatus);
            return CreatedAtAction(nameof(GetGame), new { id = gameId }, response);
        }
    }

    /// Выход из игры (удаление игры, если она еще не началась)
    [HttpPost("{id}/leave")]
    public IActionResult LeaveGame(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new ErrorResponse("User ID not found in authorization context"));
        }

        var game = _gameService.GetGame(id);
        
        if (game == null)
        {
            return NotFound(new ErrorResponse("Game not found"));
        }

        bool isParticipant = game.UserId == userId || 
                           game.Player1Id == userId || 
                           game.Player2Id == userId;
        
        if (!isParticipant)
        {
            return Forbid();
        }

        bool isCreator = game.UserId == userId || game.Player1Id == userId;
        bool isSecondPlayer = game.Player2Id == userId;
        
        bool canDelete = game.Player2Id == null && 
                        (game.MoveHistory == null || game.MoveHistory.Count == 0);

        if (canDelete)
        {
            _gameService.DeleteGame(id);
            return Ok(new { message = "Game deleted successfully" });
        }
        else
        {
            if (isSecondPlayer)
            {
                _gameService.DeleteGame(id);
                return Ok(new { message = "Game deleted successfully" });
            }
            else if (isCreator)
            {
                game.Player1Id = null;
                
                game.CurrentPlayerId = null;
                
                _gameService.SaveGame(game);
                return Ok(new { message = "Left the game" });
            }
        }
        
        return BadRequest(new ErrorResponse("Unable to leave the game"));
    }

    /// Удаление игры (только для создателя, если игра еще не началась)
    [HttpDelete("{id}")]
    public IActionResult DeleteGame(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new ErrorResponse("User ID not found in authorization context"));
        }

        var game = _gameService.GetGame(id);
        
        if (game == null)
        {
            return NotFound(new ErrorResponse("Game not found"));
        }

        if (game.UserId != userId && game.Player1Id != userId)
        {
            return Forbid();
        }

        bool canDelete = game.Player2Id == null && 
                        (game.MoveHistory == null || game.MoveHistory.Count == 0);

        if (!canDelete)
        {
            return BadRequest(new ErrorResponse("Cannot delete game that has already started"));
        }

        _gameService.DeleteGame(id);
        return NoContent();
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

        var currentGame = _gameService.GetGame(id);
        
        if (currentGame == null)
        {
            return NotFound(new ErrorResponse("Game not found"));
        }

        bool hasAccess = currentGame.UserId == userId ||
                        currentGame.Player1Id == userId ||
                        currentGame.Player2Id == userId;
        
        if (!hasAccess)
        {
            return Forbid();
        }

        bool isTwoPlayerGame = currentGame.GameType == GameType.TwoPlayer;
        if (isTwoPlayerGame)
        {
            if (currentGame.Player1Id == null || currentGame.Player2Id == null)
            {
                return BadRequest(new ErrorResponse("Your opponent has left the game"));
            }
            
            if (currentGame.CurrentPlayerId != userId)
            {
                return BadRequest(new ErrorResponse("It's not your turn"));
            }
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
        
        bool isGameFinished = gameStatus == GameStatus.PlayerWins || 
                             gameStatus == GameStatus.Draw;
        
        if (isGameFinished)
        {
            _gameService.SaveGame(currentGame);
            return Ok(GameMapper.ToResponse(currentGame, gameStatus));
        }

        if (isTwoPlayerGame)
        {
            if (currentGame.CurrentPlayerId == currentGame.Player1Id)
            {
                currentGame.CurrentPlayerId = currentGame.Player2Id;
            }
            else
            {
                currentGame.CurrentPlayerId = currentGame.Player1Id;
            }

            _gameService.SaveGame(currentGame);
            return Ok(GameMapper.ToResponse(currentGame, gameStatus));
        }
        else
        {
            _gameService.MakeComputerMove(currentGame);
            _gameService.SaveGame(currentGame);

            var finalStatus = _gameService.CheckGameEnd(currentGame);
            return Ok(GameMapper.ToResponse(currentGame, finalStatus));
        }
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
