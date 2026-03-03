using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.datasource.mapper;

namespace Tic_Tac_Toe.datasource.repository;

/// Реализация репозитория для работы с классом-хранилищем
public class GameRepository : IGameRepository
{
    private readonly GameStorage _storage;

    public GameRepository(GameStorage storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    public void Save(Game game)
    {
        if (game == null)
        {
            throw new ArgumentNullException(nameof(game));
        }

        var dto = GameMapper.ToDto(game);
        _storage.Save(dto);
    }

    public Game? Get(Guid id)
    {
        var dto = _storage.Get(id);
        if (dto == null)
        {
            return null;
        }

        return GameMapper.ToDomain(dto);
    }
}

