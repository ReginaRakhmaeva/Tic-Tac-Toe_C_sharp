namespace Tic_Tac_Toe.domain.model;


/// Модель текущей игры с UUID и игровым полем
public class Game
{
    public Guid Id { get; set; }

    public GameBoard Board { get; set; }

    public List<Move> MoveHistory { get; set; }

    public Game()
    {
        Id = Guid.NewGuid();
        Board = new GameBoard();
        MoveHistory = new List<Move>();
    }

    public Game(Guid id, GameBoard board)
    {
        Id = id;
        Board = board ?? new GameBoard();
        MoveHistory = new List<Move>();
    }
}