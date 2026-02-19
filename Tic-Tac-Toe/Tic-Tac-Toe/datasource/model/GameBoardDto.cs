namespace Tic_Tac_Toe.datasource.model;

public class GameBoardDto
{
    public int[,] Board { get; set; }

    public GameBoardDto()
    {
        Board = new int[3, 3];
    }

    public GameBoardDto(int[,] board)
    {
        if (board == null || board.GetLength(0) != 3 || board.GetLength(1) != 3)
        {
            throw new ArgumentException("Board must be a 3x3 matrix");
        }
        
        Board = new int[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Board[i, j] = board[i, j];
            }
        }
    }
}

