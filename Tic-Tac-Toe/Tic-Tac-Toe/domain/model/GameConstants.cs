namespace Tic_Tac_Toe.domain.model;

public static class GameConstants
{
    public static readonly Guid ComputerId = new Guid("00000000-0000-0000-0000-000000000001");

    public static bool IsComputer(Guid? playerId)
    {
        return playerId.HasValue && playerId.Value == ComputerId;
    }
}
