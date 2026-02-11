using Microsoft.EntityFrameworkCore;
using Tic_Tac_Toe.datasource.dbcontext;
using Tic_Tac_Toe.datasource.mapper;
using Tic_Tac_Toe.domain.model;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Tic_Tac_Toe.datasource.repository;

/// Реализация репозитория для работы с базой данных
public class GameRepository : IGameRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GameRepository> _logger;

    public GameRepository(ApplicationDbContext context, ILogger<GameRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// Сохранить текущую игру
    public void Save(Game game)
    {
        if (game == null)
        {
            throw new ArgumentNullException(nameof(game));
        }

        try
        {
            _logger.LogInformation("Saving game with Id: {GameId}, UserId: {UserId}", game.Id, game.UserId);

            var gameDto = GameMapper.ToDto(game);
            _logger.LogInformation("GameDto created: Id={Id}, UserId={UserId}", gameDto.Id, gameDto.UserId);
            
            var existingGame = _context.Games
                .Include(g => g.Moves)
                .FirstOrDefault(g => g.Id == game.Id);
            
            if (existingGame != null)
            {
                _logger.LogInformation("Updating existing game");
                existingGame.UserId = gameDto.UserId;
                existingGame.Player1Id = gameDto.Player1Id;
                existingGame.Player2Id = gameDto.Player2Id;
                existingGame.CurrentPlayerId = gameDto.CurrentPlayerId;
                existingGame.WinnerId = gameDto.WinnerId;
                existingGame.Board = gameDto.Board;
                
                // Удаляем старые ходы
                _context.Moves.RemoveRange(existingGame.Moves);
                
                // Добавляем новые ходы из gameDto
                if (gameDto.Moves != null && gameDto.Moves.Any())
                {
                    _context.Moves.AddRange(gameDto.Moves);
                }
                
                _context.Games.Update(existingGame);
            }
            else
            {
                _logger.LogInformation("Adding new game to context");
                _context.Games.Add(gameDto);
                
                // Ходы уже добавлены в gameDto через маппер
            }

            _logger.LogInformation("Calling SaveChanges()");
            _context.SaveChanges();
            _logger.LogInformation("Game saved successfully");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while saving game: {Message}, InnerException: {InnerException}", 
                ex.Message, ex.InnerException?.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while saving game: {Message}, StackTrace: {StackTrace}", 
                ex.Message, ex.StackTrace);
            throw;
        }
    }

    /// Получить текущую игру по UUID
    public Game? Get(Guid id)
    {
        var gameDto = _context.Games
            .Include(g => g.Moves)
            .FirstOrDefault(g => g.Id == id);
        
        if (gameDto == null)
        {
            return null;
        }

        return GameMapper.ToDomain(gameDto);
    }

    /// Получить доступные игры (ожидающие второго игрока)
    public List<Game> GetAvailableGames()
    {
        var gameDtos = _context.Games
            .Include(g => g.Moves)
            .Where(g => g.Player1Id != null && g.Player2Id == null)
            .ToList();

        return gameDtos.Select(g => GameMapper.ToDomain(g)).ToList();
    }
}

