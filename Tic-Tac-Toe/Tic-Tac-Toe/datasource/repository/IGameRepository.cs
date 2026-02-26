using Tic_Tac_Toe.domain.model;

namespace Tic_Tac_Toe.datasource.repository;

public interface IGameRepository
{
    void Save(Game game);
    Game? Get(Guid id);
    List<Game> GetAvailableGames();
    void Delete(Guid id);
    List<Game> GetInactiveGamesByPlayer1Id(Guid player1Id);
    void DeleteInactiveGamesByPlayer1Id(Guid player1Id);
    List<Game> GetGamesByUserId(Guid userId);
}

