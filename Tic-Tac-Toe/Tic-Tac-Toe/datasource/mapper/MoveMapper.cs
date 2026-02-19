using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.datasource.model;

namespace Tic_Tac_Toe.datasource.mapper;

public static class MoveMapper
{
    public static MoveDto ToDto(Move domain, Guid gameId)
    {
        if (domain == null)
        {
            throw new ArgumentNullException(nameof(domain));
        }
        return new MoveDto(domain.Row, domain.Col, domain.Player)
        {
            GameId = gameId
        };
    }

    public static Move ToDomain(MoveDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }
        return new Move(dto.Row, dto.Col, dto.Player);
    }
}
