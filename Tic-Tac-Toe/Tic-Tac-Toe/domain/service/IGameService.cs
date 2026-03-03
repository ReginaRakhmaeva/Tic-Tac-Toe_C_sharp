using Tic_Tac_Toe.domain.model;

namespace Tic_Tac_Toe.domain.service;

/// Интерфейс сервиса для работы с игрой
public interface IGameService
{
    Move GetNextMove(Game game);
    
    bool ValidateBoard(Game game);

    GameStatus CheckGameEnd(Game game);

    bool ProcessPlayerMove(Game game, GameBoard newBoard);

    Move MakeComputerMove(Game game);
}