using Tic_Tac_Toe.domain.model;

namespace Tic_Tac_Toe.datasource.repository;

/// Интерфейс репозитория для работы с пользователями
public interface IUserRepository
{
    void Save(User user);

    User? GetById(Guid id);

    User? GetByLogin(string login);

    bool ExistsByLogin(string login);
}
