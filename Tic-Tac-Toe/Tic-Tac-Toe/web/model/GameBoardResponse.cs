namespace Tic_Tac_Toe.web.model;

/// Модель игрового поля для ответа клиенту
public class GameBoardResponse
{
    public int[][] Board { get; set; }

    public GameBoardResponse()
    {
        Board = new int[3][];
        for (int i = 0; i < 3; i++)
        {
            Board[i] = new int[3];
        }
    }
}
