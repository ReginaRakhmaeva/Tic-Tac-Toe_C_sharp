namespace Tic_Tac_Toe.web.model;

public class GameResponse
{
    public Guid Id { get; set; }

    public GameBoardResponse Board { get; set; }

    public string Status { get; set; }

    public GameResponse()
    {
        Id = Guid.Empty;
        Board = new GameBoardResponse();
        Status = "InProgress";
    }
}
