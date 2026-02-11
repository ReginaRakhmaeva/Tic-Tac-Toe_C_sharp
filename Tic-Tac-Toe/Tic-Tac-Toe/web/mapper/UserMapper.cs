using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.web.model;

namespace Tic_Tac_Toe.web.mapper;

/// Маппер для преобразования User между domain и web слоями
public static class UserMapper
{
    /// Преобразование из domain в web
    public static UserResponse ToResponse(User domain)
    {
        if (domain == null)
        {
            throw new ArgumentNullException(nameof(domain));
        }

        return new UserResponse(domain.Id, domain.Login);
    }
}
