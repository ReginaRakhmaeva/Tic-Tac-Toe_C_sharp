using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.domain.service;

namespace Tic_Tac_Toe.datasource.service;

/// Расширенный интерфейс для datasource-слоя (наследует domain + добавляет персистентность)
public interface IGameServiceDataSource : IGameService
{
    /// Методы персистентности
    Game? GetGame(Guid id);

    void SaveGame(Game game);

    void DeleteGame(Guid id);

    List<Game> GetAvailableGames();

    void DeleteInactiveGamesByPlayer1Id(Guid player1Id);

    List<Game> GetCompletedGamesByUserId(Guid userId);
}
