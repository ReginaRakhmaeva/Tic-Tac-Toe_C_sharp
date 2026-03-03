using Microsoft.EntityFrameworkCore;
using Tic_Tac_Toe.datasource.dbcontext;
using Tic_Tac_Toe.datasource.mapper;
using Tic_Tac_Toe.datasource.model;
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
            var gameDto = GameMapper.ToDto(game);
            
            var existingGame = _context.Games
                .Include(g => g.Moves)
                .FirstOrDefault(g => g.Id == game.Id);
            
            if (existingGame != null)
            {
                if (existingGame.Player1Id == null && gameDto.Player1Id != null)
                {
                    _context.Moves.RemoveRange(existingGame.Moves);
                    _context.Games.Remove(existingGame);
                    _context.SaveChanges(); 
                    
                    _context.Games.Add(gameDto);
                }
                else
                {
                    _context.Moves.RemoveRange(existingGame.Moves);
                    
                    existingGame.UserId = gameDto.UserId;
                    existingGame.GameType = gameDto.GameType;
                    existingGame.Player1Id = gameDto.Player1Id;
                    existingGame.Player2Id = gameDto.Player2Id;
                    existingGame.CurrentPlayerId = gameDto.CurrentPlayerId;
                    existingGame.WinnerId = gameDto.WinnerId;
                    existingGame.Board = gameDto.Board;
                    
                    if (gameDto.Moves != null && gameDto.Moves.Any())
                    {
                        _context.Moves.AddRange(gameDto.Moves);
                    }
                }
            }
            else
            {
                _context.Games.Add(gameDto);
            }

            _context.SaveChanges();
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
            .Where(g => g.GameType == 1 && 
                       g.Player1Id != null && 
                       g.Player2Id == null &&
                       (!g.Moves.Any() || g.Moves.Count == 0)) 
            .ToList();

        return gameDtos.Select(g => GameMapper.ToDomain(g)).ToList();
    }

    /// Удалить игру по UUID
    public void Delete(Guid id)
    {
        try
        {
            var gameDto = _context.Games
                .Include(g => g.Moves)
                .FirstOrDefault(g => g.Id == id);

            if (gameDto != null)
            {
                if (gameDto.Moves != null && gameDto.Moves.Any())
                {
                    _context.Moves.RemoveRange(gameDto.Moves);
                }

                _context.Games.Remove(gameDto);
                _context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting game: {Message}, StackTrace: {StackTrace}", 
                ex.Message, ex.StackTrace);
            throw;
        }
    }

    /// Получить неактивные игры по Player1Id (игры, которые еще не начались)
    public List<Game> GetInactiveGamesByPlayer1Id(Guid player1Id)
    {
        var gameDtos = _context.Games
            .Include(g => g.Moves)
            .Where(g => g.Player1Id == player1Id && 
                       g.Player2Id == null && 
                       (!g.Moves.Any() || g.Moves.Count == 0))
            .ToList();

        return gameDtos.Select(g => GameMapper.ToDomain(g)).ToList();
    }

    /// Удалить все неактивные игры по Player1Id
    public void DeleteInactiveGamesByPlayer1Id(Guid player1Id)
    {
        try
        {
            var gameDtos = _context.Games
                .Include(g => g.Moves)
                .Where(g => g.Player1Id == player1Id && 
                           g.Player2Id == null && 
                           (!g.Moves.Any() || g.Moves.Count == 0))
                .ToList();

            foreach (var gameDto in gameDtos)
            {
                if (gameDto.Moves != null && gameDto.Moves.Any())
                {
                    _context.Moves.RemoveRange(gameDto.Moves);
                }

                _context.Games.Remove(gameDto);
            }

            if (gameDtos.Any())
            {
                _context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting inactive games for player: {Message}, StackTrace: {StackTrace}", 
                ex.Message, ex.StackTrace);
            throw;
        }
    }

    /// Получить все игры, в которых участвует указанный пользователь.
    public List<Game> GetGamesByUserId(Guid userId)
    {
        var gameDtos = _context.Games
            .Include(g => g.Moves)
            .Where(g => g.UserId == userId ||
                        g.Player1Id == userId ||
                        g.Player2Id == userId)
            .ToList();

        return gameDtos.Select(g => GameMapper.ToDomain(g)).ToList();
    }

    public List<PlayerStats> GetTopPlayersByWinRatio(int topN)
    {
        if (topN <= 0)
        {
            return new List<PlayerStats>();
        }

        try
        {
            
            var completedGames = _context.Games
                .Include(g => g.Moves)
                .Where(g => g.Moves.Any() && 
                           (g.WinnerId != null || 
                            (g.WinnerId == null && g.Moves.Count >= 9)))
                .ToList();

            var playerStatsDict = new Dictionary<Guid, PlayerStats>();

            foreach (var game in completedGames)
            {
                var participants = new List<Guid>();
                if (game.Player1Id.HasValue && !GameConstants.IsComputer(game.Player1Id))
                {
                    participants.Add(game.Player1Id.Value);
                }
                if (game.Player2Id.HasValue && !GameConstants.IsComputer(game.Player2Id))
                {
                    participants.Add(game.Player2Id.Value);
                }

                if (participants.Count == 0)
                {
                    continue;
                }

                foreach (var playerId in participants)
                {
                    if (!playerStatsDict.ContainsKey(playerId))
                    {
                        playerStatsDict[playerId] = new PlayerStats { UserId = playerId };
                    }
                }

                if (game.WinnerId == null)
                {
                    foreach (var playerId in participants)
                    {
                        playerStatsDict[playerId].Draws++;
                    }
                }
                else if (!GameConstants.IsComputer(game.WinnerId))
                {
                    var winnerId = game.WinnerId.Value;
                    foreach (var playerId in participants)
                    {
                        if (playerId == winnerId)
                        {
                            playerStatsDict[playerId].Wins++;
                        }
                        else
                        {
                            playerStatsDict[playerId].Losses++;
                        }
                    }
                }
                else
                {
                    foreach (var playerId in participants)
                    {
                        playerStatsDict[playerId].Losses++;
                    }
                }
            }

            var userIds = playerStatsDict.Keys.ToList();
            var users = _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.Login);

            var playerStatsList = new List<PlayerStats>();
            foreach (var kvp in playerStatsDict)
            {
                var stats = kvp.Value;
                if (users.TryGetValue(stats.UserId, out var login))
                {
                    stats.Login = login;
                }
                else
                {
                    stats.Login = "Unknown";
                }

                // Рассчитываем соотношение побед: wins / (losses + draws)
                var nonWins = stats.Losses + stats.Draws;
                if (nonWins > 0)
                {
                    stats.WinRatio = (double)stats.Wins / nonWins;
                }
                else if (stats.Wins > 0)
                {
                    stats.WinRatio = stats.Wins;
                }
                else
                {
                    stats.WinRatio = 0.0;
                }

                playerStatsList.Add(stats);
            }

            return playerStatsList
                .OrderByDescending(s => s.WinRatio)
                .ThenByDescending(s => s.Wins)
                .Take(topN)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting top players: {Message}, StackTrace: {StackTrace}", 
                ex.Message, ex.StackTrace);
            throw;
        }
    }
}

