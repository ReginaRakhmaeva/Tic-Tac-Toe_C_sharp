using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.datasource.model;

namespace Tic_Tac_Toe.datasource.mapper;

/// Маппер для преобразования Move между domain и datasource слоями
public static class MoveMapper
{
    public static MoveDto ToDto(Move domain)
    {
        if (domain == null)
        {
            throw new ArgumentNullException(nameof(domain));
        }

        return new MoveDto(domain.Row, domain.Col, domain.Player);
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
