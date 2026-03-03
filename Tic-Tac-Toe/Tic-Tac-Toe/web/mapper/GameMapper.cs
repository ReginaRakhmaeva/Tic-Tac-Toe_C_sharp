using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.web.model;

namespace Tic_Tac_Toe.web.mapper;

/// Маппер для преобразования Game между domain и web слоями
public static class GameMapper
{
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

    public static GameResponse ToResponse(Game domain, GameStatus? status = null)
    {
        if (domain == null)
        {
            throw new ArgumentNullException(nameof(domain));
        }

        return new GameResponse
        {
            Id = domain.Id,
            Board = GameBoardMapper.ToResponse(domain.Board),
            Status = status?.ToString() ?? "InProgress"
        };
    }
}
