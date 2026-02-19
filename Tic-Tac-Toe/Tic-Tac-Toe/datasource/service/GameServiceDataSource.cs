using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.domain.service;
using Tic_Tac_Toe.datasource.repository;

namespace Tic_Tac_Toe.datasource.service;

/// Класс, реализующий интерфейс IGameService и принимающий в качестве параметра интерфейс репозитория
public class GameServiceDataSource : IGameService
{
    private readonly IGameRepository _repository;
    private readonly GameService _domainService;

    public GameServiceDataSource(IGameRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _domainService = new GameService();
    }

    public Move GetNextMove(Game game)
    {
        if (game == null)
        {
            throw new ArgumentNullException(nameof(game));
        }

        var move = _domainService.GetNextMove(game);
        
        _repository.Save(game);
        
        return move;
    }

    public bool ValidateBoard(Game game)
    {
        if (game == null)
        {
            throw new ArgumentNullException(nameof(game));
        }

        return _domainService.ValidateBoard(game);
    }

    public GameStatus CheckGameEnd(Game game)
    {
        if (game == null)
        {
            throw new ArgumentNullException(nameof(game));
        }

        return _domainService.CheckGameEnd(game);
    }

    public bool ProcessPlayerMove(Game game, GameBoard newBoard)
    {
        if (game == null || newBoard == null)
        {
            return false;
        }

        return _domainService.ProcessPlayerMove(game, newBoard);
    }

    public Move MakeComputerMove(Game game)
    {
        if (game == null)
        {
            throw new ArgumentNullException(nameof(game));
        }

        var move = _domainService.MakeComputerMove(game);
        
        _repository.Save(game);
        
        return move;
    }

    public bool ValidateBoardBeforeMove(Game currentGame, GameBoard newBoard)
    {
        if (currentGame == null || newBoard == null)
        {
            return false;
        }

        return _domainService.ValidateBoardBeforeMove(currentGame, newBoard);
    }
}

