using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.web.model;

namespace Tic_Tac_Toe.web.mapper;

/// Маппер для преобразования GameBoard между domain и web слоями
public static class GameBoardMapper
{
    public static GameBoard ToDomain(GameBoardRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Board == null || request.Board.Length != 3)
        {
            throw new ArgumentException("Board must be a 3x3 matrix");
        }

        int[,] boardArray = new int[3, 3];
        for (int i = 0; i < 3; i++)
        {
            if (request.Board[i] == null || request.Board[i].Length != 3)
            {
                throw new ArgumentException("Board must be a 3x3 matrix");
            }
            for (int j = 0; j < 3; j++)
            {
                boardArray[i, j] = request.Board[i][j];
            }
        }

        return new GameBoard(boardArray);
    }

    public static GameBoardResponse ToResponse(GameBoard domain)
    {
        if (domain == null)
        {
            throw new ArgumentNullException(nameof(domain));
        }

        int[,] boardArray = domain.GetBoard();
        int[][] boardJagged = new int[3][];
        for (int i = 0; i < 3; i++)
        {
            boardJagged[i] = new int[3];
            for (int j = 0; j < 3; j++)
            {
                boardJagged[i][j] = boardArray[i, j];
            }
        }

        return new GameBoardResponse
        {
            Board = boardJagged
        };
    }
}
