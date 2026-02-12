namespace Tic_Tac_Toe.web.model;

public class GameResponse
{
    public Guid Id { get; set; }

    public GameBoardResponse Board { get; set; }

    /// Статус игры: "InProgress", "PlayerXWins", "PlayerOWins", "Draw", "WaitingForPlayers", "PlayerTurn", "PlayerWins"
    public string Status { get; set; }

    /// Идентификатор первого игрока (играет за X)
    public Guid? Player1Id { get; set; }

    /// Идентификатор второго игрока (играет за O)
    public Guid? Player2Id { get; set; }

    /// Идентификатор текущего игрока, чей сейчас ход (используется при Status = "PlayerTurn")
    public Guid? CurrentPlayerId { get; set; }

    /// Идентификатор победителя (используется при Status = "PlayerWins")
    public Guid? WinnerId { get; set; }

    public GameResponse()
    {
        Id = Guid.Empty;
        Board = new GameBoardResponse();
        Status = "InProgress";
        Player1Id = null;
        Player2Id = null;
        CurrentPlayerId = null;
        WinnerId = null;
    }
}
