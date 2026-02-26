namespace Tic_Tac_Toe.web.model;

public class RefreshJwtRequest
{
    public string RefreshToken { get; set; }

    public RefreshJwtRequest()
    {
        RefreshToken = string.Empty;
    }
}
