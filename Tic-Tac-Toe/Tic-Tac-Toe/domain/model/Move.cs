namespace Tic_Tac_Toe.domain.model;

/// Модель хода в игре
public class Move
{
    public int Row { get; set; }
    public int Col { get; set; }
    public int Player { get; set; } 

    public Move(int row, int col, int player)
    {
        Row = row;
        Col = col;
        Player = player;
    }
}
