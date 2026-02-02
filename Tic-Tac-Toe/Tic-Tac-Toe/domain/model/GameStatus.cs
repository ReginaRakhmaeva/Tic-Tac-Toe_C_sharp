namespace Tic_Tac_Toe.domain.model;

/// Статус игры
public enum GameStatus
{
    InProgress,      // Игра продолжается (для обратной совместимости с игрой против компьютера)
    PlayerXWins,     // Победил игрок X (для обратной совместимости)
    PlayerOWins,     // Победил игрок O (для обратной совместимости)
    Draw,            // Ничья
    
    // Состояния для игры между двумя игроками
    WaitingForPlayers, // Ожидание игроков (игра создана, но второй игрок еще не присоединился)
    PlayerTurn,        // Ход игрока (текущий игрок указан в Game.CurrentPlayerId)
    PlayerWins         // Победа игрока (победитель указан в Game.WinnerId)
}
