using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.datasource.model;

namespace Tic_Tac_Toe.datasource.mapper;

/// Маппер для преобразования Game между domain и datasource слоями
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
            Board = GameBoardMapper.ToDto(domain.Board),
            MoveHistory = domain.MoveHistory?.Select(m => MoveMapper.ToDto(m)).ToList() ?? new List<MoveDto>()
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
            Board = GameBoardMapper.ToDomain(dto.Board),
            MoveHistory = dto.MoveHistory?.Select(m => MoveMapper.ToDomain(m)).ToList() ?? new List<Move>()
        };

        return domain;
    }
}

