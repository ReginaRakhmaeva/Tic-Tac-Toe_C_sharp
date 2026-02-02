namespace Tic_Tac_Toe.domain.model;


/// Модель текущей игры с UUID и игровым полем
public class Game
{
    /// Уникальный идентификатор игры (UUID)
    public Guid Id { get; set; }

    /// Идентификатор пользователя, владельца игры (для обратной совместимости с игрой против компьютера)
    public Guid UserId { get; set; }

    /// Идентификатор первого игрока (играет за X)
    public Guid? Player1Id { get; set; }

    /// Идентификатор второго игрока (играет за O)
    public Guid? Player2Id { get; set; }

    /// Идентификатор текущего игрока, чей сейчас ход
    public Guid? CurrentPlayerId { get; set; }

    /// Идентификатор победителя (если игра завершена)
    public Guid? WinnerId { get; set; }

    /// Игровое поле
    public GameBoard Board { get; set; }

    /// История ходов для валидации (хранит последовательность ходов)
    public List<Move> MoveHistory { get; set; }

    public Game()
    {
        Id = Guid.NewGuid();
        UserId = Guid.Empty;
        Player1Id = null;
        Player2Id = null;
        CurrentPlayerId = null;
        WinnerId = null;
        Board = new GameBoard();
        MoveHistory = new List<Move>();
    }

    public Game(Guid id, GameBoard board)
    {
        Id = id;
        UserId = Guid.Empty;
        Player1Id = null;
        Player2Id = null;
        CurrentPlayerId = null;
        WinnerId = null;
        Board = board ?? new GameBoard();
        MoveHistory = new List<Move>();
    }

    public Game(Guid id, Guid userId, GameBoard board)
    {
        Id = id;
        UserId = userId;
        Player1Id = null;
        Player2Id = null;
        CurrentPlayerId = null;
        WinnerId = null;
        Board = board ?? new GameBoard();
        MoveHistory = new List<Move>();
    }
}