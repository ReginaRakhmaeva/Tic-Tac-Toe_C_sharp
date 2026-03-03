using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.web.model;

namespace Tic_Tac_Toe.web.mapper;

/// Маппер для преобразования PlayerStats в LeaderboardEntryResponse
public static class LeaderboardMapper
{
    /// Преобразование из domain в web
    public static LeaderboardEntryResponse ToResponse(PlayerStats domain)
    {
        if (domain == null)
        {
            throw new ArgumentNullException(nameof(domain));
        }

        return new LeaderboardEntryResponse
        {
            UserId = domain.UserId,
            Login = domain.Login,
            WinRatio = domain.WinRatio,
            Wins = domain.Wins,
            Losses = domain.Losses,
            Draws = domain.Draws
        };
    }
}
