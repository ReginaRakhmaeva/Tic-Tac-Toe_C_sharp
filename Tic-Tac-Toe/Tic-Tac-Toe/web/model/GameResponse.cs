namespace Tic_Tac_Toe.web.model;

public class GameResponse
{
    public Guid Id { get; set; }

    public GameBoardResponse Board { get; set; }

    public string Status { get; set; }

    public Guid? Player1Id { get; set; }

    public Guid? Player2Id { get; set; }

    public Guid? CurrentPlayerId { get; set; }

    public Guid? WinnerId { get; set; }

    public string Player1Symbol { get; set; }

    public string Player2Symbol { get; set; }

    public GameResponse()
    {
        Id = Guid.Empty;
        Board = new GameBoardResponse();
        Status = "InProgress";
        Player1Id = null;
        Player2Id = null;
        CurrentPlayerId = null;
        WinnerId = null;
        Player1Symbol = "X";
        Player2Symbol = "O";
    }
}
