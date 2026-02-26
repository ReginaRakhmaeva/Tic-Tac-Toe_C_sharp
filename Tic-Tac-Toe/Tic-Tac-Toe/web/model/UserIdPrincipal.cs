using System.Security.Claims;

namespace Tic_Tac_Toe.web.model;

/// Пользовательский principal, в котором явно хранится UUID пользователя.
public class UserIdPrincipal : ClaimsPrincipal
{
    public Guid UserId { get; }

    public UserIdPrincipal(Guid userId, ClaimsIdentity identity)
        : base(identity)
    {
        UserId = userId;
    }
}

