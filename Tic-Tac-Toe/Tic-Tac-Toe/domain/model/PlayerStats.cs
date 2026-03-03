namespace Tic_Tac_Toe.domain.model;

/// Модель статистики игрока для таблицы лидеров
public class PlayerStats
{
    public Guid UserId { get; set; }

    public string Login { get; set; }

    public double WinRatio { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public int Draws { get; set; }

    public PlayerStats()
    {
        UserId = Guid.Empty;
        Login = string.Empty;
        WinRatio = 0.0;
        Wins = 0;
        Losses = 0;
        Draws = 0;
    }
}
