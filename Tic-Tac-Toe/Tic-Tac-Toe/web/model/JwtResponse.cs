namespace Tic_Tac_Toe.web.model;

public class JwtResponse
{
    public string Type { get; set; }

    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }

    public JwtResponse()
    {
        Type = string.Empty;
        AccessToken = string.Empty;
        RefreshToken = string.Empty;
    }
}
