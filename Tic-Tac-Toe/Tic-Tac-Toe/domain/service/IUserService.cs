using Tic_Tac_Toe.domain.model;

namespace Tic_Tac_Toe.domain.service;

public interface IUserService
{
    User? CreateUser(string login, string password);

    User? GetUserByLogin(string login);

    User? GetUserById(Guid id);

    bool UserExists(string login);

    bool VerifyPassword(User user, string password);
}
