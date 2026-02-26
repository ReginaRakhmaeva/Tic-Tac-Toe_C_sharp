using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.datasource.model;

namespace Tic_Tac_Toe.datasource.mapper;

public static class GameMapper
{
    public static GameDto ToDto(Game domain)
    {
        if (domain == null)
        {
            throw new ArgumentNullException(nameof(domain));
        }

        var dto = new GameDto
        {
            Id = domain.Id,
            UserId = domain.UserId,
            GameType = (int)domain.GameType,
            Player1Id = domain.Player1Id,
            Player2Id = domain.Player2Id,
            CurrentPlayerId = domain.CurrentPlayerId,
            WinnerId = domain.WinnerId,
            CreatedAt = domain.CreatedAt,
            Board = GameBoardMapper.ToDto(domain.Board),
            Moves = domain.MoveHistory?.Select(m => MoveMapper.ToDto(m, domain.Id)).ToList() ?? new List<MoveDto>()
        };

        return dto;
    }

    public static Game ToDomain(GameDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        var domain = new Game
        {
            Id = dto.Id,
            UserId = dto.UserId,
            GameType = (GameType)dto.GameType,
            Player1Id = dto.Player1Id,
            Player2Id = dto.Player2Id,
            CurrentPlayerId = dto.CurrentPlayerId,
            WinnerId = dto.WinnerId,
            CreatedAt = dto.CreatedAt,
            Board = GameBoardMapper.ToDomain(dto.Board),
            MoveHistory = dto.Moves?.Select(m => MoveMapper.ToDomain(m)).ToList() ?? new List<Move>()
        };
        return domain;
    }
}

