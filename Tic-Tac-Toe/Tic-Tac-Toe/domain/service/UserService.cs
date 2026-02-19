using Tic_Tac_Toe.domain.model;
using Tic_Tac_Toe.datasource.repository;

namespace Tic_Tac_Toe.domain.service;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public User? CreateUser(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            throw new ArgumentException("Login cannot be null or empty", nameof(login));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        }

        if (_repository.ExistsByLogin(login))
        {
            return null;
        }

        var user = new User(Guid.NewGuid(), login, password);
        _repository.Save(user);

        return user;
    }

    public User? GetUserByLogin(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            throw new ArgumentException("Login cannot be null or empty", nameof(login));
        }

        return _repository.GetByLogin(login);
    }

    public User? GetUserById(Guid id)
    {
        return _repository.GetById(id);
    }

    public bool UserExists(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            return false;
        }

        return _repository.ExistsByLogin(login);
    }

    public bool VerifyPassword(User user, string password)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }
        return user.Password == password;
    }
}
