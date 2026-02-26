namespace Tic_Tac_Toe.domain.model;

public class Game
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public GameType GameType { get; set; }

    public Guid? Player1Id { get; set; }

    public Guid? Player2Id { get; set; }

    public Guid? CurrentPlayerId { get; set; }

    public Guid? WinnerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public GameBoard Board { get; set; }

    public List<Move> MoveHistory { get; set; }

    public Game()
    {
        Id = Guid.NewGuid();
        UserId = Guid.Empty;
        GameType = GameType.Computer;
        Player1Id = null;
        Player2Id = null;
        CurrentPlayerId = null;
        WinnerId = null;
        CreatedAt = DateTime.UtcNow;
        Board = new GameBoard();
        MoveHistory = new List<Move>();
    }

    public Game(Guid id, GameBoard board)
    {
        Id = id;
        UserId = Guid.Empty;
        GameType = GameType.Computer;
        Player1Id = null;
        Player2Id = null;
        CurrentPlayerId = null;
        WinnerId = null;
        CreatedAt = DateTime.UtcNow;
        Board = board ?? new GameBoard();
        MoveHistory = new List<Move>();
    }

    public Game(Guid id, Guid userId, GameBoard board)
    {
        Id = id;
        UserId = userId;
        GameType = GameType.Computer;
        Player1Id = null;
        Player2Id = null;
        CurrentPlayerId = null;
        WinnerId = null;
        CreatedAt = DateTime.UtcNow;
        Board = board ?? new GameBoard();
        MoveHistory = new List<Move>();
    }
}