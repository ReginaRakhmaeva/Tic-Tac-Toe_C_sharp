using System.Text.Json;
using Tic_Tac_Toe.datasource.model;

namespace Tic_Tac_Toe.datasource.dbcontext.Converters;

public static class GameBoardJsonConverter
{
    public static string Serialize(GameBoardDto? board)
    {
        if (board == null || board.Board == null)
        {
            return "{\"Board\":[[0,0,0],[0,0,0],[0,0,0]]}";
        }

        int[][] boardArray = new int[3][];
        for (int i = 0; i < 3; i++)
        {
            boardArray[i] = new int[3];
            for (int j = 0; j < 3; j++)
            {
                boardArray[i][j] = board.Board[i, j];
            }
        }

        var wrapper = new { Board = boardArray };
        return JsonSerializer.Serialize(wrapper);
    }

    public static GameBoardDto Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new GameBoardDto();
        }

        try
        {
            var wrapper = JsonSerializer.Deserialize<JsonElement>(json);
            
            if (wrapper.TryGetProperty("Board", out var boardElement))
            {
                var boardArray = JsonSerializer.Deserialize<int[][]>(boardElement.GetRawText());
                
                if (boardArray != null && boardArray.Length == 3)
                {
                    int[,] board = new int[3, 3];
                    for (int i = 0; i < 3; i++)
                    {
                        if (boardArray[i] != null && boardArray[i].Length == 3)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                board[i, j] = boardArray[i][j];
                            }
                        }
                    }
                    return new GameBoardDto(board);
                }
            }
        }
        catch
        {
        }

        return new GameBoardDto();
    }
}
