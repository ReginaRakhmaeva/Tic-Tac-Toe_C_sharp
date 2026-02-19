namespace Tic_Tac_Toe.domain.model;

/// Статус игры
public enum GameStatus
{
    InProgress,      
    PlayerXWins,  
    PlayerOWins, 
    Draw, 
    
    WaitingForPlayers,
    PlayerTurn,
    PlayerWins,
    PlayerLeft  
}
