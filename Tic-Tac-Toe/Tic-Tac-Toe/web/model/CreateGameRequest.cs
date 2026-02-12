namespace Tic_Tac_Toe.web.model;

/// Модель запроса для создания новой игры
public class CreateGameRequest
{
    public string GameType { get; set; }

    public Guid? Player2Id { get; set; }

    public string FirstMove { get; set; }

    public CreateGameRequest()
    {
        GameType = "computer";
        Player2Id = null;
        FirstMove = "player";
    }
}
