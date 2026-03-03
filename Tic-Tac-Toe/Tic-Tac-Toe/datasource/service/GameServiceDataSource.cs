using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.domain.service;
using Tic_Tac_Toe.datasource.repository;

namespace Tic_Tac_Toe.datasource.service;

/// Класс, реализующий интерфейс IGameServiceDataSource и принимающий в качестве параметра интерфейс репозитория
public class GameServiceDataSource : IGameServiceDataSource
{
    private readonly IGameRepository _repository;
    private readonly IGameService _domainService;

    public GameServiceDataSource(IGameRepository repository, IGameService domainService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _domainService = domainService ?? throw new ArgumentNullException(nameof(domainService));
    }

    // Реализация доменных методов через делегирование
    public Move GetNextMove(Game game)
    {
        var move = _domainService.GetNextMove(game);
        _repository.Save(game);
        return move;
    }

    public bool ValidateBoard(Game game)
    {
        return _domainService.ValidateBoard(game);
    }

    public GameStatus CheckGameEnd(Game game)
    {
        return _domainService.CheckGameEnd(game);
    }

    public bool ProcessPlayerMove(Game game, GameBoard newBoard)
    {
        return _domainService.ProcessPlayerMove(game, newBoard);
    }

    public Move MakeComputerMove(Game game)
    {
        var move = _domainService.MakeComputerMove(game);
        _repository.Save(game);
        return move;
    }

    public bool ValidateBoardBeforeMove(Game currentGame, GameBoard newBoard)
    {
        return _domainService.ValidateBoardBeforeMove(currentGame, newBoard);
    }

    // Реализация методов персистентности
    public Game? GetGame(Guid id)
    {
        return _repository.Get(id);
    }

    public void SaveGame(Game game)
    {
        if (game == null)
        {
            throw new ArgumentNullException(nameof(game));
        }

        _repository.Save(game);
    }

    public void DeleteGame(Guid id)
    {
        _repository.Delete(id);
    }

    public List<Game> GetAvailableGames()
    {
        return _repository.GetAvailableGames();
    }

    public void DeleteInactiveGamesByPlayer1Id(Guid player1Id)
    {
        _repository.DeleteInactiveGamesByPlayer1Id(player1Id);
    }


    public List<Game> GetCompletedGamesByUserId(Guid userId)
    {
        var games = _repository.GetGamesByUserId(userId);
        var completedGames = new List<Game>();

        foreach (var game in games)
        {
            var status = _domainService.CheckGameEnd(game);

            bool isGameFinished = status == GameStatus.PlayerWins || status == GameStatus.Draw;

            if (isGameFinished)
            {
                completedGames.Add(game);
            }
        }

        return completedGames
            .OrderByDescending(g => g.CreatedAt)
            .ToList();
    }

    public List<PlayerStats> GetTopPlayers(int topN)
    {
        if (topN <= 0)
        {
            return new List<PlayerStats>();
        }

        return _repository.GetTopPlayersByWinRatio(topN);
    }
}

