using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.web.model;

namespace Tic_Tac_Toe.web.mapper;

/// Маппер для преобразования Game между domain и web слоями
public static class GameMapper
{
    /// Преобразование из web в domain
    public static Game ToDomain(GameRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return new Game
        {
            Id = request.Id,
            Board = GameBoardMapper.ToDomain(request.Board),
            MoveHistory = new List<Move>()
        };
    }

    /// Преобразование из domain в web
    public static GameResponse ToResponse(Game domain, GameStatus? status = null)
    {
        if (domain == null)
        {
            throw new ArgumentNullException(nameof(domain));
        }

        string player1Symbol = "X";
        string player2Symbol = "O";

        return new GameResponse
        {
            Id = domain.Id,
            CreatedAt = domain.CreatedAt,
            Board = GameBoardMapper.ToResponse(domain.Board),
            Status = status?.ToString() ?? "InProgress",
            Player1Id = domain.Player1Id,
            Player2Id = domain.Player2Id,
            CurrentPlayerId = domain.CurrentPlayerId,
            WinnerId = domain.WinnerId,
            GameType = (int)domain.GameType,
            Player1Symbol = player1Symbol,
            Player2Symbol = player2Symbol
        };
    }
}
