namespace Tic_Tac_Toe.web.model;

/// Модель ответа для записи в таблице лидеров
public class LeaderboardEntryResponse
{
    public Guid UserId { get; set; }

    public string Login { get; set; }

    public double WinRatio { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public int Draws { get; set; }

    public LeaderboardEntryResponse()
    {
        UserId = Guid.Empty;
        Login = string.Empty;
        WinRatio = 0.0;
        Wins = 0;
        Losses = 0;
        Draws = 0;
    }
}
