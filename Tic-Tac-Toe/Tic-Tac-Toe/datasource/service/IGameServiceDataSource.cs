using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.domain.service;

namespace Tic_Tac_Toe.datasource.service;

/// Расширенный интерфейс для datasource-слоя
public interface IGameServiceDataSource : IGameService
{
    Game? GetGame(Guid id);

    void SaveGame(Game game);
}
