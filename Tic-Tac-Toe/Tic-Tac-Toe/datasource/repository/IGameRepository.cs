using Tic_Tac_Toe.domain.model;

namespace Tic_Tac_Toe.datasource.repository;

public interface IGameRepository
{
    void Save(Game game);

    Game? Get(Guid id);
}

