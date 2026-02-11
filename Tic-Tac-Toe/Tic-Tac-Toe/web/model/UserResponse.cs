namespace Tic_Tac_Toe.web.model;

/// Модель ответа с информацией о пользователе
public class UserResponse
{
    /// Уникальный идентификатор пользователя (UUID)
    public Guid Id { get; set; }

    /// Логин пользователя
    public string Login { get; set; }

    public UserResponse()
    {
        Id = Guid.Empty;
        Login = string.Empty;
    }

    public UserResponse(Guid id, string login)
    {
        Id = id;
        Login = login ?? string.Empty;
    }
}
