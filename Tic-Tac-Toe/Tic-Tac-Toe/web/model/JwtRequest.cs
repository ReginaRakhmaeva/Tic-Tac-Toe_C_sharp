namespace Tic_Tac_Toe.web.model;

public class JwtRequest
{
    public string Login { get; set; }

    public string Password { get; set; }

    public JwtRequest()
    {
        Login = string.Empty;
        Password = string.Empty;
    }
}
