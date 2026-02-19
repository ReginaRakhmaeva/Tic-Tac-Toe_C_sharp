namespace Tic_Tac_Toe.domain.model;

public class User
{
    public Guid Id { get; set; }

    public string Login { get; set; }

    public string Password { get; set; }

    public User()
    {
        Id = Guid.NewGuid();
        Login = string.Empty;
        Password = string.Empty;
    }

    public User(Guid id, string login, string password)
    {
        Id = id;
        Login = login ?? string.Empty;
        Password = password ?? string.Empty;
    }
}
