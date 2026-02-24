using Tic_Tac_Toe.domain.model;

namespace Tic_Tac_Toe.domain.service;

/// Интерфейс сервиса для доменной логики (без персистентности)
public interface IGameService
{
    /// Метод получения следующего хода текущей игры алгоритмом Минимакс
    Move GetNextMove(Game game);

    /// Метод валидации игрового поля текущей игры (проверка, что не изменены предыдущие ходы)
    bool ValidateBoard(Game game);

    /// Метод проверки окончания игры
    GameStatus CheckGameEnd(Game game);

    /// Дополнительные доменные методы
    bool ProcessPlayerMove(Game game, GameBoard newBoard);

    Move MakeComputerMove(Game game);

    bool ValidateBoardBeforeMove(Game currentGame, GameBoard newBoard);
}