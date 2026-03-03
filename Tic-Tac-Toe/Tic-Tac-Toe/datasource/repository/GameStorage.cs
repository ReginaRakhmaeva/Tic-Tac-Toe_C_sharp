using System.Collections.Concurrent;
using Tic_Tac_Toe.datasource.model;

namespace Tic_Tac_Toe.datasource.repository;

/// Класс-хранилище для хранения текущих игр с использованием потокобезопасных коллекций
public class GameStorage
{
    private readonly ConcurrentDictionary<Guid, GameDto> _games;

    public GameStorage()
    {
        _games = new ConcurrentDictionary<Guid, GameDto>();
    }

    public void Save(GameDto game)
    {
        if (game == null)
        {
            throw new ArgumentNullException(nameof(game));
        }

        _games.AddOrUpdate(game.Id, game, (key, oldValue) => game);
    }

    public GameDto? Get(Guid id)
    {
        _games.TryGetValue(id, out GameDto? game);
        return game;
    }

    public bool Remove(Guid id)
    {
        return _games.TryRemove(id, out _);
    }

    public bool Contains(Guid id)
    {
        return _games.ContainsKey(id);
    }

    public IEnumerable<GameDto> GetAll()
    {
        return _games.Values;
    }
}

