using Tic_Tac_Toe.domain.model;

namespace Tic_Tac_Toe.datasource.repository;

/// Интерфейс репозитория для работы с хранилищем игр
public interface IGameRepository
{
    /// Сохранить текущую игру
    void Save(Game game);

    /// Получить текущую игру по UUID
    Game? Get(Guid id);

    /// Получить доступные игры (ожидающие второго игрока)
    /// Игры, где Player1Id != null и Player2Id == null
    List<Game> GetAvailableGames();
}

